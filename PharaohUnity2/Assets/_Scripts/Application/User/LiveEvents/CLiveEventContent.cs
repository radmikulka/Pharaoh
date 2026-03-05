// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CLiveEventContent : ILiveEventContent
	{
		protected readonly CUser User;
		public ELiveEvent EventId { get; }

		protected CLiveEventContent(CUser user, ELiveEvent eventId)
		{
			EventId = eventId;
			User = user;
		}
	}
}