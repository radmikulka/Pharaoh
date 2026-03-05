// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.01.2026
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CLeaderboard
	{
		private static readonly CLeaderboardComparer LeaderboardComparer = new();
		public CLeaderboardUser[] Competitors { get; private set; }
		public CLeaderboardReward[] Rewards { get; }
		public int PhaseIndex { get; private set; }
		public long EndTime { get; }
		public string Uid { get; }

		public CLeaderboard(CLeaderboardUser[] competitors, CLeaderboardReward[] rewards, long endTime, string uid, int phaseIndex)
		{
			Competitors = competitors;
			PhaseIndex = phaseIndex;
			Rewards = rewards;
			EndTime = endTime;
			Uid = uid;
		}
		
		public long GetRemainingTime(long currentTime)
		{
			long remainingTime = CMath.Max(0, EndTime - currentTime);
			return remainingTime;
		}
		
		public bool IsFinished(long currentTime)
		{
			return currentTime >= EndTime;
		}
		
		public CLeaderboardReward GetRewardForRank(int rank)
		{
			foreach (CLeaderboardReward reward in Rewards)
			{
				if (rank <= reward.MinRank)
				{
					return reward;
				}
			}
			return null;
		}

		public int GetPointsForUid(string uid)
		{
			foreach (CLeaderboardUser competitor in Competitors)
			{
				if (competitor.UserSocial.EncryptedUid == uid)
				{
					return competitor.Points;
				}
			}
			return -1;
		}

		public int GetRankForUid(string uid)
		{
			CLeaderboardUser[] sortedCompetitors = GetSortedCompetitors();
			for (int i = 0; i < sortedCompetitors.Length; i++)
			{
				if (sortedCompetitors[i].UserSocial.EncryptedUid == uid)
				{
					return i + 1;
				}
			}
			return -1;
		}

		public CLeaderboardUser[] GetSortedCompetitors()
		{
			List<CLeaderboardUser> allCompetitors = Competitors.ToList();
			allCompetitors.Sort(LeaderboardComparer);
			return allCompetitors.ToArray();
		}

		private class CLeaderboardComparer : IComparer<CLeaderboardUser>
		{
			public int Compare(CLeaderboardUser a, CLeaderboardUser b)
			{
				if (a.Points != b.Points)
					return b.Points.CompareTo(a.Points);
				if (a.PointsChangeTimestamp != b.PointsChangeTimestamp)
					return a.PointsChangeTimestamp.CompareTo(b.PointsChangeTimestamp);
				return string.Compare(a.UserSocial.EncryptedUid, b.UserSocial.EncryptedUid, StringComparison.Ordinal);
			}
		}

		public void UpdateCompetitorsWithComplement(CLeaderboardComplement leaderboardComplement)
		{
			List<CLeaderboardUser> newUsers = new(); 
			foreach (CLeaderboardUser modifiedCompetitor in leaderboardComplement.ValuableModifications)
			{
				for (int i = 0; i < Competitors.Length; i++)
				{
					if (Competitors[i].UserSocial.EncryptedUid != modifiedCompetitor.UserSocial.EncryptedUid)
						continue;
					
					Competitors[i] = new CLeaderboardUser(
						modifiedCompetitor.UserSocial,
						modifiedCompetitor.Points,
						modifiedCompetitor.PointsChangeTimestamp);
					break;
				}

				if (Competitors.All(user => user.UserSocial.EncryptedUid != modifiedCompetitor.UserSocial.EncryptedUid))
				{
					newUsers.Add(modifiedCompetitor);
				}
			}
			
			if (newUsers.Count > 0)
			{
				Competitors = Competitors.Concat(newUsers).ToArray();
			}
		}

		public int GetAveragePoints()
		{
			if (Competitors.Length == 0)
				return 0;
			
			int totalPoints = Competitors.Sum(competitor => competitor.Points);
			return totalPoints / Competitors.Length;
		}
	}
}