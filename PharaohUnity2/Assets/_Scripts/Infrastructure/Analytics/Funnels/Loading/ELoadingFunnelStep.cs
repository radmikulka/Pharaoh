// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

namespace Pharaoh
{
	public enum ELoadingFunnelStep
	{
		None,
		Start,
		BaseGameBundlesLoaded,
		WaitingForServer,
		ServerResponseReceived,
		BaseGameSceneActivated,
		UiSceneActivated,
		OfflineServicesInited,
		NetworkStatusChecked,
		ServicesInited,
		Completed
	}
}