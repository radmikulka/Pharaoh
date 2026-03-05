using AldaEngine.AldaFramework;
using KBCore.Refs;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
    public class CUiRect : ValidatedMonoBehaviour, IInitializable
    {
        [SerializeField, SearchableEnum] private EUiRect _rectId;
        [SerializeField, Self] private RectTransform _rectTransform;

        private CUiRectsProvider _uiRectsProvider;
        
        public RectTransform RectTransform => _rectTransform;
        public Vector3 Position => _rectTransform.position;
        public EUiRect RectId => _rectId;

        [Inject]
        private void Inject(CUiRectsProvider uiRectsProvider)
        {
            _uiRectsProvider = uiRectsProvider;
        }

        public void Initialize()
        {
            _uiRectsProvider.Register(this);
        }
    }
}