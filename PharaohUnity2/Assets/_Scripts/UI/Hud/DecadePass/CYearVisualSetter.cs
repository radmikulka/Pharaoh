// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Configs;
using TycoonBuilder.MenuTriggers;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CYearVisualSetter : MonoBehaviour, IAldaFrameworkComponent
	{
		private const float LockedTextAlpha = 0.7f;
		
		[SerializeField] private CUiComponentImage _iconImage;
		[SerializeField] private CUiComponentText _yearText;
		[SerializeField] private bool _curved = true;

		private CResourceConfigs _resourceConfigs;
		private IDecadeProvider _decadeProvider;
		private IBundleManager _bundleManager;
		private IYearProvider _yearProvider;
		private CUiGlobalsConfig _uiGlobals;
		private CUser _user;

		[Inject]
		private void Inject(
			CResourceConfigs resourceConfigs,
			IDecadeProvider decadeProvider,
			IBundleManager bundleManager,
			IYearProvider yearProvider,
			CUiGlobalsConfig uiGlobals,
			CUser user)
		{
			_resourceConfigs = resourceConfigs;
			_decadeProvider = decadeProvider;
			_bundleManager = bundleManager;
			_yearProvider = yearProvider;
			_uiGlobals = uiGlobals;
			_user = user;
		}

		public void SetYear(EYearMilestone yearMilestone)
		{
			SetVisual(yearMilestone, false, false);
		}

		private void SetVisual(EYearMilestone yearMilestone, bool locked, bool isRequirement)
		{
			_yearText.SetAlpha(locked ? LockedTextAlpha : 1f);
			EDecadeMilestone decadeMilestone = _decadeProvider.GetDecade(yearMilestone);
			EDecadeMilestone milestone = locked ? EDecadeMilestone.Locked : decadeMilestone;
			CDecadeMilestoneResourceConfig config = _resourceConfigs.DecadeMilestoneConfigs.GetConfig(milestone);
			IBundleLink bundleLink = _curved ? config.CurvedSprite : config.StraightSprite;
			Sprite sprite = _bundleManager.LoadItem<Sprite>(bundleLink, EBundleCacheType.Persistent);
			int year = _yearProvider.GetYear(yearMilestone);

			_iconImage.SetSprite(sprite);
			_yearText.SetValue(year);
			SetTextColor(locked, isRequirement);
		}

		private void SetTextColor(bool locked, bool isRequirement)
		{
			if (isRequirement && locked)
			{
				_yearText.SetColor(_uiGlobals.NotEnoughCurrencyColor, true);
				return;
			}
			
			_yearText.SetColor(_uiGlobals.EnoughCurrencyColor, true);
		}

		public void SetDecade(EDecadeMilestone decadeMilestone, bool locked)
		{
			SetDecadeVisual(decadeMilestone, locked);
		}
		
		public void SetDecade(ERegion region, bool locked)
		{
			EYearMilestone currentDecadeYearMilestone = _decadeProvider.GetCurrentDecadeYearMilestone(region);
			EDecadeMilestone decade = _decadeProvider.GetDecade(currentDecadeYearMilestone);
			SetDecadeVisual(decade, locked);
		}

		private void SetDecadeVisual(EDecadeMilestone decadeMilestone, bool locked)
		{
			_yearText.SetAlpha(locked ? LockedTextAlpha : 1f);
			EDecadeMilestone milestone = locked ? EDecadeMilestone.Locked : decadeMilestone;
			CDecadeMilestoneResourceConfig config = _resourceConfigs.DecadeMilestoneConfigs.GetConfig(milestone);
			IBundleLink bundleLink = _curved ? config.CurvedSprite : config.StraightSprite;
			Sprite sprite = _bundleManager.LoadItem<Sprite>(bundleLink, EBundleCacheType.Persistent);
			int year = (int) decadeMilestone;

			_iconImage.SetSprite(sprite);
			_yearText.SetValue(year);
		}

		public void SetLockedYear(EYearMilestone yearMilestone)
		{
			CDecadeMilestoneResourceConfig config = _resourceConfigs.DecadeMilestoneConfigs.GetConfig(EDecadeMilestone.Locked);
			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.CurvedSprite, EBundleCacheType.Persistent);
			int year = _yearProvider.GetYear(yearMilestone);

			_iconImage.SetSprite(sprite);
			_yearText.SetValue(year);
		}

		public void SetRequirementYear(EYearMilestone yearMilestone, bool isUiRequirement)
		{
			bool isLocked = _user.Progress.SeenYear < yearMilestone;
			SetVisual(yearMilestone, isLocked, isUiRequirement);
		}

		public Sprite GetYearSprite(EYearMilestone yearMilestone)
		{
			EDecadeMilestone decadeMilestone = _decadeProvider.GetDecade(yearMilestone);
			CDecadeMilestoneResourceConfig config = _resourceConfigs.DecadeMilestoneConfigs.GetConfig(decadeMilestone);
			IBundleLink bundleLink = _curved ? config.CurvedSprite : config.StraightSprite;
			Sprite sprite = _bundleManager.LoadItem<Sprite>(bundleLink, EBundleCacheType.Persistent);
			return sprite;
		}
	}
}