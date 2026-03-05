// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.09.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using ServerData;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CFogPlane : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Child] private MeshRenderer[] _meshRenderers;
		[SerializeField] private EYearMilestone _blockingYear;
		[SerializeField] private Transform _targetCameraPoint;
		[SerializeField] private ERegionLocation _location;

		private const string FadeKeyword = "_FADE";
		private static readonly CInputLock InputLock = new("FogPlaneLock", EInputLockLayer.FullscreenOverlay);
		private static readonly int DissolveHash = Shader.PropertyToID("_Dissolve");

		private CLazyActionQueue _lazyActionQueue;
		private ICtsProvider _ctsProvider;
		private CEventSystem _eventSystem;
		private IEventBus _eventBus;
		private bool _isVisible;
		private CUser _user;
		
		[Inject]
		private void Inject(
			CLazyActionQueue lazyActionQueue,
			ICtsProvider ctsProvider, 
			CEventSystem eventSystem, 
			IEventBus eventBus, 
			CUser user
			)
		{
			_lazyActionQueue = lazyActionQueue;
			_eventSystem = eventSystem;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			SetInitialState();
			TryBindToYearChange();
		}

		public void Show()
		{
			SetActiveRenderer(true);
		}

		public async UniTask UncoverFog(CancellationToken ct)
		{
			_eventSystem.AddInputLocker(InputLock);
			_eventBus.ProcessTask(new CFocusCameraOnPointTask(_targetCameraPoint.position, 1f));
			_eventBus.ProcessTask(new COpenNewLocationScreenTask(_location));
			await UniTask.WaitForSeconds(1f, cancellationToken: ct);
			await BlendOut(2f, ct);
			await UniTask.WaitForSeconds(0.15f, cancellationToken: ct);
			_eventSystem.RemoveInputLocker(InputLock);
		}
		
		private void SetInitialState()
		{
			bool shouldBeVisible = ShouldBeVisible();
			SetActiveRenderer(shouldBeVisible);
			_isVisible = shouldBeVisible;
		}

		private void TryBindToYearChange()
		{
			bool isActive = IsRendererActive();
			if (!isActive)
				return;
			
			bool shouldBeVisible = ShouldBeVisible();
			if (!shouldBeVisible)
				return;
			
			_eventBus.Subscribe<CYearIncreasedSignal>(OnYearIncreased);
		}

		private bool ShouldBeVisible()
		{
			return _user.Progress.Year < _blockingYear;
		}

		private void OnYearIncreased(CYearIncreasedSignal signal)
		{
			HandleYearIncreased(_ctsProvider.Token).Forget();
		}

		private async UniTask HandleYearIncreased(CancellationToken ct)
		{
			if(!_isVisible)
				return;
			
			bool shouldBeVisible = ShouldBeVisible();
			if (shouldBeVisible)
				return;
			
			if(!gameObject.activeSelf)
				return;

			await UniTask.WaitUntil(() => !_lazyActionQueue.IsProcessing, cancellationToken: ct);

			_isVisible = false;
			Func<CancellationToken, UniTask> action = UncoverFog;
			CPlayUniTaskLazyAction uncoverLazyAction = new (action);
			_lazyActionQueue.AddAction(uncoverLazyAction);
		}
		
		private async UniTask BlendOut(float duration, CancellationToken ct)
		{
			for (int i = 1; i < _meshRenderers.Length; i++)
			{
				PlayAnim(_meshRenderers[i], duration, ct).Forget();
			}

			await PlayAnim(_meshRenderers[0], duration, ct);
			SetActiveRenderer(false);
		}
		
		private async UniTask PlayAnim(Renderer rend, float duration, CancellationToken ct)
		{
			Material material = rend.material;
			material.EnableKeyword(FadeKeyword);
			await material.DOFloat(0f, DissolveHash, duration).WithCancellation(ct);
		}
		
		private void SetActiveRenderer(bool isActive)
		{
			foreach (MeshRenderer meshRenderer in _meshRenderers)
			{
				meshRenderer.enabled = isActive;
			}
		}
		
		private bool IsRendererActive()
		{
			return _meshRenderers[0].enabled;
		}
	}
}