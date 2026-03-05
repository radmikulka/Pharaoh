// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CRaycastTargetHandler : MonoBehaviour, IConstructable, IInitializable
	{
		private const float SphereCastRadius = 0.1f;
		private const float MaxDistance = 5000;
		
		private IMainCameraProvider _mainCameraProvider;
		private IEventBus _eventBus;
		
		private readonly CRaycastHitComparer _rayCastHitComparer = new();
		private System.Buffers.ArrayPool<RaycastHit> _rayCastHitPool;
		
		[Inject]
		private void Inject(IMainCameraProvider mainCameraProvider, IEventBus eventBus)
		{
			_mainCameraProvider = mainCameraProvider;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_rayCastHitPool = System.Buffers.ArrayPool<RaycastHit>.Shared;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CTouchInputStartSignal>(OnTouchStart);
		}
		
		private void OnTouchStart(CTouchInputStartSignal touchStart)
		{
			CastCameraRay(touchStart.ScreenCoords);
		}

		private void CastCameraRay(Vector2 cameraScreenCoords)
		{
			RaycastHit[] hits = _rayCastHitPool.Rent(10);
			try
			{
				Ray ray = _mainCameraProvider.Camera.ScreenPointToRay(cameraScreenCoords);
				int hitsCount = Physics.RaycastNonAlloc(ray, hits, MaxDistance, CObjectLayerMask.RaycastTarget);
				bool somethingHit = ProcessRayHits(hits, hitsCount);

				if (!somethingHit)
				{
					hitsCount = Physics.SphereCastNonAlloc(ray, SphereCastRadius, hits, MaxDistance, CObjectLayerMask.RaycastTarget);
					ProcessRayHits(hits, hitsCount);
				}
			}
			finally
			{
				_rayCastHitPool.Return(hits);
			}
		}
		
		private bool ProcessRayHits(RaycastHit[] hits, int hitsCount)
		{
			Array.Sort(hits, 0, hitsCount, _rayCastHitComparer);
			for (int i = 0; i < hitsCount; i++)
			{
				RaycastHit hit = hits[i];
				IRaycastTarget raycastTarget = hit.transform.GetComponent<IRaycastTarget>();
				if (raycastTarget == null)
					continue;

				raycastTarget.OnClick();
				return true;
			}
			return false;
		}
	}
}