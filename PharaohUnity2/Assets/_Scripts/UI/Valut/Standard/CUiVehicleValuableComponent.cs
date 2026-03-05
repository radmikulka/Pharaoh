// =========================================
// AUTHOR: Juraj Joscak
// DATE:   21.07.2025
// =========================================

using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiVehicleValuableComponent : CUiValuableComponent<CVehicleValuable>, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentText _text;
		[SerializeField] private CUiComponentImage _icon;
		[SerializeField] private CUiVehicleRarityVisual _rarityVisual;

		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private ITranslation _translation;
		
		[Inject]
		private void Inject(
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			ITranslation translation)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_translation = translation;
		}
		
		protected override void SetValue(CVehicleValuable value)
		{
			_text.SetValue(_translation.GetText($"Vehicle.{(int)value.Vehicle}.Name"));
			SetVehicleIcon(value.Vehicle);
			_rarityVisual?.SetVehicle(value.Vehicle);
		}

		private void SetVehicleIcon(EVehicle vehicle)
		{
			if (_icon == null)
			{
				Debug.LogError("Icon component is not assigned.", gameObject);
				return;
			}
			
			CVehicleResourceConfig[] vehicleResourceConfigs = _resourceConfigs.Vehicles.GetConfigs().ToArray();
			CVehicleResourceConfig vehicleResourceConfig = vehicleResourceConfigs.FirstOrDefault(config => config.Id == vehicle);
			if (!vehicleResourceConfig)
			{
				Debug.LogError($"Vehicle resource config not found for vehicle: {vehicle}");
				return;
			}
			
			Sprite icon = _bundleManager.LoadItem<Sprite>(vehicleResourceConfig.Sprite);
			if (!icon)
			{
				Debug.LogError($"Icon not found for vehicle: {vehicleResourceConfig.Sprite}");
				return;
			}
			
			_icon.SetSprite(icon);
		}

		protected override bool IsValidValuable(IValuable valuable)
		{
			return valuable is CVehicleValuable;
		}
	}
}