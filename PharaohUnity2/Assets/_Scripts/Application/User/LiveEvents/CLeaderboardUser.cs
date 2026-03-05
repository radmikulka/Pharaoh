// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.01.2026
// =========================================

namespace TycoonBuilder
{
	public class CLeaderboardUser
	{
		public CUserSocial UserSocial { get; set; }
		public int Points { get; set; }
		public long PointsChangeTimestamp { get; set; }
		
		public CLeaderboardUser(CUserSocial userSocial, int points, long pointsChangeTimestamp)
		{
			UserSocial = userSocial;
			Points = points;
			PointsChangeTimestamp = pointsChangeTimestamp;
		}
	}
}