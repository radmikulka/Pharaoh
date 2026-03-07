// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using RoboRyanTron.SearchableEnum;
using ServerData;
using Pharaoh;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CUiValuable : MonoBehaviour, IConstructable
	{
		[SerializeField, SearchableEnum] protected EModificationSource _modificationSource;
		[SerializeField] private CUiComponentImage _icon;

		private IUiValuableComponent[] _components;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;

		private IValuable Valuable;
		
		public EValuable Id => Valuable?.Id ?? EValuable.None;

		[Inject]
		private void Inject(CResourceConfigs resourceConfigs, IBundleManager bundleManager)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
		}

		public void Construct()
		{
			_components = GetComponentsInChildren<IUiValuableComponent>(true);
		}

		public void SetValue(IValuable valuable)
		{
			Valuable = valuable;
			SetIcon(Valuable);
			SendToComponents(Valuable);
		}

		private void SetIcon(IValuable valuable)
		{
			switch (valuable)
			{
				default:
					SetValuable(valuable);
					break;
			}
		}

		private void SetValuable(IValuable valuable)
		{
			CValuableResourceConfig config = _resourceConfigs.Valuables.GetConfig(valuable?.Id ?? EValuable.None);
			if (config == null)
			{
				_icon.SetAlpha(0);
				return;
			}
			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.Sprite, EBundleCacheType.Persistent);
			_icon.SetSprite(sprite);
			_icon.SetAlpha(1);
		}

		private void SendToComponents(IValuable valuable)
		{
			foreach (IUiValuableComponent component in _components)
			{
				component.Init(valuable);
			}
		}
	}
}