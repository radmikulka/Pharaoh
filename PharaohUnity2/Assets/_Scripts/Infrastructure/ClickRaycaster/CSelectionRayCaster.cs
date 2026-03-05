using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
    internal class CSelectionRayCaster : MonoBehaviour, IInitializable, IConstructable
    {
        private const float SphereCastRadius = 0.1f;
		
        private readonly CRaycastHitComparer _rayCastHitComparer = new();
        private System.Buffers.ArrayPool<RaycastHit> _rayCastHitPool;
        private IMainCameraProvider _mainCameraProvider;
        private CEventSystem _eventSystem;
        private IEventBus _eventBus;

        [Inject]
        private void Inject(IEventBus eventBus, IMainCameraProvider mainCameraProvider, CEventSystem eventSystem)
        {
            _mainCameraProvider = mainCameraProvider;
            _eventSystem = eventSystem;
            _eventBus = eventBus;
        }

        public void Construct()
        {
            _rayCastHitPool = System.Buffers.ArrayPool<RaycastHit>.Shared;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<CTouchInputEndSignal>(OnTouchEnd);
        }

        private void OnTouchEnd(CTouchInputEndSignal touchStart)
        {
            const float maxDelta = 50f;
            Vector2 delta = touchStart.PointerDownPos - touchStart.ScreenCoords;
            if(delta.magnitude > maxDelta)
                return;
            
            CastCameraRay(touchStart.ScreenCoords);
        }

        private bool IsBlocked()
        {
            return _eventSystem.IsInputLocked();
        }

        private void CastCameraRay(Vector2 cameraScreenCoords)
        {
            bool isBlocked = IsBlocked();
            if (isBlocked)
                return;
            
            const float maxRayDistance = 10_000;
            RaycastHit[] hits = _rayCastHitPool.Rent(10);
            try
            {
                Ray ray = _mainCameraProvider.Camera.ScreenPointToRay(cameraScreenCoords);
                int hitsCount = Physics.RaycastNonAlloc(ray, hits, maxRayDistance, CObjectLayerMask.RaycastTarget);
                bool somethingHit = ProcessRayHits(hits, hitsCount);

                if (somethingHit) 
                    return;
                
                hitsCount = Physics.SphereCastNonAlloc(ray, SphereCastRadius, hits, maxRayDistance, CObjectLayerMask.RaycastTarget);
                ProcessRayHits(hits, hitsCount);
            }
            finally
            {
                _rayCastHitPool.Return(hits);
            }
        }

        private bool ProcessRayHits(RaycastHit[] hits, int hitsCount)
        {
            if (hitsCount <= 0)
                return false;
			
            Array.Sort(hits, 0, hitsCount, _rayCastHitComparer);
            for (int i = 0; i < hitsCount; i++)
            {
                RaycastHit hit = hits[i];
                IClickableItem item = hit.transform.GetComponent<IClickableItem>();
                if (item == null)
                    continue;

                SetCurrentSelectedItem(item);
                return true;
            }

            return false;
        }

        private void SetCurrentSelectedItem(IClickableItem item)
        {
            item.OnClicked();
        }
		
        private class CRaycastHitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}