// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.Tcp;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Hits;

namespace Pharaoh
{
	public class CRequestSender
	{
		private readonly CCommunicationTokenProvider _communicationTokenProvider;
		private readonly CRequestDispatcher _hitDispatcher;
		private readonly IServerTime _serverTime;

		public CRequestSender(
			CCommunicationTokenProvider communicationTokenProvider,
			CRequestDispatcher hitDispatcher,
			IServerTime serverTime
			)
		{
			_communicationTokenProvider = communicationTokenProvider;
			_hitDispatcher = hitDispatcher;
			_serverTime = serverTime;
		}

		public CRequestBuilder GetBuilder(CRequestHit hit)
		{
			return new CRequestBuilder(hit, this);
		}

		public void BuildAndSend(CRequestBuilder recordBuilder)
		{
			recordBuilder.InitCommTokenRequest(_communicationTokenProvider.CommunicationToken, _serverTime.GetTimestampInMs());
			
			CRequestRecord hitRecord = new(
				recordBuilder.SuppressAutomaticErrorHandling,
				recordBuilder.OnSuccess,
				recordBuilder.ExecuteImmediately,
				recordBuilder.SendAsSingleHit,
				recordBuilder.Hit,
				recordBuilder.OnFail);

			_hitDispatcher.Enqueue(hitRecord);
		}

		public async UniTask<CServerResponse<T>> BuildAndSendAsync<T>(CRequestBuilder recordBuilder, CancellationToken ct) where T : CResponseHit
		{
			T response = null;
			EErrorCode? errorCode = null;
			bool completed = false;

			recordBuilder.InitCommTokenRequest(_communicationTokenProvider.CommunicationToken, _serverTime.GetTimestampInMs());

			CRequestRecord hitRecord = new(
				recordBuilder.SuppressAutomaticErrorHandling,
				hit =>
				{
					response = hit as T;
					recordBuilder.OnSuccess?.Invoke(hit);
					completed = true;
				},
				recordBuilder.ExecuteImmediately,
				recordBuilder.SendAsSingleHit,
				recordBuilder.Hit,
				error =>
				{
					recordBuilder.OnFail?.Invoke(error);
					errorCode = error;
					completed = true;
				});
			
			_hitDispatcher.Enqueue(hitRecord);
			await UniTask.WaitUntil(() => completed, cancellationToken: ct);

			return new CServerResponse<T>(errorCode, response);
		}
	}
}