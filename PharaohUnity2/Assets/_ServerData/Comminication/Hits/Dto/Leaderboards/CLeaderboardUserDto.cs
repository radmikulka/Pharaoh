// =========================================
// AUTHOR: Radek Mikulka
// DATE:   4.3.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CLeaderboardUserDto : IMapAble
	{
		[JsonProperty] public CUserSocialDto UserSocial { get; set; }
		[JsonProperty] public int Points { get; set; }
		[JsonProperty] public long PointsChangeTime { get;  set; }

		public CLeaderboardUserDto()
		{
		}

		public CLeaderboardUserDto(CUserSocialDto userSocial, int points, long pointsChangeTime)
		{
			PointsChangeTime = pointsChangeTime;
			UserSocial = userSocial;
			Points = points;
		}
	}
}