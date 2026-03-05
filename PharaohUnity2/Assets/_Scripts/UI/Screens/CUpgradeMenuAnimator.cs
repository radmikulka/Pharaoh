// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUpgradeMenuAnimator : MonoBehaviour, IConstructable
	{
		private const float ScaleDownDuration = 0.2f;
		private const float ScaleUpDuration = 0.2f;
		private const float TextDuration = 0.3f;
		private const float GlowDuration = 0.3f;

		[SerializeField] private RectTransform _upgradeContent;
		[SerializeField] private CUiComponentText _levelText;
		[SerializeField] private CMenuLevelSetter _menuLevelSetter;
		[SerializeField] private ParticleSystem _levelParticleSystem;
		[SerializeField] private CUpgradeAnimatedItem[] _upgradeItems;
		[SerializeField] private CUpgradeTimerAnimator _upgradeTimerAnimator;
		
		private CAnimationProvider _animationProvider;
		private CUiTextInScreenStaticScaleSetter[] _textStaticScaleSetters;

		[Inject]
		private void Inject(CAnimationProvider animationProvider)
		{
			_animationProvider = animationProvider;
		}

		public void Construct()
		{
			_textStaticScaleSetters = GetComponentsInChildren<CUiTextInScreenStaticScaleSetter>(true);
		}
		
		public async UniTask AnimateHide(CancellationToken ct)
		{
			SetTextStaticScale(false);
			await _animationProvider.ScaleUpAndDown.AnimateScaleDown(_upgradeContent, ScaleDownDuration, ct);
			SetTextStaticScale(true);
		}

		public async UniTask AnimateShow(CancellationToken ct)
		{
			SetTextStaticScale(false);
			await _animationProvider.ScaleUpAndDown.AnimateScaleUp(_upgradeContent, ScaleUpDuration, ct);
			SetTextStaticScale(true);
		}

		public async UniTask AnimateLevelText(CancellationToken ct, int newLevel)
		{
			await _animationProvider.ScaleUpAndDown.AnimateScaleDownAndUpWithBounce(_levelText.RectTransform,
				TextDuration, ct, HighestScaleAction);
			return;

			void HighestScaleAction()
			{
				_levelParticleSystem.Stop();
				_levelParticleSystem.Play();
				_menuLevelSetter.SetLevel(newLevel);
			}
		}

		public async UniTask AnimateUpgradeItemsImage(CancellationToken ct, Action onHighlightAction,
			params EUpgradeAnimatedItemType[] blockedItems)
		{
			List<UniTask> tasks = new List<UniTask>();
			bool allBlocked = true;
			foreach (CUpgradeAnimatedItem upgradeItem in _upgradeItems)
			{
				if (blockedItems.Contains(upgradeItem.ItemType))
					continue;

				allBlocked = false;
				tasks.Add(upgradeItem.AnimateGlow(GlowDuration, ct, onHighlightAction));
			}

			await UniTask.WhenAll(tasks);

			if (allBlocked)
			{
				onHighlightAction?.Invoke();
			}
		}

		public async UniTask AnimateRequirementsToInProgress(CancellationToken ct, Action onTransitionMiddle)
		{
			_upgradeTimerAnimator.BeginAnimation();
			
			RectTransform requirementsVisual = _upgradeTimerAnimator.GetRequirementsVisualForAnimation();
			RectTransform inProgressVisual = _upgradeTimerAnimator.GetInProgressVisualForAnimation();
			
			await _animationProvider.ScaleUpAndDown.AnimateScaleDown(requirementsVisual, ScaleDownDuration, ct);
			
			_upgradeTimerAnimator.SetRequirementsActive(false);
			_upgradeTimerAnimator.SetRequirementsScale(Vector3.one);
			
			onTransitionMiddle?.Invoke();
			
			_upgradeTimerAnimator.SetInProgressScale(Vector3.zero);
			_upgradeTimerAnimator.SetInProgressActive(true);
			_upgradeTimerAnimator.SetProgressText(true);
			
			await _animationProvider.ScaleUpAndDown.AnimateScaleUp(inProgressVisual, ScaleUpDuration, ct);
			
			_upgradeTimerAnimator.EndAnimation();
		}

		private void SetTextStaticScale(bool isStatic)
		{
			foreach (CUiTextInScreenStaticScaleSetter textStaticScaleSetter in _textStaticScaleSetters)
			{
				if (isStatic)
				{
					textStaticScaleSetter.SetScaleDefault();
				}
				else
				{
					textStaticScaleSetter.SetScaleNotStatic();
				}
			}
		}
	}
}