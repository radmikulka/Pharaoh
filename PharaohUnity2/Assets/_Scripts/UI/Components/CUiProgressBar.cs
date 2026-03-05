// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.01.2026
// =========================================

using System;
using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using ModestTree;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CUiProgressBar : ValidatedMonoBehaviour, IScreenCloseEnd
	{
		[SerializeField] private CUiComponentSlider _durabilitySlider;
		[SerializeField] private CUiSpriteSwapper _fillGlowSwapper;
		[SerializeField] private CUiComponentImage _fillGlow;
		[SerializeField, Child] private CUiImageAlphaAnimator _glowAnimator;
		[SerializeField] private float _sliderStepDuration = 0.25f;

		public bool IsAnimating { get; private set; }
		private readonly List<float> _animationQueue = new();
		private readonly float _sliderStepSize = 0.1f;
		
		public void OnScreenCloseEnd()
		{
			SetGlowAnimation(false);
		}

		private void SetGlowAnimation(bool active)
		{
			_glowAnimator.Toggle(active);
		}

		public void SetSlider(int currentValue, int maxValue, int lowGlowThreshold)
		{
			_fillGlow.SetAlpha(0);
			float endValue = currentValue / (float)maxValue;
			SetValue(endValue);
			SetGlowAnimation(currentValue <= lowGlowThreshold);
		}

		public async UniTask Animate(int currentValue, int maxValue, CancellationToken ct)
		{
			float animationEndValue = currentValue / (float)maxValue;
			_fillGlowSwapper.SetSprite(_durabilitySlider.Value > animationEndValue ? 1 : 0);
			SetGlowAnimation(false);

			RecalculateQueue(animationEndValue);
			
			if(IsAnimating)
				return;
			
			await AnimateInternal(ct);
		}

		private void RecalculateQueue(float endValue)
		{
			_animationQueue.Clear();
			int direction = endValue <= _durabilitySlider.Value ? -1 : 1;
			Func<float, bool> condition = direction < 0 ? Greater : Less;
			
			float step = _durabilitySlider.Value;
			while (condition(step + direction * _sliderStepSize))
			{
				step += direction * _sliderStepSize;
				_animationQueue.Add(step);
			}
			
			_animationQueue.Add(endValue);
			
			bool Greater(float a)
			{
				return a > endValue;
			}
			
			bool Less(float a)
			{
				return a < endValue;
			}
		}

		private async UniTask AnimateInternal(CancellationToken ct)
		{
			float glowDuration = 0.2f;

			IsAnimating = true;
			try
			{
				await AnimateShowGlow(glowDuration, ct);

				while (!_animationQueue.IsEmpty())
				{
					float value = _animationQueue[0];
					_animationQueue.RemoveAt(0);
					await AnimateSlider(value,
						_sliderStepDuration * (CMath.Abs(_durabilitySlider.Value - value) / _sliderStepSize), ct);
				}

				await AnimateHideGlow(glowDuration, ct);
			}
			finally
			{
				IsAnimating = false;
			}

			if (_animationQueue.IsEmpty())
				return;
			
			await AnimateInternal(ct);
		}

		private async UniTask AnimateShowGlow(float duration, CancellationToken ct)
		{
			float timeElapsed = 0;
			_fillGlow.SetAlpha(0f);

			while (timeElapsed < duration)
			{
				float progress = timeElapsed / duration;
				_fillGlow.SetAlpha(progress);
				await UniTask.Yield(ct);
				timeElapsed += Time.deltaTime;
			}
			_fillGlow.SetAlpha(1f);
		}
		
		private async UniTask AnimateHideGlow(float duration, CancellationToken ct)
		{
			float timeElapsed = 0;
			_fillGlow.SetAlpha(1f);

			while (timeElapsed < duration)
			{
				float progress = timeElapsed / duration;
				_fillGlow.SetAlpha(1 - progress);
				await UniTask.Yield(ct);
				timeElapsed += Time.deltaTime;
			}
			_fillGlow.SetAlpha(0f);
		}
		
		private async UniTask AnimateSlider(float endValue, float duration, CancellationToken ct)
		{
			float startValue = _durabilitySlider.Value;
			float timeElapsed = 0;

			while (timeElapsed < duration)
			{
				float progress = timeElapsed / duration;
				float value = Mathf.Lerp(startValue, endValue, progress);
				SetValue(value);
				await UniTask.Yield(ct);
				timeElapsed += Time.deltaTime;
			}
			_durabilitySlider.SetValue(endValue);
			_durabilitySlider.Component.fillRect.gameObject.SetActiveObject(endValue > 0.001);
			SetGlowAnimation(endValue == 0);
		}
		
		private void SetValue(float value)
		{
			_durabilitySlider.SetValue(value);
			_durabilitySlider.Component.fillRect.gameObject.SetActiveObject(value > 0.001);
		}

		public void StopAnimation()
		{
			IsAnimating = false;
		}
	}
}