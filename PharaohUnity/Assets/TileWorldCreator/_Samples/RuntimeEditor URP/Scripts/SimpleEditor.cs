using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/*
	SIMPLE RUNTIME EDITOR
	--------------------
	
	This is a simple demonstration on how to modify and build a map at runtime as well as how to save and load "painted" maps.
	It also demonstrates how to create new blueprint layers with modifiers and build layers at runtime.
	Please note that for demo purpose, serialization is done using Unity's JsonUtility class. 
	It is recommended to use a proper serialization solution like Newtonsoft Json or Odin Serializer.
*/

namespace GiantGrey.TileWorldCreator.Samples
{
	public class SimpleEditor : MonoBehaviour
	{
		/// Public fields
#region Public fields
		public TileWorldCreatorManager tileWorldCreator;
		// public Configuration configurationA;
		// public Configuration configurationB;

		[Header("Tile Presets")]
		[TilePresetPopup("Water Tile Preset")]
		public TilePreset waterTilePreset;

		[TilePresetPopup("Sand Tile Preset")]
		public TilePreset sandTilePreset;

		[Header("Prefabs")]
		public GameObject treePrefabA;
		public GameObject treePrefabB;

		
		[Header("Editor objects")]
		public Texture2D headerLogo;
		public Camera editorCamera;
		public UIDocument uIDocument;
		public GameObject cursorObject;
		public GameObject addCellObject;
		public GameObject removeCellObject;


#endregion
		
#region Private fields
		/// Private fields
		private bool leftDrag;
		private bool rightDrag;
		private bool removeCells;
		private string selectedLayer;
		private int selectedLayerIndex;
		private HashSet<GameObject> paintedCells = new HashSet<GameObject>();
		private int cellSize = 1;
		private Vector3 currentPosition;
		private Vector3 lastPosition;
		private float cameraZoom = 12f;
		private int brushSize = 1;
		private HashSet<Vector2> positions = new HashSet<Vector2>();
		private HashSet<Vector3> cellPositions = new HashSet<Vector3>();
		private Subtract treesSubtractModifier;

		enum PaintMode
		{
			Add,
			Remove
		}

		private PaintMode paintMode;
#endregion

#region SerializableClass
		/// <summary>
		/// Class used for saving and loading "painted" maps
		/// </summary>
		[System.Serializable]
		public class SavedMap
		{
			[SerializeField]
			public string layerName;
			[SerializeField]
			public List<Vector2> positions = new List<Vector2>();

			public SavedMap(string _layerName, HashSet<Vector2> _positions)
			{
				layerName = _layerName;
				positions = _positions.ToList();
			}
		}

		[System.Serializable]
		public class JSONObject
		{
			public List<SavedMap> savedMaps = new List<SavedMap>();
		}

		JSONObject jSONObject;
#endregion

		// Register to blueprint layers ready event
		void OnEnable()
		{
			tileWorldCreator.OnBlueprintLayersReady += BuildMap;

			#if ENABLE_INPUT_SYSTEM
			mouseLeftDown.Enable();
			mouseLeftDown.performed += OnMouseLeftDown;
			mouseLeftDown.canceled += OnMouseLeftUp;

			mouseRightDown.Enable();
			mouseRightDown.performed += OnMouseRightDown;
			mouseRightDown.canceled += OnMouseRightUp;
			#endif
		}
		
		// Deregister from blueprint layers ready event
		void OnDisable()
		{
			tileWorldCreator.OnBlueprintLayersReady -= BuildMap;

			#if ENABLE_INPUT_SYSTEM
			mouseLeftDown.Disable();
			mouseLeftDown.performed -= OnMouseLeftDown;
			mouseLeftDown.canceled -= OnMouseLeftUp;

			mouseRightDown.Disable();
			mouseRightDown.performed -= OnMouseRightDown;
			mouseRightDown.canceled -= OnMouseRightUp;
			#endif
		}

		
		// Build map after blueprint map generation is complete
		void BuildMap()
		{
			tileWorldCreator.ExecuteBuildLayers();
		}
		
		void Start()
		{
			// Clear all layers which has been created during runtime to make sure we start with a clean configuration.
			// We do this because when creating new layers in the Unity editor at runtime, scriptable objects are being serialized.
			// So every time we run the game, new layers will be added to the configuration which already contains layers from the last run.
			// To prevent this we clear the configuration before running the game and creating new layers.
			// In a build, this won't happen as scriptable objects which were created at runtime won't be serialized. So ClearConfiguration will then do nothing.
			tileWorldCreator.ClearConfiguration();

			// tileWorldCreator.ClearAllBlueprintLayers();

			// Here we create a new serializable json class which will be used for saving and loading "painted" maps.
			jSONObject = new JSONObject();

			BuildUI();
		}



#region HandleInput


