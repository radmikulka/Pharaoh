// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2023
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public interface IServerTime : ICurrentTimeProvider
	{
		bool IsDayRefreshTime();
		long GetDayRefreshTimeInMs();
		long GetTimestampInSecs() => GetTimestampInMs() / CTimeConst.Second.InMilliseconds;
		void Init(long serverTime, long dayRefreshTime);
	}
}