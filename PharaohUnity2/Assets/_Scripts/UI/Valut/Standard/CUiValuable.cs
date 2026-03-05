// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using RoboRyanTron.SearchableEnum;
using ServerData;
using TycoonBuilder;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiValuable : MonoBehaviour, IConstructable
	{
		[SerializeField, SearchableEnum] protected EModificationSource _modificationSource;
		[SerializeField] private CUiComponentImage _icon;

		private IUiValuableComponent[] _components;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;

		protected CValuableRegionModifier ValuableRegionModifier;
		protected IValuable Valuable;
		protected CUser User;
		
		public EValuable Id => Valuable?.Id ?? EValuable.None;

		[Inject]
		private void Inject(
			CValuableRegionModifier valuableRegionModifier, 
			CResourceConfigs resourceConfigs, 
			IBundleManager bundleManager,
			CUser user
			)
		{
			ValuableRegionModifier = valuableRegionModifier;
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			User = user;
		}

		public void Construct()
		{
			_components = GetComponentsInChildren<IUiValuableComponent>(true);
		}

		public void SetValue(IValuable valuable)
		{
			Valuable = PreProcessInputValuable(valuable);
			SetIcon(Valuable);
			SendToComponents(Valuable);
		}
		
		protected virtual IValuable PreProcessInputValuable(IValuable valuable)
		{
			return ValuableRegionModifier.ModifyValuable(valuable, User.Progress.Region, _modificationSource);
		}

		private void SetIcon(IValuable valuable)
		{
			switch (valuable)
			{
				case CResourceValuable resourceValuable:
					SetResource(resourceValuable);
					return;
				case CEventPointValuable eventPointValuable:
					SetEventPoint(eventPointValuable);
					return;
				case CEventCoinValuable eventCoinValuable:
					SetEventCoin(eventCoinValuable);
					return;
				case CFrameValuable frameValuable:
					SetFrame(frameValuable);
					break;
				default:
					SetValuable(valuable);
					break;
			}
		}

		private void SetFrame(CFrameValuable frameValuable)
		{
			CProfileFrameConfig config = _resourceConfigs.ProfileFrames.GetConfig(frameValuable.Frame);
			if (config == null)
			{
				Debug.LogError("Profile frame config not found for frame: " + frameValuable.Frame);
				_icon.SetAlpha(0);
				return;
			}
			
			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.Sprite, EBundleCacheType.Persistent);
			_icon.SetSprite(sprite);
			_icon.SetAlpha(1);
		}

		private void SetEventPoint(CEventPointValuable eventPointValuable)
		{
			CLiveEventResourceConfig config = _resourceConfigs.LiveEvents.GetConfig(eventPointValuable.LiveEvent);
			if (config == null)
			{
				Debug.LogError("Live event config not found for event: " + eventPointValuable.LiveEvent);
				_icon.SetAlpha(0);
				return;
			}
			
			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.EventPointsIconSprite, EBundleCacheType.Persistent);
			_icon.SetSprite(sprite);
			_icon.SetAlpha(1);
		}
		
		private void SetEventCoin(CEventCoinValuable eventCoinValuable)
		{
			CLiveEventResourceConfig config = _resourceConfigs.LiveEvents.GetConfig(eventCoinValuable.LiveEvent);
			if (config == null)
			{
				Debug.LogError("Live event config not found for event: " + eventCoinValuable.LiveEvent);
				_icon.SetAlpha(0);
				return;
			}
			
			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.EventCoinsIconSprite, EBundleCacheType.Persistent);
			_icon.SetSprite(sprite);
			_icon.SetAlpha(1);
		}

		private void SetResource(CResourceValuable resourceValuable)
		{
			CResourceResourceConfig resourceConfig = _resourceConfigs.Resources.GetConfig(resourceValuable.Resource.Id);
			if (resourceConfig == null)
			{
				Debug.LogError("Resource config not found for resource: " + resourceValuable.Resource.Id);
				_icon.SetAlpha(0);
				return;
			}
			
			Sprite resourceSprite = _bundleManager.LoadItem<Sprite>(resourceConfig.Sprite, EBundleCacheType.Persistent);
			_icon.SetSprite(resourceSprite);
			_icon.SetAlpha(1);
		}
		
		private void SetValuable(IValuable valuable)
		{
			CValuableResourceConfig config = _resourceConfigs.Valuables.GetConfig(valuable?.Id ?? EValuable.None);
			if (config == null || valuable is CVehicleValuable)
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