// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

namespace TycoonBuilder
{
	public enum EVehicleState
	{
		None = 0,
		// owned states
		ReadyToCollect = 1,
		ReadyToSend = 2,
		DispatchedCurrent = 3,
		LowDurability = 4,
		DispatchedOther = 5,
		DispatchedTransportFleet = 6,
		// not owned states
		NotOwned = 7,
		Locked = 8,
	}
}