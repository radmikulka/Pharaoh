// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2023
// =========================================

using System;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Hits;
using ServiceEngine.Ads;
using Pharaoh;
using Zenject;

namespace Pharaoh
{
	public class CAdsManager : CBaseAdsManager
	{
		private CRequestSender _hitBuilder;

		[Inject]
		private void Inject(CRequestSender hitBuilder)
		{
			_hitBuilder = hitBuilder;
		}
		
		public override void Initialize()
		{
			base.Initialize();

			EventBus.Subscribe<CAdFailedSignal>(OnAdFailed);
		}
		
		private void OnAdFailed(CAdFailedSignal signal)
		{
			EventBus.ProcessTask(new CShowTooltipTask(signal.FailReason == EEditorFailReason.Closed ? "Generic.AdClosed" : "Generic.AdFailed", true));
		}

		public override bool CanPlayInterstitialAd()
		{
			bool isUserEligible = IsUserEligibleToPlayInterstitialAd();
			return isUserEligible;
		}
		
		private bool IsUserEligibleToPlayInterstitialAd()
		{
			return false;
		}

		private double GetAdRevenue(IAdInfo adInfo)
		{
			return adInfo.Revenue;
		}
	}
}