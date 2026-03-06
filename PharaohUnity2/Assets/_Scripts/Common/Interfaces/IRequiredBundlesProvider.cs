// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.11.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IRequiredBundlesProvider
	{
		int[] GetBundles(EMissionId mission);
	}
}