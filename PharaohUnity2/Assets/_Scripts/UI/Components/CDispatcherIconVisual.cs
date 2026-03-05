// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDispatcherIconVisual : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentImage _image;
		
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		
		[Inject]
		private void Inject(CResourceConfigs resourceConfigs, IBundleManager bundleManager)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
		}

		public void SetVisual(EDispatcher dispatcherId)
		{
			CDispatcherConfig config = _resourceConfigs.Dispatchers.GetConfig(dispatcherId);
			Sprite dispatcherSprite = _bundleManager.LoadItem<Sprite>(config.Sprite);
			_image.SetSprite(dispatcherSprite);
		}
	}
}