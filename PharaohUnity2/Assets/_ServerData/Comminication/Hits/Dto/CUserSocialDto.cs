// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.02.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CUserSocialDto : IMapAble
	{
		[JsonProperty] public string EncryptedUid { get; set; }
		[JsonProperty] public string NickName { get;  set; }
		[JsonProperty] public EProfileFrame Frame { get; set; }
		[JsonProperty] public EProfileAvatar Avatar { get; set; }
		[JsonProperty] public bool IsOnline { get; set; }

		public CUserSocialDto()
		{
		}

		public CUserSocialDto(
			string encryptedUid, 
			string nickName, 
			EProfileFrame frame, 
			EProfileAvatar avatar, 
			bool isOnline
		)
		{
			EncryptedUid = encryptedUid;
			NickName = nickName;
			Frame = frame;
			Avatar = avatar;
			IsOnline = isOnline;
		}
	}
}