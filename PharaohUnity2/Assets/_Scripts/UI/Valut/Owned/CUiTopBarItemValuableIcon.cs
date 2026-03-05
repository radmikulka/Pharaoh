// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using AldaEngine;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTopBarItemValuableIcon : CUiTopBarItemIcon
	{
		[SerializeField, Self] private CUiComponentImage _iconImage;
		
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		
		[Inject]
		private void Inject(CResourceConfigs resourceConfigs, IBundleManager bundleManager)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
		}

		internal override void SetIcon(ETopBarItem id)
		{
			Sprite icon;
			if ((int)id < 1000)
			{
				CValuableResourceConfig valuableConfig = _resourceConfigs.Valuables.GetConfig((EValuable)id);
				icon = _bundleManager.LoadItem<Sprite>(valuableConfig.Sprite, EBundleCacheType.Persistent);
			}
			else
			{
				CResourceResourceConfig resourceConfig = _resourceConfigs.Resources.GetConfig((EResource)(id - 1000));
				icon = _bundleManager.LoadItem<Sprite>(resourceConfig.Sprite, EBundleCacheType.Persistent);
			}
			
			_iconImage.SetSprite(icon);
		}
	}
}