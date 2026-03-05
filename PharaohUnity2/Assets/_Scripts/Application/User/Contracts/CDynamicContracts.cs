// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.09.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;

namespace TycoonBuilder
{
	public class CDynamicContracts
	{
		private readonly Dictionary<string, CContract> _contracts = new();

		public void AddContract(CContract contract)
		{
			_contracts.Add(contract.Uid, contract);
		}

		public bool RemoveContract(string uid)
		{
			return _contracts.Remove(uid);
		}

		public CContract GetContract(string uid)
		{
			return _contracts[uid];
		}

		public CContract GetPassengerContractOrDefault(ECity cityId)
		{
			foreach (KeyValuePair<string, CContract> contract in _contracts)
			{
				if (contract.Value.Type != EContractType.Passenger)
					continue;
				if(contract.Value.PassengerData.CityId != cityId)
					continue;
				return contract.Value;
			}

			return null;
		}

		public CContract[] GetAllPassengerContracts()
		{
			return _contracts.Where(contract => contract.Value.Type == EContractType.Passenger)
				.Select(contract => contract.Value)
				.ToArray()
				;
		}
	}
}
