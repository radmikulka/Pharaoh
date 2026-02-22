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
		private readonly CHitsDispatcher _hitDispatcher;

		public CHitBuilder(CHitsDispatcher hitDispatcher)
		{
			_hitDispatcher = hitDispatcher;
		}

		public CHitRecordBuilder GetBuilder(CRequestHit hit)
		{
			return new CHitRecordBuilder(hit, this);
		}

		public void BuildAndSend(CHitRecordBuilder recordBuilder)
		{
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