		// Handle Input
	    void Update()
	    {
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			#if !ENABLE_INPUT_SYSTEM
		    Ray _ray = editorCamera.ScreenPointToRay(Input.mousePosition);
		    #else
			Ray _ray = editorCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			#endif

			RaycastHit _hit = new RaycastHit();
	
		    if (Physics.Raycast(_ray, out _hit, 1000))
		    {
				// Get the relative grid position from the world position.
				// We will later add the relative grid position including the offsets based on the brush size to the positions hashset 
				// which will be then passed to the AddCellsToLayer method on mouse up
		    	var _relativeGridPos = tileWorldCreator.GetRelativeGridPosition(_hit.point);

		    	var _xPos = Mathf.RoundToInt(_hit.point.x / cellSize);
		    	var _zPos = Mathf.RoundToInt(_hit.point.z / cellSize);
		    	
		    	cursorObject.transform.position = new Vector3(_xPos, (selectedLayerIndex * 1) + 0.6f, _zPos);
			    currentPosition = new Vector3(_xPos, (selectedLayerIndex * 1) + 0.6f, _zPos);

				int _halfSize = Mathf.CeilToInt(brushSize * 0.5f);
                float _radius = brushSize * 0.5f;
				
			    
			    // Left mouse button add cell position to layer
				#if !ENABLE_INPUT_SYSTEM
			    if (Input.GetMouseButton(0))
			    {
					leftDrag = true;
				}
				else
				{
					leftDrag = false;
				}
				#endif

				if (leftDrag)
				{
					paintMode = PaintMode.Add;

					for (int x = -_halfSize; x <= _halfSize; x ++)
                	{
						for (int y = -_halfSize; y <= _halfSize; y ++)
						{
                        	Vector2 _pos = new Vector2(_relativeGridPos.x + x, _relativeGridPos.y + y);
							Vector3 _cellPos = new Vector3(_xPos + x, (selectedLayerIndex * 1) + 0.6f, _zPos + y);

							if (!tileWorldCreator.IsRelativePositionOverGrid(_pos))
								continue;
                        
							if (Vector2.Distance(_relativeGridPos, _pos) <= _radius)
							{
								positions.Add(_pos);
								cellPositions.Add(_cellPos);
							}
						}
					}

					foreach (var _c in cellPositions)
					{
						if (!paintedCells.Any(x => x != null && x.transform.position == _c))
						{
							var _cellObject = Instantiate(addCellObject, _c, Quaternion.identity);
							paintedCells.Add(_cellObject);
						}
					}	
			    }
			    
				// Right mouse button remove cell position from layer
				#if !ENABLE_INPUT_SYSTEM
			    if (Input.GetMouseButton(1))
			    {
					rightDrag = true;
				}
				else
				{
					rightDrag = false;
				}
				#endif

				if (rightDrag)
				{
		
					paintMode = PaintMode.Remove;

					for (int x = -_halfSize; x <= _halfSize; x ++)
                	{
						for (int y = -_halfSize; y <= _halfSize; y ++)
						{
                        	Vector2 _pos = new Vector2(_relativeGridPos.x + x, _relativeGridPos.y + y);
							Vector3 _cellPos = new Vector3(_xPos + x, (selectedLayerIndex * 1) + 0.6f, _zPos + y);

							if (!tileWorldCreator.IsRelativePositionOverGrid(_pos))
								continue;
                        
							if (Vector2.Distance(_relativeGridPos, _pos) <= _radius)
							{
								positions.Add(_pos);
								cellPositions.Add(_cellPos);
							}
						}
					}

			    	
					foreach (var _c in cellPositions)
					{
						// Check if gameobject position is already in painted cells
						// If so, then remove it
						if (!paintedCells.Any(x =>  x != null && x.transform.position == _c))
						{
							var _cellObject = Instantiate(removeCellObject, _c, Quaternion.identity);
							paintedCells.Add(_cellObject);
						}
					}
			    }
			}
			    
			#if !ENABLE_INPUT_SYSTEM
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
			{
				leftDrag = false;
				rightDrag = false;
				removeCells = true;
			}
			#endif

			if (removeCells)
			{

				foreach(var _cellObject in paintedCells)
				{
					Destroy(_cellObject);
				}

				paintedCells.Clear();
				paintedCells.TrimExcess();

				if (string.IsNullOrEmpty(selectedLayer))
					return;

				// Depending on the paint mode we either add or remove cells. Positions is a HashSet which contains all painted positions
				if (paintMode == PaintMode.Add)
				{
					tileWorldCreator.AddCellsToLayer(selectedLayer, positions);
				}
				else
				{
					tileWorldCreator.RemoveCellsFromLayer(selectedLayer, positions);
				}


				if (jSONObject.savedMaps.Count > 0)
				{
					tileWorldCreator.ExecuteBlueprintLayers();
				}

				positions.Clear();
				positions.TrimExcess();

				cellPositions.Clear();
				cellPositions.TrimExcess();

				removeCells = false;
			}
	    }
#endregion  

#region SaveAndLoad
		void SaveMap()
		{
			JSONObject savedMaps = new JSONObject();
			savedMaps.savedMaps = new List<SavedMap>();

			// First get all positions from blueprint layers
			for (int i = 0; i < tileWorldCreator.configuration.blueprintLayerFolders.Count; i ++)
			{
				for (int j = 0; j < tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers.Count; j ++)
				{
					// Skip trees layer
					if (tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers[j].layerName == "Trees")
						continue;

					var _positions = tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers[j].GetAllCellPositions(new HashSet<Vector2>());

					savedMaps.savedMaps.Add(new SavedMap(tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers[j].layerName, _positions));
				}
			}

			// Serialized SavedMap class
			string _json = JsonUtility.ToJson(savedMaps, true);
			System.IO.File.WriteAllText(Application.persistentDataPath + "/TWC_RuntimeEditorSample_SavedMap.json", _json);
		}

