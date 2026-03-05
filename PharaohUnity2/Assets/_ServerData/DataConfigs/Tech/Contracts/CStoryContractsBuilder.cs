// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CStoryContractsBuilder
	{
		private readonly List<CStoryContractBuilder> _storyContracts = new(40);
		
		public readonly ERegion Region;

		public CStoryContractsBuilder(ERegion region)
		{
			Region = region;
		}

		public CStoryContractBuilder CreateStoryContract(EStaticContractId id)
		{
			CStoryContractBuilder builder = new(id);
			builder.SetRegion(Region);
			_storyContracts.Add(builder);
			return builder;
		}
        
		public CStoryContractConfig[] Build()
		{
			CStoryContractConfig[] contracts = new CStoryContractConfig[_storyContracts.Count];
			for (int i = 0; i < _storyContracts.Count; i++)
			{
				contracts[i] = _storyContracts[i].Build();
			}
			return contracts;
		}
	}
}