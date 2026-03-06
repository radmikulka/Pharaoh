// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.Tcp;
using System.Net;
using Cysharp.Threading.Tasks;
using Pharaoh;
using Pharaoh.Loading;
using ServerData;
using UnityEngine;
using ILogger = AldaEngine.ILogger;

namespace Pharaoh
{
	public class CServerEndpointProvider
	{
		private readonly CTcpServerEndPointsCollection _clientEndPoints;
		private readonly IRemoteDatabase _remoteDatabase;
		private readonly ILogger _logger;
		
		public CServerTcpEndPoint ActiveEndPoint { get; private set; }

		public CServerEndpointProvider(
			CTcpServerEndPointsCollection clientEndPoints, 
			IRemoteDatabase remoteDatabase, 
			ILogger logger
			)
		{
			_clientEndPoints = clientEndPoints;
			_remoteDatabase = remoteDatabase;
			_logger = logger;
		}

		public async UniTask Reload(CancellationToken ct)
		{
			if (CServerConfig.Instance.IsLocal || CServerConfig.Instance.ServerType == EServerType.Develop)
			{
				ActiveEndPoint = InitDebugEndPoint();
				return;
			}

			ActiveEndPoint = await InitRemoteEndPointAsync(ct);
		}

		private CServerTcpEndPoint InitDebugEndPoint()
		{
			CServerTcpEndPoint endPointConfig = _clientEndPoints.GetEndPointConfig(CServerConfig.Instance.ServerType, EServerId.MainServer);
			if (CServerConfig.Instance.IsLocal)
			{
				endPointConfig = endPointConfig.WithLocalIpAddress();
			}
			return endPointConfig;
		}

		private async UniTask<CServerTcpEndPoint> InitRemoteEndPointAsync(CancellationToken ct)
		{
			CServerTcpEndPoint fallbackEndPoint = _clientEndPoints.GetEndPointConfig(CServerConfig.Instance.ServerType, EServerId.MainServer);
			
			try
			{
				CServerConfigFetch configFetch = new(_remoteDatabase);
				CRemoteServerConfig remoteServerConfig = await configFetch.FetchServerConfig(ct);
				if (remoteServerConfig == null)
					return fallbackEndPoint;

				CServerTcpEndPoint remoteEndPoint = new(
					remoteServerConfig.ServerType,
					EServerId.MainServer, 
					remoteServerConfig.Ip, 
					remoteServerConfig.Port);

				return remoteEndPoint;
			}
			catch (Exception e)
			{
				_logger.LogError(e);
				return fallbackEndPoint;
			}
		}
	}
}