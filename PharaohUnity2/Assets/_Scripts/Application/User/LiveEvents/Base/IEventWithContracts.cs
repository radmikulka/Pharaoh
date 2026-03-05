// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.01.2026
// =========================================

using System.Collections.Generic;
using ServerData;

namespace TycoonBuilder
{
	public interface IEventWithContracts : ILiveEventContent
	{
		ERegion Region { get; }
		IEnumerable<CContract> AllActiveContracts();
		bool IsContractCompleted(EStaticContractId id);
		bool RemoveContract(EStaticContractId contractId);
		CContract GetContractOrDefault(EStaticContractId id);
		void Sync(CContract contract);
		void AddCompletedContract(CContract contract);
	}
}
