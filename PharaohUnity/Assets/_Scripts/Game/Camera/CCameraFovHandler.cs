// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;

namespace Pharaoh
{
    public class CCameraFovHandler : ValidatedMonoBehaviour, IConstructable
    {
        [SerializeField, Self] private Camera _camera;
        [SerializeField] private Transform _referencePoint;
        [SerializeField] private float _camFrustrumWidth;

        public void Construct()
        {
            RecalculateFov();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            
            RecalculateFov();
        }

        private void RecalculateFov()
        {
            // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
            float height = _camFrustrumWidth / _camera.aspect;
            var distance = Vector3.Distance(_camera.transform.position, _referencePoint.position);
            _camera.fieldOfView = 2.0f * CMath.Atan(height * 0.5f / distance) * CMath.Rad2Deg;
        }
    }
}