		void LoadMap()
		{
			if (!System.IO.File.Exists(Application.persistentDataPath + "/TWC_RuntimeEditorSample_SavedMap.json"))
			{
				return;
			}
			// Load SavedMap class
			string _json = System.IO.File.ReadAllText(Application.persistentDataPath + "/TWC_RuntimeEditorSample_SavedMap.json");
			var _jsonObject = JsonUtility.FromJson<JSONObject>(_json);

			// Iterate through savedMaps, get blueprint layer by name and add cells to layer
			for (int i = 0; i < _jsonObject.savedMaps.Count; i ++)
			{
				// var _layer = tileWorldCreator.GetBlueprintLayer(_jsonObject.savedMaps[i].layerName);
				AddLayer();

				var _layer = tileWorldCreator.GetBlueprintLayer(_jsonObject.savedMaps[i].layerName);

				if (_layer != null)
				{
					_layer.ClearLayer();
					_layer.AddCells(_jsonObject.savedMaps[i].positions.ToHashSet());
				}
			}

			// Regenerate complete map
			tileWorldCreator.GenerateCompleteMap();
		}

		JSONObject BuildJSONObject()
		{
			// JSONObject savedMaps = new JSONObject();
			// savedMaps.savedMaps = new List<SavedMap>();

			// First get all positions from blueprint layers
			for (int i = 0; i < tileWorldCreator.configuration.blueprintLayerFolders.Count; i ++)
			{
				for (int j = 0; j < tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers.Count; j ++)
				{
					var _positions = tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers[j].GetAllCellPositions(new HashSet<Vector2>());

					// Add positions to the savedMaps serializable class
					jSONObject.savedMaps.Where(x => x.layerName == tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers[j].layerName).First().positions = _positions.ToList();
					// jSONObject.savedMaps.Add(new SavedMap(tileWorldCreator.configuration.blueprintLayerFolders[i].blueprintLayers[j].layerName, _positions));
				}
			}

			return jSONObject;
		}

#endregion

#region AddLayer
		/// <summary>
		/// Add new blueprint and build layer. 
		/// Make sure that layer 0 has the water tile preset while every other layer has the cliff tile preset.
		/// Additionally layer 1 creates an additional Trees blueprint layer with some additional modifiers. 
		/// </summary>
		void AddLayer()
		{
			// Add new blueprint layer
			var _blueprintLayer = tileWorldCreator.AddNewBlueprintLayer("New Layer " + jSONObject.savedMaps.Count);

			// And also add new build layer
			var _buildLayer = tileWorldCreator.AddNewBuildLayer<TilesBuildLayer>("New Layer " + jSONObject.savedMaps.Count);
			// Set water preset for the very first layer
			if (jSONObject.savedMaps.Count == 0)
			{
				_buildLayer.SetNewTilePreset(waterTilePreset);
			}
			// any other layer gets the sand preset, also set the y offset based on the amount of layers we've added to our
			// serializable object.
			else
			{
				if (jSONObject.savedMaps.Count == 1)
				{
					// Only add layer if Trees layer doesn't exist.
					if (tileWorldCreator.GetBlueprintLayer("Trees") == null)
					{
						// Add tree layer
						var _blueprintTreeLayer = tileWorldCreator.AddNewBlueprintLayer("Trees");
						// Add second blueprint layer to tree blueprint layer
						var _addModifier = _blueprintTreeLayer.AddModifier<Add>();
						_addModifier.SetLayer(_blueprintLayer);
						// Add shrink modifier
						var _shrinkModifier = _blueprintTreeLayer.AddModifier<Shrink>();
						_shrinkModifier.shrinkCount = 1;
						// Add random select modifier
						var _randomSelectModifier = _blueprintTreeLayer.AddModifier<Select>();
						_randomSelectModifier.selectionType = Select.SelectionType.Random;
						_randomSelectModifier.randomSelectionWeight = 0.02f;

						// Add build layer which instantiates the trees
						var _buildTreeLayer = tileWorldCreator.AddNewBuildLayer<ObjectBuildLayer>("Trees");
						_buildTreeLayer.SetBlueprintLayer(_blueprintTreeLayer);
						_buildTreeLayer.AddPrefabObject(treePrefabA, Random.Range(0.3f, 0.6f));
						_buildTreeLayer.AddPrefabObject(treePrefabB, Random.Range(0.2f, 0.4f));
						// Set random scale and random rotation for trees layer
						_buildTreeLayer.useRndScale = true;
						_buildTreeLayer.uniformScale = true;
						_buildTreeLayer.uniformMinScale = 12f;
						_buildTreeLayer.uniformMaxScale = 22f;
						_buildTreeLayer.useRndRotation = true;
						_buildTreeLayer.objectRNDMinRotation = new Vector3(-10, -360, -10);
						_buildTreeLayer.objectRNDMaxRotation = new Vector3(10, 360, 10);
						_buildTreeLayer.layerOffset = new Vector3(0, 1.2f, 0);
					}
				}

				if (jSONObject.savedMaps.Count > 1)
				{	
					// Add additional subtract folder modifier to trees layer
					// to make sure, trees won't be placed where layer 2 cliffs are.
					var _trees = tileWorldCreator.GetBlueprintLayer("Trees");
					// keep track of subtract modifier, so we can remove it later
					treesSubtractModifier = _trees.AddModifier<Subtract>();
					treesSubtractModifier.SetLayer(tileWorldCreator.GetBlueprintLayer("New Layer " + (jSONObject.savedMaps.Count)));
				}

				// Set sand tile preset for every other build layer.
				_buildLayer.SetNewTilePreset(sandTilePreset);
				// Set y offset one higher than the last build layer
				_buildLayer.layerYOffset = jSONObject.savedMaps.Count;
			}
			
			// Assign newly created blueprint layer to build layer
			_buildLayer.SetBlueprintLayer(_blueprintLayer);

			// Add new layer to the serialized object savedmaps list so we can restore it back after saving and loading.
			jSONObject.savedMaps.Add(new SavedMap("New Layer " + jSONObject.savedMaps.Count, new HashSet<Vector2>()));

			// Rebuild UI
			BuildUI();

			// Set selected layer to newly created layer
			selectedLayer = _blueprintLayer.layerName;
		}

#endregion

#region RemoveLayer
		void RemoveLayer(int _index)
		{
			tileWorldCreator.RemoveBlueprintLayer(jSONObject.savedMaps[_index].layerName);
			tileWorldCreator.RemoveBuildLayer(jSONObject.savedMaps[_index].layerName);

			jSONObject.savedMaps.RemoveAt(_index);

			// Select last layer
			if (jSONObject.savedMaps.Count > 0)
			{
				selectedLayer = jSONObject.savedMaps[jSONObject.savedMaps.Count - 1].layerName;
				selectedLayerIndex = jSONObject.savedMaps.Count - 1;
			}

			// if there's only water layer left, remove tree layer as well
			if (jSONObject.savedMaps.Count == 1)
			{
				tileWorldCreator.RemoveBlueprintLayer("Trees");
				tileWorldCreator.RemoveBuildLayer("Trees");
			}

			if (jSONObject.savedMaps.Count < 3)
			{
				// remove subtract modifier from trees layer
				var _treeLayer = tileWorldCreator.GetBlueprintLayer("Trees");
				if (_treeLayer != null && treesSubtractModifier != null)
				{
					_treeLayer.RemoveModifier<Subtract>(treesSubtractModifier);
				}
			}

			BuildUI();
		}
#endregion

#region Switching configurations 
		/// <summary>
		/// This is a simple demonstration on how to switch between configurations
		/// It is not used in the actual runtime editor
		/// </summary>
		/// <param name="_configuration"></param>
		// void SwitchToConfiguration(Configuration _configuration)
		// {
		// 	// Before switching, temporarily store current painted cells so
		// 	// we can reassign them to the new configuration
		// 	var _jsonObject = BuildJSONObject();

