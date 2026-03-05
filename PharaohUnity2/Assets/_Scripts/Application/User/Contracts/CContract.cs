// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CContract : IContract
	{
		public EContractType Type { get; }

		public SResource Requirement { get; }
		public IValuable[] Rewards { get; }
		public string Uid { get; }
		public int DeliveredAmount { get; private set; }
		public ECustomer Customer { get; }
		public bool IsSeen { get; private set; }
		public CTripPrice TripPrice { get; }
		public ERegion[] Regions { get; }
		public EMovementType MovementType { get; }

		public int RemainingAmount => Requirement.Amount - DeliveredAmount;
		public bool IsCompleted => RemainingAmount == 0;

		public CStaticContractData StaticData { get; }
		public CEventContractData EventData { get; }
		public CPassengerContractData PassengerData { get; }
		public CFleetContractData FleetData { get; }
		public bool IsFleetTask => FleetData != null;

		public CContract(
			EContractType type,
			SResource requirement,
			int deliveredAmount,
			CTripPrice tripPrice,
			IValuable[] rewards,
			ECustomer customer,
			string uid,
			bool isSeen,
			ERegion[] regions,
			EMovementType movementType,
			CStaticContractData staticData = null,
			CEventContractData eventData = null,
			CPassengerContractData passengerData = null,
			CFleetContractData fleetData = null
			)
		{
			Type = type;
			Requirement = requirement;
			DeliveredAmount = deliveredAmount;
			TripPrice = tripPrice;
			Rewards = rewards;
			Customer = customer;
			Uid = uid;
			IsSeen = isSeen;
			Regions = regions;
			MovementType = movementType;
			StaticData = staticData;
			EventData = eventData;
			PassengerData = passengerData;
			FleetData = fleetData;
		}

		public void AddDeliveredAmount(int amount)
		{
			DeliveredAmount += amount;
		}

		public void MarkAsSeen()
		{
			IsSeen = true;
		}

		public SStaticContractPointer GetPointer()
		{
			return new SStaticContractPointer(StaticData.ContractId, StaticData.Task);
		}

		public void Activate()
		{
			StaticData.Activate();
		}
	}
}
