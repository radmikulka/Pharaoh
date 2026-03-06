// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2023
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using ServiceEngine;
using UnityEngine;
using ILogger = AldaEngine.ILogger;

namespace Pharaoh
{
	public class CAuthService : IAuthService
	{
		private readonly CAuthUidStorage _authUidStorage;

		public CAuthService(IActiveAuth authUidStorage)
		{
			_authUidStorage = (CAuthUidStorage) authUidStorage;
		}

		public void InitAuth(string authUid)
		{
			_authUidStorage.SetAuthUid(authUid);
		}

		public string GetAuthUidOrDefault()
		{
			return _authUidStorage.AuthUid;
		}
	}
}