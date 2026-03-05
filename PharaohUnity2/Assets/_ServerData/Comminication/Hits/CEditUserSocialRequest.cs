// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.07.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CEditUserSocialRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string Nickname { get; set; }
		[JsonProperty] public EProfileAvatar Avatar { get; set; }
		[JsonProperty] public EProfileFrame Frame { get; set; }
		
		public CEditUserSocialRequest() : base(EHit.EditUserSocialRequest)
		{
		}
		
		public CEditUserSocialRequest(string nickname, EProfileAvatar avatar, EProfileFrame frame) : base(EHit.EditUserSocialRequest)
		{
			Nickname = nickname;
			Avatar = avatar;
			Frame = frame;
		}
	}
	
	public class CEditUserSocialResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }

		public CEditUserSocialResponse() : base(EHit.EditUserSocialResponse)
		{
		}

		public CEditUserSocialResponse(CModifiedUserDataDto modifiedData) : base(EHit.EditUserSocialResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}