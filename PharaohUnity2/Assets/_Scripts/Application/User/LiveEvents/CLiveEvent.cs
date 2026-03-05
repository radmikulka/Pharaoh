// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CLiveEvent<T> : ILiveEvent where T : ILiveEventContent
	{
		public ILiveEventContent BaseContent { get; }
		public T Content => (T) BaseContent;
		
		public ELiveEvent Id { get; }
		public long EndTimeInMs { get; }
		public long StartTimeInMs { get; }
		public long CancellationTimeInMs { get; }
		
		public bool IsFinished(long currentTimeInMs)
		{
			return currentTimeInMs >= EndTimeInMs;
		}
		
		public bool IsCancelled(long currentTimeInMs)
		{
			return currentTimeInMs >= CancellationTimeInMs;
		}

		public CLiveEvent(T content, ELiveEvent id, long endTimeInMs, long startTimeInMs, long cancellationTimeInMs)
		{
			Id = id;
			BaseContent = content;
			EndTimeInMs = endTimeInMs;
			StartTimeInMs = startTimeInMs;
			CancellationTimeInMs = cancellationTimeInMs;
		}
	}
}