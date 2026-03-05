// =========================================
// AUTHOR: Marek Karaba
// DATE:   28.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiCountryFlagVisual : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField, Self] private CUiComponentImage _image;

		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		
		[Inject]
		private void Inject(CResourceConfigs resourceConfigs, IBundleManager bundleManager)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
		}

		public void SetCountry(ECountryCode countryCode)
		{
			CCountryFlagConfig config = _resourceConfigs.CountryFlagConfigs.GetConfig(countryCode);
			if (!config)
			{
				config = _resourceConfigs.CountryFlagConfigs.GetConfig(ECountryCode.None);
			}

			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.Sprite);
			SetImage(sprite);
		}

		private void SetImage(Sprite sprite)
		{
			_image.SetSprite(sprite);
		}
	}
}