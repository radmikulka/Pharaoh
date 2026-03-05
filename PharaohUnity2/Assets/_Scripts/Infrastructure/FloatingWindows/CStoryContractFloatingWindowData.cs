// =========================================
// AUTHOR: Marek Karaba
// DATE:   07.08.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CStoryContractFloatingWindowData : CFloatingWindowData
	{
		public readonly EStaticContractId ContractId;
		public bool IsActivated { get; private set; }
		
		public CStoryContractFloatingWindowData(
			EStaticContractId contractId, 
			bool isActivated,
			Transform worldPoint, 
			Transform ownerTransform
			) : base(worldPoint, ownerTransform)
		{
			ContractId = contractId;
			IsActivated = isActivated;
		}
	}
}