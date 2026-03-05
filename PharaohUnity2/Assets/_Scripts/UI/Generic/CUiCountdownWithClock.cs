// =========================================
// AUTHOR: Juraj Joscak
// DATE:   30.09.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiCountdownWithClock : ValidatedMonoBehaviour, IAldaFrameworkComponent, IScreenCloseEnd
	{
		[SerializeField, Child] private CUiCountdown _countdown;
		[SerializeField, Child] private Animation _animatedClock;

		private IServerTime _serverTime;
		
		[Inject]
		private void Inject(IServerTime serverTime)
		{
			_serverTime = serverTime;
		}
		
		public void SetTargetTime(long targetTime, IAldaTimeSpanProvider formatProvider, ICurrentTimeProvider timeProvider = null)
		{
			timeProvider ??= _serverTime;
			_countdown.SetTargetTime(targetTime, formatProvider, timeProvider);
		}

		public void SetTime(long milliseconds, IAldaTimeSpanProvider formatProvider, ICurrentTimeProvider timeProvider = null)
		{
			timeProvider ??= _serverTime;
			_countdown.SetTime(milliseconds, formatProvider, timeProvider);
		}
		
		public void Enable()
		{
			EnableAnimation();
			_countdown.Enable();
		}

		public void Disable()
		{
			StopAnimation();
			_countdown.Disable();
		}
		
		public void ShowClock()
		{
			_countdown.TextComponent.Component.alignment = TextAlignmentOptions.Right;
			_animatedClock.gameObject.SetActive(true);
		}
		
		public void HideClock()
		{
			_countdown.TextComponent.Component.alignment = TextAlignmentOptions.Center;
			StopAnimation();
			_animatedClock.gameObject.SetActive(false);
		}

		public void AddCallback(long secondsToTarget, Action callback)
		{
			_countdown.AddCallback(secondsToTarget, callback);
		}

		public void ClearCallbacks()
		{
			_countdown.ClearCallbacks();
		}

		public void SetText(string text)
		{
			_countdown.SetText(text);
		}

		public void EnableAnimation()
		{
			if (!_animatedClock.enabled)
			{
				_animatedClock.SetActiveBehaviour(true);
			}
			
			if (_animatedClock.isPlaying)
				return;
			
			_animatedClock.Play();
		}
		
		public void StopAnimation()
		{
			if(!_animatedClock.enabled)
				return;
			
			_animatedClock.Rewind();
			_animatedClock.Sample();
			_animatedClock.Stop();
			_animatedClock.SetActiveBehaviour(false);
		}

		public void OnScreenCloseEnd()
		{
			_animatedClock.SetActiveBehaviour(false);
		}
	}
}