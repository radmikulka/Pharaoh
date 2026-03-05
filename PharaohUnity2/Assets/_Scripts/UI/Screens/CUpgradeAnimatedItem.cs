// =========================================
// AUTHOR: Marek Karaba
// DATE:   05.11.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUpgradeAnimatedItem : MonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private EUpgradeAnimatedItemType _itemType;
		[SerializeField] private ParticleSystem _particleSystem;
		[SerializeField] private RectTransform _textRectTransform;

		private CAnimationProvider _animationProvider;
		
		public EUpgradeAnimatedItemType ItemType => _itemType;
		
		[Inject]
		private void Inject(CAnimationProvider animationProvider)
		{
			_animationProvider = animationProvider;
		}
		
		public async UniTask AnimateGlow(float duration, CancellationToken ct, Action onHighlightAction)
		{
			await _animationProvider.ScaleUpAndDown.AnimateScaleDownAndUpWithBounce(_textRectTransform, duration, ct, OnHighestScaleAction);
			return;
			
			void OnHighestScaleAction()
			{
				_particleSystem.Stop();
				_particleSystem.Play();

				onHighlightAction?.Invoke();
			}
		}
	}
}