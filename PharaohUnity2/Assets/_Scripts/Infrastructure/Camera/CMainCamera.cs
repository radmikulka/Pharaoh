// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public class CMainCamera : ValidatedMonoBehaviour, 
        IConstructable, IInitializable, IMainCamera
    {
        [SerializeField, Self] private CinemachineBrain _cinemachineBrain;
        [SerializeField, Self] private Camera _camera;

        private LayerMask _defaultRenderingMask;
        private IMainCameraProvider _provider;
        private Vector3 _initialForwardDirection;

        public Camera Camera => _camera;
        public Vector3 InitialForwardDirection => _initialForwardDirection;

        [Inject]
        private void Inject(IMainCameraProvider provider)
        {
            _provider = provider;
        }

        public void Construct()
        {
            _defaultRenderingMask = _camera.cullingMask;
            SetCameraBlendMode(true);
            _initialForwardDirection = _camera.transform.forward;
        }

        public void Initialize()
        {
            _provider.SetMainCamera(this);
        }

        public bool IsLiveCamera(CinemachineCamera cam)
        {
            return _cinemachineBrain.IsLiveChild(cam);
        }

        public void SetCameraBlendMode(bool allowBlend)
        {
            CinemachineBlendDefinition newBlend = allowBlend ? 
                new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.6f) 
                : new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            _cinemachineBrain.DefaultBlend = newBlend;
        }

        public void CaptureFrame(bool state)
        {
            if (state)
            {
                _camera.cullingMask = 0;
                return;
            }
            
            _camera.cullingMask = _defaultRenderingMask;
        }

        public Vector3 WorldToScreenPoint(Vector3 point)
        {
            return _camera.WorldToScreenPoint(point);
        }

        public Ray ScreenPointToRay(Vector2 screenCoords)
        {
            return _camera.ScreenPointToRay(screenCoords);
        }
    }
}