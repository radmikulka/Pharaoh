// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public interface IGoToHandler
	{
		public void GoToContract(EStaticContractId contractId);
		public void GoToContractAndClaim(EStaticContractId contractId);
		public bool GoToResourceMine(EIndustry industry);
		UniTask GoToRegionPoint(ERegionPoint regionPoint, ERegion region, CancellationToken ct);
		void GoToRegionPointInstant(ERegionPoint regionPoint, ERegion region);
		UniTask GoToCityAndUpgradeIt(CancellationToken ct);
		public bool GoToFactory(EFactory factory, EResource resource);
		void GetMoreMaterial(SResource resource, string source);
		void GoToShop(EShopTab maintenance);
		void GoToSideCity(ECity city, ERegion region);
		void GoToCityUpgradeItAndGoBackToMenu();
		void GoToMainCity(ERegionPoint regionPoint, ERegion region);
		void TryKillActiveGoTo();
		void GoToRegionalOfficeDispatch(EIndustry industry);
		void GoToDailyTask(ETaskId taskId);
	}
}