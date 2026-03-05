// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.02.2026
// =========================================

namespace ServerData
{
	public class CUpgradeFactoryToLevelRequirementConfig : CCountableTaskRequirementConfig
	{
		public readonly EFactory Factory;
		
		public CUpgradeFactoryToLevelRequirementConfig(EFactory factory, int targetCount) 
			: base(ETaskRequirement.UpgradeFactoryToLevel, ETaskRequirementType.Retroactive, targetCount)
		{
			Factory = factory;
		}
	}
}