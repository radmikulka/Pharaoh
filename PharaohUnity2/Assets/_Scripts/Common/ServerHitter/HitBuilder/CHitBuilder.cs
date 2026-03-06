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
	public class CHitBuilder
	{
		private readonly CCommunicationTokenProvider _communicationTokenProvider;
		private readonly CHitsDispatcher _hitDispatcher;
		private readonly IServerTime _serverTime;

		public CHitBuilder(
			CCommunicationTokenProvider communicationTokenProvider, 
			CHitsDispatcher hitDispatcher, 
			IServerTime serverTime
			)
		{
			_communicationTokenProvider = communicationTokenProvider;
			_hitDispatcher = hitDispatcher;
			_serverTime = serverTime;
		}

		public CHitRecordBuilder GetBuilder(CRequestHit hit)
		{
			return new CHitRecordBuilder(hit, this);
		}

		public void BuildAndSend(CHitRecordBuilder recordBuilder)
		{
			recordBuilder.InitCommTokenRequest(_communicationTokenProvider.CommunicationToken, _serverTime.GetTimestampInMs());
			
			CHitRecord hitRecord = new(
				recordBuilder.SuppressAutomaticErrorHandling,
				recordBuilder.OnSuccess,
				recordBuilder.ExecuteImmediately,
				recordBuilder.SendAsSingleHit,
				recordBuilder.Hit,
				recordBuilder.OnFail);
			
			_hitDispatcher.Enqueue(hitRecord);
		}

		public async UniTask<CAsyncHitResponse<T>> BuildAndSendAsync<T>(CHitRecordBuilder recordBuilder, CancellationToken ct) where T : CResponseHit
		{
			T response = null;
			EErrorCode? errorCode = null;
			bool completed = false;
			
			recordBuilder.InitCommTokenRequest(_communicationTokenProvider.CommunicationToken, _serverTime.GetTimestampInMs());
			
			CHitRecord hitRecord = new(
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

			return new CAsyncHitResponse<T>(errorCode, response);
		}
	}
}