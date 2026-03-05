// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CGetStoryContractNameTagPointRequest
	{
		public readonly EStaticContractId ContractId;
		
		public CGetStoryContractNameTagPointRequest(EStaticContractId contractId)
		{
			ContractId = contractId;
		}
	}

	public class CGetStoryContractNameTagPointResponse
	{
		public readonly Vector3 Point;
		
		public CGetStoryContractNameTagPointResponse(Vector3 point)
		{
			Point = point;
		}
	}
}