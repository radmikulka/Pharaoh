// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.10.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace Pharaoh
{
	public class CRequiredBundlesDownloader
	{
		private readonly IRequiredBundlesProvider _requiredBundlesProvider;
		private readonly IBundleManager _bundleManager;
		private readonly ILoadingScreen _loadingScreen;
		private readonly CErrorHandler _errorHandler;

		public CRequiredBundlesDownloader(
			IRequiredBundlesProvider requiredBundlesProvider, 
			IBundleManager bundleManager, 
			ILoadingScreen loadingScreen, 
			CErrorHandler errorHandler
			)
		{
			_requiredBundlesProvider = requiredBundlesProvider;
			_bundleManager = bundleManager;
			_loadingScreen = loadingScreen;
			_errorHandler = errorHandler;
		}

		public async UniTask DownloadBundlesAsync(EMissionId mission, CancellationToken ct)
		{
			int[] bundleIds = _requiredBundlesProvider.GetBundles(mission);
			CBundleLoadResult bundleLoad = _bundleManager.LoadBundles(bundleIds, ct);
			
			while (!bundleLoad.IsDone())
			{
				float progress = bundleLoad.GetDownloadProgress();
				_loadingScreen.SetActiveProgressBar(progress < 1f);
				_loadingScreen.UpdateProgressBar(progress);
				await UniTask.WaitForSeconds(0.1f, cancellationToken: ct);
			}

			if (bundleLoad.Failed)
			{
				_errorHandler.HandleInternalError(EErrorCode.Internal);
				throw new Exception($"Bundle download failed for bundles: {string.Join(", ", bundleIds)}");
			}
			
			_loadingScreen.SetActiveProgressBar(false);
		}
	}
}