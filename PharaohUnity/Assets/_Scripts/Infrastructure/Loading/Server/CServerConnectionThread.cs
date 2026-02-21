// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.Tcp;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using Pharaoh;
using UnityEngine;

namespace Pharaoh
{
	public class CServerConnectionThread
	{
		private readonly CDebugUserDeletionHandler _debugUserDeletionHandler;
		private readonly CServerEndpointProvider _serverEndpointProvider;
		private readonly CHitsDispatcher _hitsDispatcher;
		private readonly ITranslation _translation;
		private readonly CHitBuilder _hitBuilder;

		public CServerConnectionThread(
			CDebugUserDeletionHandler debugUserDeletionHandler, 
			CServerEndpointProvider serverEndpointProvider,
			CHitsDispatcher hitsDispatcher, 
			ITranslation translation, 
			CHitBuilder hitBuilder
			)
		{
			_debugUserDeletionHandler = debugUserDeletionHandler;
			_serverEndpointProvider = serverEndpointProvider;
			_hitsDispatcher = hitsDispatcher;
			_translation = translation;
			_hitBuilder = hitBuilder;
		}

		public async UniTask<CResponseHit> ConnectAsync(CancellationToken ct)
		{
			 _serverEndpointProvider.Reload();
			_hitsDispatcher.CreateNewTcpClient(_serverEndpointProvider.ActiveEndPoint);
			
			await _debugUserDeletionHandler.TryDeleteUserAsync();
			
			CResponseHit response = await SendConnectHitAsync(ct);
			return response;
		}
		
		private async UniTask<CResponseHit> SendConnectHitAsync(CancellationToken ct)
		{
			CConnectRequest connectRequest = new(SystemInfo.deviceUniqueIdentifier, CServerConfig.Instance.PresetId);
			
			CAsyncHitResponse<CResponseHit> response = await _hitBuilder.GetBuilder(connectRequest)
				.SetSuppressAutomaticErrorHandling()
				.SetSendAsSingleHit()
				.BuildAndSendAsync<CResponseHit>(ct);
			
			return response.Response;
		}
	}
}