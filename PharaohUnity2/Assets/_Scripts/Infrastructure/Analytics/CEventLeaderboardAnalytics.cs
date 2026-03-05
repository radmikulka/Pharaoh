// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.01.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
	public class CEventLeaderboardAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;
		
		private readonly Dictionary<string, object> _cachedParams = new();

		public CEventLeaderboardAnalytics(IAnalytics analytics, IEventBus eventBus, CUser user)
		{
			_analytics = analytics;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CEventLeaderboardClaimedSignal>(OnEventLeaderboardClaimed);
		}

		private void OnEventLeaderboardClaimed(CEventLeaderboardClaimedSignal signal)
		{
			int rank = signal.Leaderboard.GetRankForUid(_user.Account.EncryptedUid);
			int pointsOnRank = signal.Leaderboard.GetPointsForUid(_user.Account.EncryptedUid);
			int averagePoints = signal.Leaderboard.GetAveragePoints();
			int playersCount = signal.Leaderboard.Competitors.Length;
			int phaseIndex = signal.Leaderboard.PhaseIndex;
			
			_cachedParams.Clear();
			
			_cachedParams.Add("Placement", rank);
			_cachedParams.Add("Stage", phaseIndex);
			_cachedParams.Add("Points", pointsOnRank);
			_cachedParams.Add("AveragePoints", averagePoints);
			_cachedParams.Add("Players", playersCount);
			
			_analytics.SendData("EventLeaderboardEnd", _cachedParams);
		}
	}
}