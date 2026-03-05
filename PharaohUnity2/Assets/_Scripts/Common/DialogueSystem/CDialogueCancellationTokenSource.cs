// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Threading;

namespace TycoonBuilder
{
	public class CDialogueCancellationTokenSource
	{
		public readonly CancellationTokenSource UserCancellationTokenSource;
		public readonly CancellationTokenSource ApplicationCancellationTokenSource;
		public readonly CancellationTokenSource CombinedCts;

		public CDialogueCancellationTokenSource(CancellationTokenSource userCancellationTokenSource, CancellationTokenSource applicationCancellationTokenSource)
		{
			UserCancellationTokenSource = userCancellationTokenSource;
			ApplicationCancellationTokenSource = applicationCancellationTokenSource;

			CombinedCts =  CancellationTokenSource.CreateLinkedTokenSource(
				userCancellationTokenSource.Token,
				applicationCancellationTokenSource.Token
			);
		}
	}
}