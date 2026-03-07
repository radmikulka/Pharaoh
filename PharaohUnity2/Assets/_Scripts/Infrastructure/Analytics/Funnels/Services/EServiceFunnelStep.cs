// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

namespace Pharaoh
{
	public enum EServiceFunnelStep
	{
		None,
		OnlineStart,
		MainThreadServicesPassed,
		NotificationsPassed,
		AttPassed,
		RemoteConfigFetched,
		RemoteConfigActivated
	}
}