// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface ILiveEvent
	{
		ELiveEvent Id { get; }
		long EndTimeInMs { get; }
		long StartTimeInMs { get; }
		long CancellationTimeInMs { get; }
		bool IsFinished(long currentTimeInMs);
		bool IsCancelled(long currentTimeInMs);
		ILiveEventContent BaseContent { get; }
	}
}