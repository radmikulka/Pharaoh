// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

namespace TycoonBuilder
{
	public enum EServiceTechnicalFlow
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