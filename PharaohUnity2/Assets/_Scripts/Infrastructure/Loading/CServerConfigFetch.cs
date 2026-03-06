// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServiceEngine;
using Random = UnityEngine.Random;
// ReSharper disable UnassignedReadonlyField

namespace Pharaoh
{
	internal class CServerConfigFetch
	{
		private readonly IRemoteDatabase _database;

		public CServerConfigFetch(IRemoteDatabase database)
		{
			_database = database;
		}

		public async UniTask<CRemoteServerConfig> FetchServerConfig(CancellationToken ct)
		{
			string version = GetVersionInFirebaseFormat();
			
			string jsonText = await _database.TryGetValueAsync(ct, "ServerConfig", version);
			CRemoteServerConfig result = JsonUtility.FromJson<CRemoteServerConfig>(jsonText);
			
			return result;
		}

		private string GetVersionInFirebaseFormat()
		{
			string version = CConfigVersion.Instance.Version;
			string result = version.Replace('.', '_');
			return result;
		}
	}
}