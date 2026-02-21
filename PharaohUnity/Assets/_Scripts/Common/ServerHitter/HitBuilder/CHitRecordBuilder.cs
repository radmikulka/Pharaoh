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
	public class CHitRecordBuilder
	{
		private readonly CHitBuilder _hitBuilder;
		public CRequestHit Hit { get; private set; }
		public Action<CResponseHit> OnSuccess { get; private set; }
		public Action<EErrorCode> OnFail { get; private set; }
		public bool ExecuteImmediately { get; private set; }
		public bool SendAsSingleHit { get; private set; }
		public bool SuppressAutomaticErrorHandling { get; private set; }
		
		public CHitRecordBuilder(CRequestHit hit, CHitBuilder hitBuilder)
		{
			_hitBuilder = hitBuilder;
			Hit = hit;
		}
		
		public CHitRecordBuilder SetExecuteImmediately()
		{
			ExecuteImmediately = true;
			return this;
		}

		public CHitRecordBuilder SetSuppressAutomaticErrorHandling()
		{
			SuppressAutomaticErrorHandling = true;
			SetSendAsSingleHit();
			SetExecuteImmediately();
			return this;
		}

		public CHitRecordBuilder SetSendAsSingleHit()
		{
			SendAsSingleHit = true;
			SetExecuteImmediately();
			return this;
		}

		public CHitRecordBuilder SetOnSuccess<T>(Action<T> callback) where T : CResponseHit
		{
			OnSuccess = responseHit => callback?.Invoke(responseHit as T);
			return this;
		}

		public CHitRecordBuilder SetOnFail(Action<EErrorCode> callback)
		{
			OnFail = callback;
			return this;
		}

		public void BuildAndSend()
		{
			_hitBuilder.BuildAndSend(this);
		}
		
		public async UniTask<CAsyncHitResponse<T>> BuildAndSendAsync<T>(CancellationToken ct) where T : CResponseHit
		{
			return await _hitBuilder.BuildAndSendAsync<T>(this, ct);
		}
	}
}