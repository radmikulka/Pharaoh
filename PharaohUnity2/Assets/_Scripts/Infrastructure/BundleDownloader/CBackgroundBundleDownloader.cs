// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using Zenject;

namespace Pharaoh
{
	public class CBackgroundBundleDownloader : IInitializable
	{
		private readonly IBundleManager _bundleManager;
		private readonly ICtsProvider _ctsProvider;

		public CBackgroundBundleDownloader(
			IBundleManager bundleManager, 
			ICtsProvider ctsProvider
			)
		{
			_bundleManager = bundleManager;
			_ctsProvider = ctsProvider;
		}

		public void Initialize()
		{
			//DownloadAndLoadBundles(new []{(int)EBundleId.Region1Environment_2}, _ctsProvider.Token).Forget();
		}

		private async UniTask DownloadAndLoadBundles(int[] bundles, CancellationToken ct)
		{
			bool completed = _bundleManager.AreBundlesLoaded(bundles);
			if(completed)
				return;
			
			while (true)
			{
				bool result = await _bundleManager.LoadBundlesAsync(bundles, ct);
				if (result)
				{
					return;
				}
				await UniTask.WaitForSeconds(10f, cancellationToken: ct);
			}
		}
	}
}