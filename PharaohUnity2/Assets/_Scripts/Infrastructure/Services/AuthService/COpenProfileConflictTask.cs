// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.09.2025
// =========================================

using System;

namespace TycoonBuilder
{
	public class COpenProfileConflictTask
	{
		public readonly CLoginService.SProfileData DeviceData;
		public readonly CLoginService.SProfileData RemoteData;
		public readonly Action OnUseRemote;
		public readonly Action OnUseDevice;
		public readonly Action OnClose;

		public COpenProfileConflictTask(CLoginService.SProfileData deviceData, CLoginService.SProfileData remoteData, Action onUseRemote, Action onUseDevice, Action onClose)
		{
			DeviceData = deviceData;
			RemoteData = remoteData;
			OnUseRemote = onUseRemote;
			OnUseDevice = onUseDevice;
			OnClose = onClose;
		}
	}
}