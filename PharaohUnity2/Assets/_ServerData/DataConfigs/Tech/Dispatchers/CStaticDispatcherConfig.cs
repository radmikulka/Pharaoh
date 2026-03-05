// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.11.2025
// =========================================

namespace ServerData
{
	public class CStaticDispatcherConfig
	{
		public EDispatcher DispatcherId { get; }
		public IUnlockRequirement UnlockRequirement { get; }

		public CStaticDispatcherConfig(EDispatcher dispatcherId, IUnlockRequirement unlockRequirement)
		{
			DispatcherId = dispatcherId;
			UnlockRequirement = unlockRequirement;
		}
	}
}