		// 	// Switch to new configuration
		// 	tileWorldCreator.configuration = _configuration;
			
		// 	// Reset new configuration
		// 	tileWorldCreator.ResetConfiguration();

		// 	// Assign temporary painted cells to layers of new configuration
		// 	for (int i = 0; i < _jsonObject.savedMaps.Count; i ++)
		// 	{
		// 		var _tmpLayer = _jsonObject.savedMaps[i];
		// 		var _layer = tileWorldCreator.GetBlueprintLayer(_tmpLayer.layerName);
		// 		if (_layer != null)
		// 		{
		// 			_layer.AddCells(_tmpLayer.positions.ToHashSet());
		// 		}
		// 	}

		// 	// Rebuild map
		// 	tileWorldCreator.GenerateCompleteMap();
		// }
#endregion

#region UI
		// Simple editor UI
		void BuildUI()
		{
			uIDocument.rootVisualElement.Clear();

			var _root = new VisualElement();
			var _ui = new VisualElement();
			
			_ui.style.position = Position.Absolute;
			_ui.style.left = 20;
			_ui.style.top = 20;
			_ui.style.minWidth = 200;
			_ui.style.minHeight = 100;
			_ui.style.backgroundColor = Color.white;
			_ui.style.borderTopLeftRadius = 10;
			_ui.style.borderTopRightRadius = 10;
			_ui.style.borderBottomLeftRadius = 10;
			_ui.style.borderBottomRightRadius = 10;
			_ui.style.paddingBottom = 10;
			_ui.style.paddingLeft = 10;
			_ui.style.paddingRight = 10;
			_ui.style.paddingTop= 10;

			var _header = new VisualElement();
			_header.style.backgroundImage = headerLogo;
			_header.style.width = 300;
			_header.style.height = 100;
			_header.style.marginBottom = 10;
			_ui.Add(_header);

			var _saveLoadContainer = new VisualElement();
			_saveLoadContainer.style.flexDirection = FlexDirection.Row;

			var _saveButton = new Button();
			_saveButton.text = "Save";
			_saveButton.style.flexGrow = 1;
			_saveButton.style.height = 20;
			_saveButton.RegisterCallback<ClickEvent>(evt => 
			{
				SaveMap();
			}); 

			var _loadButton = new Button();
			_loadButton.text = "Load";
			_loadButton.style.flexGrow = 1;
			_loadButton.style.height = 20;
			_loadButton.RegisterCallback<ClickEvent>(evt => 
			{
				LoadMap();
			}); 

			_saveLoadContainer.Add(_saveButton);
			_saveLoadContainer.Add(_loadButton);

			_ui.Add(_saveLoadContainer);

			// SWITCH CONFIGURATION UI
			//--------------------------------------------------------------------------------------
			// var _configurationsContainer = new VisualElement();
			// _configurationsContainer.style.flexDirection = FlexDirection.Row;

			// var _switchConfigurationLabel = new Label();
			// _switchConfigurationLabel.text = "Switch Configuration";
			// _switchConfigurationLabel.style.marginTop = 5;

			// var _configA = new Button();
			// _configA.text = "Config A";
			// _configA.style.flexGrow = 1;
			// _configA.RegisterCallback<ClickEvent>(evt => 
			// {
			// 	SwitchToConfiguration(configurationA);
			// });

			// var _configB = new Button();
			// _configB.text = "Config B";
			// _configB.style.flexGrow = 1;
			// _configB.RegisterCallback<ClickEvent>(evt => 
			// {
			// 	SwitchToConfiguration(configurationB);
			// });

			// _configurationsContainer.Add(_switchConfigurationLabel);
			// _configurationsContainer.Add(_configA);
			// _configurationsContainer.Add(_configB);

			// _ui.Add(_configurationsContainer);
			//--------------------------------------------------------------------------------------

			var _camerazoomLabel = new Label("Camera Zoom");
			_camerazoomLabel.style.marginTop = 5;
			var _camerazoom = new Slider(5, 25);
			_camerazoom.value = cameraZoom;
			_camerazoom.RegisterValueChangedCallback((evt) => 
			{
				cameraZoom = evt.newValue;
				editorCamera.orthographicSize = cameraZoom;
			});

			_ui.Add(_camerazoomLabel);
			_ui.Add(_camerazoom);

			var _brushSizeLabel = new Label("Brush Size");
			var _brushSize = new Slider(1, 10);
			_brushSize.value = brushSize;
			_brushSize.RegisterValueChangedCallback((evt) => 
			{
				brushSize = (int)evt.newValue;
			});

			_ui.Add(_brushSizeLabel);
			_ui.Add(_brushSize);

			var _layersLabel = new Label("Layers");
			_ui.Add(_layersLabel);

			for (int l = 0; l < jSONObject.savedMaps.Count; l ++)
			{
				// skip tree layer as this should not be paintable.
				if (jSONObject.savedMaps[l].layerName == "Trees") continue;

				var _index = l;

				var _layerHorizontalContainer = new VisualElement();
				_layerHorizontalContainer.style.flexDirection = FlexDirection.Row;
				_layerHorizontalContainer.style.borderLeftWidth = 2;
				_layerHorizontalContainer.style.borderRightWidth = 2;
				_layerHorizontalContainer.style.borderBottomWidth = 2;
				_layerHorizontalContainer.style.borderTopWidth = 2;
				_layerHorizontalContainer.style.borderBottomColor = Color.black;
				_layerHorizontalContainer.style.borderTopColor = Color.black;
				_layerHorizontalContainer.style.borderLeftColor = Color.black;
				_layerHorizontalContainer.style.borderRightColor = Color.black;
				_layerHorizontalContainer.style.paddingBottom = 5;
				_layerHorizontalContainer.style.paddingLeft = 5;
				_layerHorizontalContainer.style.paddingRight = 5;
				_layerHorizontalContainer.style.paddingTop= 5;
				
				var _layer = new Button();
				_layer.style.minWidth = 100;
				_layer.text =  jSONObject.savedMaps[l].layerName;

				// Set selected layer name
				_layer.RegisterCallback<ClickEvent>((evt) => 
				{
					_layer.style.backgroundColor = Color.green;

					selectedLayer = jSONObject.savedMaps[_index].layerName;
					selectedLayerIndex = _index;

					// Update UI
					BuildUI();
				});

				var _fillLayer = new Button();
				_fillLayer.style.width = 40;
				_fillLayer.text = "Fill";
				_fillLayer.RegisterCallback<ClickEvent>((evt) => 
				{
					tileWorldCreator.FillLayer(jSONObject.savedMaps[_index].layerName);
					tileWorldCreator.ExecuteBuildLayers();
				});

				var _clearLayer = new Button();
				_clearLayer.style.width = 40;
				_clearLayer.text = "Clear";
				_clearLayer.RegisterCallback<ClickEvent>((evt) => 
				{
					tileWorldCreator.ClearLayer(jSONObject.savedMaps[_index].layerName);
					tileWorldCreator.ExecuteBuildLayers();
				});

				var _removeLayer = new Button();
				_removeLayer.text = "X";
				_removeLayer.RegisterCallback<ClickEvent>(evt => 
				{
					RemoveLayer(_index);
				});

				_layerHorizontalContainer.Add(_layer);
				_layerHorizontalContainer.Add(_fillLayer);
				_layerHorizontalContainer.Add(_clearLayer);
				_layerHorizontalContainer.Add(_removeLayer);	

				_ui.Add(_layerHorizontalContainer);
			}

			var _addLayer = new Button();
			_addLayer.text = "Add Layer";
			_addLayer.RegisterCallback<ClickEvent>((evt) =>
			{
				AddLayer();
			});

			_ui.Add(_addLayer);

			_root.Add(_ui);


			uIDocument.rootVisualElement.Add(_root);
		}
#endregion
	}
}