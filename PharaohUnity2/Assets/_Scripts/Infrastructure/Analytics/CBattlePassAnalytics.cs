// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
	public class CBattlePassAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;
		
		private readonly Dictionary<string, object> _cachedParams = new();

		public CBattlePassAnalytics(IAnalytics analytics, IEventBus eventBus, CUser user)
		{
			_analytics = analytics;
			_eventBus = eventBus;
			_user = user;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CDecadePassRewardClaimedSignal>(OnDecadePassRewardClaimed);
			_eventBus.Subscribe<CEventPassRewardClaimedSignal>(OnEventPassRewardClaimed);
		}

		private void OnDecadePassRewardClaimed(CDecadePassRewardClaimedSignal signal)
		{
			_cachedParams.Clear();
			string passType = "DecadePass";
			string category = signal.IsPremium ? "Premium" : "Free";
			int index = signal.RewardIndex;
			string rewardName = signal.Reward.Id.ToString();
			bool isDoubleReward = signal.IsDoubled;
			string rewardValue = signal.Reward.GetAnalyticsValue();
			EBattlePassPremiumStatus passStatus = _user.DecadePass.PremiumStatus;
			
			_cachedParams.Add("PassType", passType);
			_cachedParams.Add("Category", category);
			_cachedParams.Add("Index", index);
			_cachedParams.Add("RewardName", rewardName);
			_cachedParams.Add("RewardValue", rewardValue);
			_cachedParams.Add("IsDoubleReward", isDoubleReward);
			_cachedParams.Add("PassStatus", passStatus.ToString());
			
			_analytics.SendData("PassRewardClaim", _cachedParams);
		}

		private void OnEventPassRewardClaimed(CEventPassRewardClaimedSignal signal)
		{
			_cachedParams.Clear();
			string passType = "EventPass";
			string category = signal.IsPremium ? "Premium" : "Free";
			int index = signal.RewardIndex;
			string rewardName = signal.Reward.Id.ToString();
			bool isDoubleReward = signal.IsDoubled;
			string rewardValue = signal.Reward.GetAnalyticsValue();
			EBattlePassPremiumStatus passStatus = signal.Status;
			
			_cachedParams.Add("PassType", passType);
			_cachedParams.Add("Category", category);
			_cachedParams.Add("Index", index);
			_cachedParams.Add("RewardName", rewardName);
			_cachedParams.Add("RewardValue", rewardValue);
			_cachedParams.Add("IsDoubleReward", isDoubleReward);
			_cachedParams.Add("PassStatus", passStatus.ToString());
			
			_analytics.SendData("PassRewardClaim", _cachedParams);
		}
	}
}