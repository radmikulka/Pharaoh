// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.08.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenContractDispatchMenuTask
	{
		public readonly SStaticContractPointer ContractId;

		public COpenContractDispatchMenuTask(SStaticContractPointer contractId)
		{
			ContractId = contractId;
		}
	}
	
	public class COpenResourceDispatchMenuTask
	{
		public readonly EIndustry Industry;

		public COpenResourceDispatchMenuTask(EIndustry industry)
		{
			Industry = industry;
		}
	}
	
	public class COpenCityDispatchMenuTask
	{
		public readonly ECity City;

		public COpenCityDispatchMenuTask(ECity city)
		{
			City = city;
		}
	}
}