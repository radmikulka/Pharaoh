// =========================================
// NAME: Marek Karaba
// DATE: 03.03.2026
// =========================================

using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CContractTaskProvider : IContractTaskProvider
	{
		private readonly CDesignStoryContractConfigs _storyContractConfigs;

		public CContractTaskProvider(CDesignStoryContractConfigs storyContractConfigs)
		{
			_storyContractConfigs = storyContractConfigs;
		}

		public CContractTask GetContractTaskOrDefault(SStaticContractPointer contractPointer)
		{
			CContractTask contractTask = _storyContractConfigs.GetTaskOrDefault(contractPointer);
			return contractTask;
		}
	}
}