// =========================================
// NAME: Marek Karaba
// DATE: 03.03.2026
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class CDummyContractTaskProvider : IContractTaskProvider
	{
		public CContractTask GetContractTaskOrDefault(SStaticContractPointer contractPointer)
		{
			IValuable reward = CValuableFactory.HardCurrency(500);
			CContractTask fakeFleetConfig = new 
			(
				new CTransportFleetTaskConfig(25, new[]
				{
					new CTransportFleetSlotConfig(EMovementType.Rail, ETransportType.Bulk),
					new CTransportFleetSlotConfig(EMovementType.Rail, ETransportType.Bulk),
					new CTransportFleetSlotConfig(EMovementType.Road, ETransportType.Container),
				}),
				rewards: new[] { reward },
				closeDispatchMenuWhenIsFullyLoaded: false,
				closeDispatchMenuWhenIsFullyLoadedAndFollowVehicle: false,
				allowGoTo: false
			);
			return fakeFleetConfig;
		}
	}
}