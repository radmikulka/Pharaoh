// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Unity.Cinemachine;
using Zenject;

namespace Pharaoh
{
	public class CCameraZoom : ValidatedMonoBehaviour, IInitializable
	{
		private const float ZoomDuration = 1.5f;

		[SerializeField, Child] private CinemachineCameraOffset _cinemachineOffset;
		[SerializeField, Child] private CinemachineCamera _cinemachineCamera;

		private IEventBus _eventBus;
		private Tween _zoomTween;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CMonumentPartActivatedSignal>(OnMonumentPartActivated);
		}

		public void SetTargetZoom(float zoom)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zoom);
		}

		private void OnMonumentPartActivated(CMonumentPartActivatedSignal signal)
		{
			float targetZoom = signal.Part.CameraZoom;
			_zoomTween?.Kill();
			_zoomTween = DOTween.To(
				() => transform.localPosition.z,
				z => transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z),
				targetZoom,
				ZoomDuration
			).SetEase(Ease.InOutSine);
		}
	}
}
