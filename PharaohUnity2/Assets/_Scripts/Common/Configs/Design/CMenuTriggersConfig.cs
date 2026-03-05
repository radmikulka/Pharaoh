// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using TycoonBuilder;

namespace ServerData.Design
{
	public class CMenuTriggersConfig : CBaseMenuTriggersConfig
	{
		private EMenuTrigger _menuTrigger;
		
		public CMenuTriggersConfig()
		{
			AddRateUsTriggers();
			AddNativeRateUsTriggers();
			AddPermitNotificationTriggers();
			AddSaveProgressTriggers();
			AddFacebookLikeTriggers();
		}

		private void AddContractTrigger(EStaticContractId contractId, EMenuTrigger menuTrigger)
		{
			AddTrigger(contractId, menuTrigger);
		}

		private void AddRateUsTriggers()
		{
			_menuTrigger = EMenuTrigger.RateUs;
			AddContractTrigger(EStaticContractId._1931_EastRoute, _menuTrigger);
			AddContractTrigger(EStaticContractId._1931_RailroadTurntable, _menuTrigger);
			AddContractTrigger(EStaticContractId._1932_HooverDam, _menuTrigger);
			AddContractTrigger(EStaticContractId._1933_PublicLibrary, _menuTrigger);
			AddContractTrigger(EStaticContractId._1934_AquaticPark, _menuTrigger);


			//AddLoopTrigger(1932, 1, _menuTrigger);
		}

		private void AddNativeRateUsTriggers()
		{
			_menuTrigger = EMenuTrigger.NativeRateUs;
			AddContractTrigger(EStaticContractId._1930_SmallPowerPlant, _menuTrigger);
			AddContractTrigger(EStaticContractId._1932_CityAirport, _menuTrigger);
		}

		private void AddPermitNotificationTriggers()
		{
			_menuTrigger = EMenuTrigger.PermitNotifications;
			AddContractTrigger(EStaticContractId._1931_WaterPump, _menuTrigger);
			AddContractTrigger(EStaticContractId._1934_CommunityTheater, _menuTrigger);
		}

		private void AddSaveProgressTriggers()
		{
			_menuTrigger = EMenuTrigger.SaveProgress;
			

		}

		private void AddFacebookLikeTriggers()
		{
			_menuTrigger = EMenuTrigger.FacebookLike;
			

		}

        private void AddFacebookConnectTriggers()
        {
            _menuTrigger = EMenuTrigger.FacebookConnect;


        }
    }
}