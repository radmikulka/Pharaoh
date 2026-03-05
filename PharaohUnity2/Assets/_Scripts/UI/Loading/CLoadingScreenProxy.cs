// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder
{
	public class CLoadingScreenProxy : ILoadingScreen
	{
		private ILoadingScreen _instance;
        
		public void RegisterInstance(ILoadingScreen instance)
		{
			_instance = instance;
		}

		public async UniTask Show(CancellationToken ct, float duration)
		{
			await _instance.Show(ct, duration);
		}

		public async UniTask Hide(CancellationToken ct, float duration)
		{
			await _instance.Hide(ct, duration);
		}

		public void SetInfoText(string text, bool localized)
		{
			_instance.SetInfoText(text, localized);
		}

		public void UpdateProgressBar(float progress)
		{
			_instance.UpdateProgressBar(progress);
		}

		public void SetActiveProgressBar(bool active)
		{
			_instance.SetActiveProgressBar(active);
		}
	}
}