// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.09.2025
// =========================================

using System;
using AldaEngine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace TycoonBuilder.Ui
{
	public class CUiTimer : ValidatedMonoBehaviour
	{
		[SerializeField, Child] private CUiCountdown _countdown;
		[SerializeField, Child] private Animation _animatedClock;
		[SerializeField, Self] private Slider _progressSlider;
		[SerializeField] private GameObject _completedVisual;
		
		public bool TryUpdateTimer(IServerTime serverTime, long currentTimeInMs, long startTimeInMs, long completionTimeInMs)
		{
			long timeRemaining = completionTimeInMs - currentTimeInMs;
			if (timeRemaining <= 0)
			{
				SetCorrectVisual(false);
				return false;
			}
			
			SetCorrectVisual(true);
			_countdown.SetTime(timeRemaining, IAldaTimeSpanProvider.ProviderOnlyNecessaryPartsWithSeconds, serverTime);

			float duration = completionTimeInMs - startTimeInMs;
			float progress = 1f - timeRemaining / duration;
			_progressSlider.value = progress;
			return true;
		}

		public void UpdateTimer(long remainingTime, long totalTime)
		{
			SetCorrectVisual(true);
			_countdown.SetTime(remainingTime, IAldaTimeSpanProvider.ProviderOnlyNecessaryPartsWithSeconds);
			float progress = 1f - (float)remainingTime / totalTime;
			_progressSlider.value = progress;
		}

		public void SetCompletedState()
		{
			SetCorrectVisual(false);
			_progressSlider.value = 1f;
			EnableAnimation(false);
		}

		private void SetCorrectVisual(bool running)
		{
			if (!running)
			{
				DisableAnimation();
			}
			
			_completedVisual.gameObject.SetActive(!running);
			_countdown.gameObject.SetActive(running);
			EnableAnimation(running);
		}

		public void DisableAnimation()
		{
			_animatedClock.Play();
			_animatedClock.Rewind();
			_animatedClock.Sample();
			_animatedClock.Stop();
			EnableAnimation(false);
		}

		private void EnableAnimation(bool state)
		{
			_animatedClock.SetActiveBehaviour(state);
			if (state)
			{
				_animatedClock.Play();
			}
			else
			{
				_animatedClock.Stop();
			}
		}

		public void SetTimeDuration(long timeRemaining)
		{
			_countdown.SetTime(timeRemaining, IAldaTimeSpanProvider.ProviderOnlyNecessaryParts);
			DisableAnimation();
			_completedVisual.SetActive(false);
			_countdown.gameObject.SetActive(true);
			_progressSlider.value = 0f;
		}

		public void ResetProgress()
		{
			_progressSlider.value = 0f;
		}
	}
}