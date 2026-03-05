// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.12.2025
// =========================================

using ServerData;

namespace TycoonBuilder.Ui
{
	public class CUpgradeAnimatedFactoryItem : CUpgradeAnimatedItem
	{
		public EResource ResourceId { get; private set; }
		
		public void SetResource(EResource resourceId)
		{
			ResourceId = resourceId;
		}
	}
}