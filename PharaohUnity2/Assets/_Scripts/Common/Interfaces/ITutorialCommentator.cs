// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.10.2025
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder
{
	public interface ITutorialCommentator
	{
		public enum ESide
		{
			Left,
			Right
		}
		
		UniTask ShowCommentator(ESide side, string langKey, bool showTapAnywhere, CancellationToken ct);
		UniTask ShowText(string langKey, bool showTapAnywhere, CancellationToken ct);
		UniTask Hide(CancellationToken ct);
		bool IsRunning { get; }
	}
}