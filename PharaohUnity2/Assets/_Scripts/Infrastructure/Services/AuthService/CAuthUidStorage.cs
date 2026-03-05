// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.11.2023
// =========================================

using System.Collections.Generic;
using AldaEngine;
using TycoonBuilder;

namespace TycoonBuilder
{
	public class CAuthUidStorage : IActiveAuth
	{
		private const string PrefsStorageKey = "TB_auid";
		
		public string AuthUid { get; private set; }

		public CAuthUidStorage()
		{
			TryLoadUid();
		}

		private void TryLoadUid()
		{
			bool willDeleteUser = CDebugUserDeletionHandler.WillDeleteUserInThisSession();
			if(willDeleteUser)
				return;
			AuthUid = CPlayerPrefs.Get<string>(PrefsStorageKey);
		}

		public void SetAuthUid(string authUid)
		{
			AuthUid = authUid;
			CPlayerPrefs.Set(PrefsStorageKey, authUid);
		}
	}
}