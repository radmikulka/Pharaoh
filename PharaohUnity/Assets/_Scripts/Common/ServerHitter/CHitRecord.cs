// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using AldaEngine.Tcp;
using ServerData;
using ServerData.Hits;

namespace Pharaoh
{
	public class CHitRecord
	{
		public readonly CRequestHit Hit;
		public readonly Action<EErrorCode> OnFail;
		public readonly Action<CResponseHit> OnSuccess;
		public readonly DateTime CreationTime;
		public readonly bool ExecuteImmediately;
		public readonly bool SendAsSingleHit;
		public readonly bool SuppressCommunicationErrorThrow;

		public CHitRecord(
			bool suppressCommunicationErrorThrow, 
			Action<CResponseHit> onSuccess, 
			bool executeImmediately, 
			bool sendAsSingleHit, 
			CRequestHit hit, 
			Action<EErrorCode> onFail)
		{
			SendAsSingleHit = sendAsSingleHit || suppressCommunicationErrorThrow;
			SuppressCommunicationErrorThrow = suppressCommunicationErrorThrow;
			ExecuteImmediately = executeImmediately;
			CreationTime = DateTime.UtcNow;
			OnSuccess = onSuccess;
			OnFail = onFail;
			Hit = hit;
		}
	}
}