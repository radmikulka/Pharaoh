// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServiceEngine;
using ServiceEngine.Ads;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CInactivityRestarter : MonoBehaviour, IInitializable, ITickable
	{
		private static readonly long MaxAllowedInactivityInSecs = 15 * CTimeConst.Minute.InSeconds;
		
		private float _lastActivityTime;
		private float _lastAppFocusTime;
		private bool _restartTriggered;
	    
		private IRestartGameHandler _restartGameHandler;
		private ICrashlytics _crashlytics;
		private IAdsManager _adsManager;
		
		[Inject]
		private void Inject(
			IRestartGameHandler restartGameHandler,
			ICrashlytics crashlytics,
			IAdsManager adsManager
			)
		{
			_restartGameHandler = restartGameHandler;
			_crashlytics = crashlytics;
			_adsManager = adsManager;
		}
		
		public void Initialize()
		{
			UpdateCurrentActivityTime();
			UpdateCurrentFocusTime();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				UpdateCurrentActivityTime();
				return;
			}

			ValidateActivity();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				_lastActivityTime = Time.realtimeSinceStartup;
				ValidateActivity();
				return;
			}

			UpdateCurrentActivityTime();
		}
		
		public void Tick()
		{
			#if UNITY_EDITOR
			if (Input.GetMouseButton(0))
			{
				UpdateCurrentActivityTime();
			}
			#endif
		    
			if (Input.touchCount > 0)
			{
				UpdateCurrentActivityTime();
			}
			ValidateActivity();
		}

		private void ValidateActivity()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
		    
			if(_adsManager.LastAdFinishTime + MaxAllowedInactivityInSecs > realtimeSinceStartup)
				return;
		    
			if (realtimeSinceStartup - _lastActivityTime > MaxAllowedInactivityInSecs)
			{
				_crashlytics.Log("Reload game - inactivity in bg");
				RestartGame();
			}
			else if (realtimeSinceStartup - _lastAppFocusTime > MaxAllowedInactivityInSecs)
			{
				_crashlytics.Log("Reload game - inactivity in fg");
				RestartGame();
			}
		}

		private void RestartGame()
		{
			if(_restartTriggered)
				return;
			_restartTriggered = true;
			_restartGameHandler.RestartGame(null);
		}
		
		private void UpdateCurrentActivityTime()
		{
			_lastActivityTime = Time.realtimeSinceStartup;
			UpdateCurrentFocusTime();
		}

		private void UpdateCurrentFocusTime()
		{
			_lastAppFocusTime = Time.realtimeSinceStartup;
		}
	}
}