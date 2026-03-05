// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CAccountDto
	{
		[JsonProperty] public string PublicId { get; set; }
		[JsonProperty] public string PublicIdShort { get; set; }
		[JsonProperty] public string FacebookUserId { get; set; }
		[JsonProperty] public string EncryptedUid { get; set; }
		[JsonProperty] public string Nickname { get; set; }
		[JsonProperty] public EProfileAvatar Avatar { get; set; }
		[JsonProperty] public EProfileFrame Frame { get; set; }
		[JsonProperty] public int StoreVersion { get; set; }
		[JsonProperty] public ECountryCode CountryCode { get; set; }
		[JsonProperty] public bool IsTestUser { get; set; }
		[JsonProperty] public bool FirstEventSeen { get; set; }
		[JsonProperty] public EPrivacyConsentStatus PrivacyConsent { get; set; }

		public CAccountDto()
		{
		}

		public CAccountDto(
			string publicId, 
			string publicIdShort, 
			string facebookUserId, 
			string encryptedUid,
			int storeVersion,
			ECountryCode countryCodeCode,
			string nickname,
			EProfileAvatar avatar,
			EProfileFrame frame,
			bool isTestUser,
			EPrivacyConsentStatus privacyConsent,
			bool firstEventSeen
			)
		{
			PublicId = publicId;
			Nickname = nickname;
			Avatar = avatar;
			Frame = frame;
			IsTestUser = isTestUser;
			PublicIdShort = publicIdShort;
			FirstEventSeen = firstEventSeen;
			PrivacyConsent = privacyConsent;
			FacebookUserId = facebookUserId;
			EncryptedUid = encryptedUid;
			StoreVersion = storeVersion;
			CountryCode = countryCodeCode;
		}
	}
}