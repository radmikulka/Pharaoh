// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.10.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder
{
	public class CTutorialCommentatorProxy : ITutorialCommentator
	{
		private ITutorialCommentator _commentator;

		public bool IsRunning => _commentator.IsRunning;

		public void SetInstance(ITutorialCommentator commentator)
		{
			_commentator = commentator;
		}
		
		public async UniTask ShowCommentator(ITutorialCommentator.ESide side, string langKey, bool showTapAnywhere, CancellationToken ct)
		{
			await _commentator.ShowCommentator(side, langKey, showTapAnywhere, ct);
		}

		public async UniTask ShowText(string langKey, bool showTapAnywhere, CancellationToken ct)
		{
			await _commentator.ShowText(langKey, showTapAnywhere, ct);
		}

		public async UniTask Hide(CancellationToken ct)
		{
			await _commentator.Hide(ct);
		}
	}
}