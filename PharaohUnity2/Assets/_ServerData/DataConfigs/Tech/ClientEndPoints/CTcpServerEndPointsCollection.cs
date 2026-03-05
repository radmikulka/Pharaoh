// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.Tcp;

namespace ServerData
{
	public class CTcpServerEndPointsCollection
	{
		private readonly List<CServerTcpEndPoint> _serverEndPoints = new();

		public void AddEndPoint(EServerType serverType, EServerId serverId, string ipAddress, int port)
		{
			CServerTcpEndPoint endPoint = new(serverType, serverId, ipAddress, port);
			_serverEndPoints.Add(endPoint);
		}

		public CServerTcpEndPoint GetEndPointConfig(EServerType type, EServerId serverId)
		{
			return _serverEndPoints.First(point => point.ServerType == type && point.ServerId == serverId);
		}
	}
}