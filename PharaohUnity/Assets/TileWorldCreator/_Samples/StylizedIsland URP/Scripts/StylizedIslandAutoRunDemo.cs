
using UnityEngine;
#if TWC_PRIMETWEEN
using PrimeTween;
#endif
using System.Collections.Generic;
using System.Collections;


namespace GiantGrey.TileWorldCreator.Samples
{
    public class StylizedIslandAutoRunDemo : MonoBehaviour
    {
        public TileWorldCreatorManager manager;

        public GameObject paintCube;
        public GameObject activeCell;

        public GameObject boat;
        public GameObject house;
        public GameObject clouds_01;
        public GameObject clouds_02;

        public List<GameObject> cells = new List<GameObject>();

        public List<GameObject> layers = new List<GameObject>();

        public Vector2 minMaxTimeBetweenCells = new Vector2(0.025f, 0.15f);
        public Vector2 minMaxTimeBetweenGroups = new Vector2(0.1f, 0.2f);
        public Vector2 minMaxTimeBetweenLayers = new Vector2(0.2f, 0.4f);

        void OnGUI()
        {
            if (GUILayout.Button("Restart"))
            {
                StartDEMO();
            }
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            StartDEMO();
        }

        void StartDEMO()
        {
            manager.ResetConfiguration();

            StartCoroutine(StartDEMOIE());
        }

        IEnumerator StartDEMOIE()
        {
            paintCube.SetActive(true);

            if (clouds_01 != null)
            {
                clouds_01.gameObject.SetActive(false);
            }
            if (clouds_02 != null)
            {
                clouds_02.gameObject.SetActive(false);
            }
            
            if (house != null) 
            {
                house.SetActive(false);
            }
            if (boat != null) 
            {
                boat.SetActive(false);
            }

            for (int l = 0; l < layers.Count; l ++)
            {
                foreach (Transform _group in layers[l].transform)
                {
                    foreach (Transform _position in _group.transform)
                    {
                        var _value = Random.Range(minMaxTimeBetweenCells.x, minMaxTimeBetweenCells.y);
#if TWC_PRIMETWEEN
                        Tween.Position(paintCube.transform, _position.position, _value).OnComplete(() => 
                        {
                            var _act = Instantiate(activeCell, _position.position, Quaternion.identity);
                            Tween.ShakeLocalPosition(_act.transform, new Vector3(1f, 3f, 1f), 0.2f, 1f);
                            cells.Add(_act);
                        });
#else
                        paintCube.transform.position = _position.position;
                        yield return new WaitForSeconds(_value);
                        var _act = Instantiate(activeCell, _position.position, Quaternion.identity);
                        cells.Add(_act);
#endif
                        yield return new WaitForSeconds(_value);
                    }
                    
                    AddPosition(_group.transform, layers[l].gameObject.name);

                    yield return new WaitForSeconds(Random.Range(minMaxTimeBetweenGroups.x, minMaxTimeBetweenGroups.y));
                }

                yield return new WaitForSeconds(Random.Range(minMaxTimeBetweenLayers.x, minMaxTimeBetweenLayers.y));
            }
        
            yield return null;
        

            paintCube.SetActive(false);

        
            if (house != null)
            {
                house.SetActive(true);
#if TWC_PRIMETWEEN
                Sequence.Create()
                .Insert(0, Tween.ShakeLocalPosition(house.transform, new Vector3(0f, 4f, 0f), 0.2f, 2f))
                .InsertCallback(0.2f, () => boat?.SetActive(true))
                .Insert(0.2f, Tween.ShakeLocalPosition(boat.transform, new Vector3(0f, 4f, 0f), 0.2f, 2f))
                .ChainDelay(0.3f)
                .ChainCallback(() => { clouds_01.gameObject.SetActive(true); })
                .ChainDelay(0.2f)
                .ChainCallback(() => { clouds_02.gameObject.SetActive(true); });
#else
                // house.transform.localPosition += new Vector3(0f, 0.4f, 0f);
               
                boat.SetActive(true);
                // boat.transform.localPosition += new Vector3(0f, 4f, 0f);
                clouds_01.gameObject.SetActive(true);
                clouds_02.gameObject.SetActive(true);
#endif
            }
        }

        void AddPosition(Transform _group, string _layerName)
        {
            HashSet<Vector2> _positions = new HashSet<Vector2>();
            foreach (Transform _position in _group.transform)
            {
                _positions.Add(new Vector2(_position.position.x, _position.position.z));
            }

            manager.AddCellsToLayer(_layerName, _positions);
            
            manager.OnBlueprintLayersReady -= BlueprintReady;
            manager.OnBlueprintLayersReady += BlueprintReady;
            manager.ExecuteBlueprintLayers();


            for (int i = 0; i < cells.Count; i ++)
            {
                Destroy(cells[i], 0.2f);
            }

            cells = new List<GameObject>();
            
        }

        void BlueprintReady()
        {
            manager.ExecuteBuildLayers();
        }


        public void OnDrawGizmos()
        {
            for (int l = 0; l < layers.Count; l ++)
            {
                if (!layers[l].gameObject.activeSelf)
                    continue;
                
                var color =  "#ffffff";
                try
                {
                color =  "#" +    (layers[l].gameObject.GetInstanceID() * (l + 1)).ToString().Substring(0, 6);
                }catch{}
                Color _colorConv = Color.white; 
                UnityEngine.ColorUtility.TryParseHtmlString(color, out _colorConv);
                
                
                foreach (Transform _group in layers[l].transform)
                {
                    if (!_group.gameObject.activeSelf)
                        continue;

                    Gizmos.color = _colorConv;
                    foreach (Transform _position in _group.transform)
                    {
                        Gizmos.DrawWireCube(_position.position, new Vector3(0.8f,0.8f,0.8f));     
                    }
                    Gizmos.color = Color.white;
                }
            }
        }

    }
}