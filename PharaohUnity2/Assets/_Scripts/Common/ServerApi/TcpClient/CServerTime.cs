// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace Pharaoh
{
	public class CServerTime : IServerTime, ITickable
	{
		private long _lastServerTimestampInMs;
		private long _currentTimeStampInMs;
		private long _dayRefreshTimeInMs;
		private DateTime _lastResetTime;

		public long GetTimestampInMs()
		{
			return _currentTimeStampInMs;
		}

		public bool IsDayRefreshTime()
		{
			long refreshTime = GetDayRefreshTimeInMs();
			return refreshTime > 0 && GetTimestampInMs() >= refreshTime;
		}

		public long GetDayRefreshTimeInMs()
		{
			return _dayRefreshTimeInMs;
		}

		public void Init(long lastServerTimestampInMs, long dayRefreshTimeInMs)
		{
			_lastServerTimestampInMs = lastServerTimestampInMs;
			_dayRefreshTimeInMs = dayRefreshTimeInMs;
			_lastResetTime = DateTime.UtcNow;
			Tick();
		}

		public void Tick()
		{
			TimeSpan elapsedTime = DateTime.UtcNow - _lastResetTime;
			long elapsedMs = CMath.FloorToInt(elapsedTime.TotalMilliseconds);
			_currentTimeStampInMs = _lastServerTimestampInMs + elapsedMs;
		}
	}
}