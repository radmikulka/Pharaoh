// =========================================
// NAME: Marek Karaba
// DATE: 03.03.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IContractTaskProvider
	{
		CContractTask GetContractTaskOrDefault(SStaticContractPointer contractPointer);
	}
}