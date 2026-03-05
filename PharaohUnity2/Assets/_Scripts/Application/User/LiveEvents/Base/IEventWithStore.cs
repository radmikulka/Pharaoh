// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.12.2025
// =========================================

namespace TycoonBuilder
{
	public interface IEventWithStore : ILiveEventContent
	{
		int EventCoins { get; }
		void ModifyEventCoins(int value);
		bool HaveCoins(int valueToTest);
	}
}