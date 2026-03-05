// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using AldaEngine.Tcp;

namespace ServerData
{
	[NonLazy]
	public class CClientEndPoints
	{
		private readonly CTcpServerEndPointsCollection _endPoints;
		
		public CClientEndPoints(CTcpServerEndPointsCollection endPoints)
		{
			_endPoints = endPoints;
			InitEndPoints();
		}

		private void InitEndPoints()
		{
			_endPoints.AddEndPoint(EServerType.Develop, EServerId.MainServer, "3.251.51.196", 1720);
			_endPoints.AddEndPoint(EServerType.Approval, EServerId.MainServer, "108.129.15.82", 1710);
			_endPoints.AddEndPoint(EServerType.Master, EServerId.MainServer, "108.129.15.82", 1700);
		}
	}
}