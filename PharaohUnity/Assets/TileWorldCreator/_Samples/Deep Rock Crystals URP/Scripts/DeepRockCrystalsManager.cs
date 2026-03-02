using UnityEngine;
using UnityEngine.UIElements;

namespace GiantGrey.TileWorldCreator.Samples
{
    public class DeepRockCrystalsManager : MonoBehaviour
    {
        private static DeepRockCrystalsManager _instance;

        public static DeepRockCrystalsManager Instance { get { return _instance; } }


        public TileWorldCreatorManager tileWorldCreatorManager;
        public Transform player;

        public UIDocument uIDocument;


        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } 
            else 
            {
                _instance = this;
            }
        }

        public void Start()
        {
            BuildUI();

            // Completely reset map and generate new from scratch   
            tileWorldCreatorManager.ResetConfiguration();
            tileWorldCreatorManager.GenerateCompleteMap();
        }

        void BuildUI()
        {
            var _root = new VisualElement();

            var _container = new VisualElement();
            _container.style.left = 20;
            _container.style.top = 20;
            _container.style.borderTopLeftRadius = 20;
            _container.style.borderTopRightRadius = 20;
            _container.style.borderBottomLeftRadius = 20;
            _container.style.borderBottomRightRadius = 20;
            _container.style.backgroundColor = Color.white;
            _container.style.paddingBottom = 10;
            _container.style.paddingLeft = 10;
            _container.style.paddingRight = 10;
            _container.style.paddingTop = 10;
            _container.style.width = 220;
            
            var _label = new Label("MiniGame");
            _label.style.fontSize = 18;
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _container.Add(_label);

            var _description = new Label("- Use WASD to move player." + "\n"
            + "- Mouse click to break tiles.");

            _container.Add(_description);

            var _button = new Button(() => 
            {
                tileWorldCreatorManager.ResetConfiguration(); 
                tileWorldCreatorManager.GenerateCompleteMap();
            });
            
            _button.text = "Generate New Map";

            _container.Add(_button);

            _root.Add(_container);

            uIDocument.rootVisualElement.Add(_root);
        }

    }
}