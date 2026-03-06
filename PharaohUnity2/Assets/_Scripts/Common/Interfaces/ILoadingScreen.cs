// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public interface ILoadingScreen
	{
		UniTask Show(CancellationToken ct, float duration = 0.2f);
		UniTask Hide(CancellationToken ct, float duration = 0.2f);
		void SetInfoText(string text, bool localized);
		void UpdateProgressBar(float progress);
		void SetActiveProgressBar(bool active);
	}
}