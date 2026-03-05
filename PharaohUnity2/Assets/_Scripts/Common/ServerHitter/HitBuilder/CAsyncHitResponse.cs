// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.11.2023
// =========================================

namespace TycoonBuilder
{
	public class CAsyncHitResponse<T>
	{
		public readonly EErrorCode? ErrorCode;
		public readonly T Response;

		public CAsyncHitResponse(EErrorCode? errorCode, T response)
		{
			ErrorCode = errorCode;
			Response = response;
		}
	}
}