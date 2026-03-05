// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CUserSocial
	{
		public string EncryptedUid { get; set; }
		public string NickName { get;  set; }
		public EProfileFrame Frame { get; set; }
		public EProfileAvatar Avatar { get; set; }
		public bool IsOnline { get; set; }

		public CUserSocial(
			string encryptedUid,
			string nickName,
			EProfileFrame frame,
			EProfileAvatar avatar,
			bool isOnline)
		{
			EncryptedUid = encryptedUid;
			NickName = nickName;
			Frame = frame;
			Avatar = avatar;
			IsOnline = isOnline;
		}
	}
}