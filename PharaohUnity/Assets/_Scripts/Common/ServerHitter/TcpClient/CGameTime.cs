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
	public class CGameTime : IGameTime, ITickable
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

		public void Init(long lastServerTimestampInMs)
		{
			_lastServerTimestampInMs = lastServerTimestampInMs;

			long day = CUnixTime.GetDay(_lastServerTimestampInMs);
			_dayRefreshTimeInMs = (day + 1) * CTimeConst.Day.InMilliseconds;
			
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