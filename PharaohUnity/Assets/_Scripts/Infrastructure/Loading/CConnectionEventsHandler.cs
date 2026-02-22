// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData.Hits;
using ServiceEngine;
using ServiceEngine.Ads;
using ServiceEngine.ServiceMaster;
using UnityEngine;

namespace Pharaoh
{
	public class CConnectionEventsHandler
	{
		private readonly CInitialUserDataProvider _initialUserDataProvider;
		private readonly IGameTime _gameTime;

		public CConnectionEventsHandler(CInitialUserDataProvider initialUserDataProvider, IGameTime gameTime)
		{
			_initialUserDataProvider = initialUserDataProvider;
			_gameTime = gameTime;
		}

		public void PreprocessServerConnection()
		{
			_gameTime.Init(CUnixTime.TimestampInMs());
		}
		
		public void PostprocessServerConnection(CConnectResponse response)
		{
			_initialUserDataProvider.Dto = response.User;
		}
	}
}