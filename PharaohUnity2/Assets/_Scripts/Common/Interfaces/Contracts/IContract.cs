// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.11.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IContract
	{
		string Uid { get; }
		ERegion[] Regions { get; }
		SResource Requirement { get; }
		EContractType Type { get; }
		int DeliveredAmount { get; }
		CTripPrice TripPrice { get; }
		IValuable[] Rewards { get; }
		EMovementType MovementType { get; }
		public bool IsFleetTask { get; }
	}
}