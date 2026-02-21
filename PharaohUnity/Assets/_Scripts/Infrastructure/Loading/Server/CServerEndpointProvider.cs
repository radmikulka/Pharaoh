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
		
		public CServerTcpEndPoint ActiveEndPoint { get; private set; }

		public CServerEndpointProvider(CTcpServerEndPointsCollection clientEndPoints)
		{
			_clientEndPoints = clientEndPoints;
		}

		public void Reload()
		{
			if (CServerConfig.Instance.IsLocal || CServerConfig.Instance.ServerType == EServerType.Develop)
			{
				ActiveEndPoint = InitDebugEndPoint();
				return;
			}

			ActiveEndPoint = InitRemoteEndPointAsync();
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

		private CServerTcpEndPoint InitRemoteEndPointAsync()
		{
			CServerTcpEndPoint endPoint = _clientEndPoints.GetEndPointConfig(CServerConfig.Instance.ServerType, EServerId.MainServer);
			return endPoint;
		}
	}
}