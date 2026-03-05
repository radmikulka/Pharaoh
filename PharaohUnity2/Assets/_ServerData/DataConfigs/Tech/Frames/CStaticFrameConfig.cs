// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2024
// =========================================

namespace ServerData
{
	public class CStaticFrameConfig
	{
		public readonly EProfileFrame FrameId;
		public readonly IUnlockRequirement UnlockRequirement;

		public CStaticFrameConfig(EProfileFrame frameId, IUnlockRequirement unlockRequirement)
		{
			FrameId = frameId;
			UnlockRequirement = unlockRequirement;
		}
	}
}