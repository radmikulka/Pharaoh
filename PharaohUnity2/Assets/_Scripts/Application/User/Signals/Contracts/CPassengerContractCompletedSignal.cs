// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CPassengerContractCompletedSignal : IEventBusSignal
	{
		public readonly CContract PassengerContract;

		public CPassengerContractCompletedSignal(CContract passengerContract)
		{
			PassengerContract = passengerContract;
		}
	}
}