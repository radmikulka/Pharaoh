using System.Collections.Generic;
using AldaEngine;
using AldaEngine.UnityObjectPool;
using Pharaoh.Map;
using ServerData;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Pharaoh.Building
{
	public class CBuildingMenuPanel : MonoBehaviour, IEscapable
	{
		[SerializeField] private Canvas _canvas;
		[SerializeField] private RectTransform _honeycombRoot;
		[SerializeField] private CBuildingMenuItem _itemPrefab;
		[SerializeField] private Button _outsideBlocker;
		[SerializeField] private float _itemSpacing = 120f;

		private CBuildingPlacementValidator _validator;
		private IMissionController _missionController;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private IMainCameraProvider _mainCameraProvider;
		private CEscapeHandler _escapeHandler;
		private CMapInstance _mapInstance;
		private IEventBus _eventBus;

		private SCellCoord _targetCell;
		private bool _isOpen;
		private readonly List<CBuildingMenuItem> _spawnedItems = new();

		[Inject]
		private void Inject(
			CBuildingPlacementValidator validator,
			IMissionController missionController,
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			IMainCameraProvider mainCameraProvider,
			CEscapeHandler escapeHandler,
			CMapInstance mapInstance,
			IEventBus eventBus)
		{
			_validator = validator;
			_missionController = missionController;
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_mainCameraProvider = mainCameraProvider;
			_escapeHandler = escapeHandler;
			_mapInstance = mapInstance;
			_eventBus = eventBus;

			_eventBus.Subscribe<COpenBuildingMenuSignal>(OnOpenBuildingMenu);
			_outsideBlocker.onClick.AddListener(Close);
			_canvas.enabled = false;
		}

		private void OnDestroy()
		{
			_outsideBlocker.onClick.RemoveListener(Close);

			if (_isOpen)
				_escapeHandler.UnregisterEscapable(this);
		}

		private void OnOpenBuildingMenu(COpenBuildingMenuSignal signal)
		{
			_targetCell = signal.Cell;
			Show();
		}

		private void Show()
		{
			if (_isOpen)
				Close();

			PopulateBuildings();

			if (_spawnedItems.Count == 0)
				return;

			_canvas.enabled = true;
			_escapeHandler.RegisterEscapable(this);
			_isOpen = true;
			UpdatePosition();
		}

		public void Close()
		{
			_canvas.enabled = false;
			_escapeHandler.UnregisterEscapable(this);
			ClearItems();
			_isOpen = false;
		}

		private void LateUpdate()
		{
			if (_isOpen)
				UpdatePosition();
		}

		public void OnEscape()
		{
			Close();
		}

		private void UpdatePosition()
		{
			Vector3 worldPos = new Vector3(_targetCell.X, 0f, _targetCell.Y);
			Vector3 screenPos = _mainCameraProvider.Camera.WorldToScreenPoint(worldPos);

			if (screenPos.z < 0)
			{
				Close();
				return;
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_canvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPoint);
			_honeycombRoot.anchoredPosition = localPoint;
		}

		private void PopulateBuildings()
		{
			ClearItems();

			CMapCell cell = _mapInstance.GetCell(_targetCell.X, _targetCell.Y);
			if (cell == null)
				return;

			CMissionConfig missionConfig = _resourceConfigs.Missions.GetConfig(_missionController.ActiveMissionId);
			EBuildingId[] availableBuildings = missionConfig.AvailableBuildings;

			if (availableBuildings == null)
				return;

			List<EBuildingId> placeable = _validator.GetPlaceableBuildings(cell, availableBuildings);

			for (int i = 0; i < placeable.Count; i++)
			{
				EBuildingId buildingId = placeable[i];
				CBuildingResourceConfig config = _resourceConfigs.Buildings.GetConfig(buildingId);
				Sprite icon = _bundleManager.LoadItem<Sprite>(config.Icon, EBundleCacheType.Persistent);

				CBuildingMenuItem item = Instantiate(_itemPrefab, _honeycombRoot);
				item.Initialize(buildingId, icon, config.DisplayName, _targetCell, _eventBus, this);

				RectTransform rt = item.GetComponent<RectTransform>();
				rt.anchoredPosition = GetHoneycombOffset(i);

				_spawnedItems.Add(item);
			}
		}

		private Vector2 GetHoneycombOffset(int index)
		{
			if (index < 6)
			{
				float angle = (90f - 60f * index) * Mathf.Deg2Rad;
				return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _itemSpacing;
			}

			int ri = index - 6;
			float a2 = (90f - 30f * ri) * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * (_itemSpacing * 2f);
		}

		private void ClearItems()
		{
			foreach (CBuildingMenuItem item in _spawnedItems)
			{
				if (item != null)
					Destroy(item.gameObject);
			}
			_spawnedItems.Clear();
		}
	}
}
