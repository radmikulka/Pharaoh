// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.12.2024
// =========================================

using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CSeenCompetitorDto
	{
		[JsonProperty] public string EncryptedUid { get; set; }
		[JsonProperty] public int LastSeenPoints { get; set; }

		public CSeenCompetitorDto()
		{
		}

		public CSeenCompetitorDto(string encryptedUid, int lastSeenPoints)
		{
			EncryptedUid = encryptedUid;
			LastSeenPoints = lastSeenPoints;
		}
	}
}