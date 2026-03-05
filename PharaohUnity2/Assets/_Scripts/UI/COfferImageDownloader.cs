// =========================================
// AUTHOR: Juraj Joscak
// DATE:   28.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class COfferImageDownloader : CImageDownloader, IInitializable
	{
		private CUser _userData;
		
		[Inject]
		private void Inject(CUser userData)
		{
			_userData = userData;
		}

		public void Initialize()
		{
			TryDownloadImages();
		}
		
		private void TryDownloadImages()
		{
			foreach (COffer offer in _userData.Offers.GetOffersWithParam(EOfferParam.BackgroundImage))
			{
				string url = offer.GetParamValue<string>(EOfferParam.BackgroundImage);
				
				if (DownloadStarted(url))
					continue;
				
				StartDownload(url);
			}
		}
	}
}