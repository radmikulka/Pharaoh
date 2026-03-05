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

namespace TycoonBuilder
{
	public class CAuthService : IAuthService
	{
		private readonly CLoggedAuthServices _loggedServices = new();
		private readonly CAuthUidStorage _authUidStorage;

		public CEvent OnAuthChanged => _loggedServices.OnAuthChanged;

		public CAuthService(IActiveAuth authUidStorage)
		{
			_authUidStorage = (CAuthUidStorage) authUidStorage;
		}

		public bool IsSignedIn(EAuthType authType)
		{
			return _loggedServices.Contains(authType);
		}

		public void InitAuth(string authUid, EAuthType[] loggedServices)
		{
			_authUidStorage.SetAuthUid(authUid);
			_loggedServices.Replace(loggedServices);
		}

		public string GetAuthUidOrDefault()
		{
			return _authUidStorage.AuthUid;
		}
	}
}