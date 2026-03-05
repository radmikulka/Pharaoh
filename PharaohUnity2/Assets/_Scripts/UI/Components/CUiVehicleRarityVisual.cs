// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using ServerData.Design;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiVehicleRarityVisual : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private GameObject _visual;
		[SerializeField] private CUiComponentImage _icon;
		[SerializeField] private Sprite _proSprite;
		[SerializeField] private Sprite _eliteSprite;
		
		private CDesignVehicleConfigs _vehicleConfigs;
		
		[Inject]
		private void Inject(CDesignVehicleConfigs vehicleConfigs)
		{
			_vehicleConfigs = vehicleConfigs;
		}
		
		public void SetVisual(IValuable reward)
		{
			if (reward is CVehicleValuable vehicleValuable)
			{
				SetVehicle(vehicleValuable.Vehicle);
			}
			else
			{
				SetVehicle(EVehicle.None);
			}
		}
		
		public void SetVehicle(EVehicle vehicleId)
		{
			if (vehicleId == EVehicle.None)
			{
				Hide();
				return;
			}
			
			CVehicleConfig config = _vehicleConfigs.GetConfig(vehicleId);
			SetVisual(config);
		}

		private void SetVisual(CVehicleConfig config)
		{
			switch (config.Rarity)
			{
				case EVehicleRarity.Standard:
					Hide();
					break;
				case EVehicleRarity.Pro:
					Show();
					_icon.SetSprite(_proSprite);
					break;
				case EVehicleRarity.Elite:
					Show();
					_icon.SetSprite(_eliteSprite);
					break;
			}
		}

		private void Hide()
		{
			_visual.SetActive(false);
		}
		
		private void Show()
		{
			_visual.SetActive(true);
		}
	}
}