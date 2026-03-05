// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

namespace TycoonBuilder
{
	public enum ELoadingTechFlow
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
		ServerResponseParsed,
		PrivacyAccepted,
		IntroSeen,
		ServicesInited,
		Completed
	}
}