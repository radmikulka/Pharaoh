// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using System;

namespace ServerData
{
	public enum EHit
	{
		// internal
        None = 0,
        BadGameVersionResponse = 2,
        InternalError = 3,
        InvalidAuth = 4,
        InvalidPurchase = 5,
        SessionExpired = 6,
        InvalidAppVersion = 7,
        DataServerUnreachable = 8,
        TooManyRequests = 9,
        InternalErrorRequest = 10,
        
        // client
        ConnectRequest = 100,
        ConnectResponse = 101,
        
        ConfigureServerRequest = 102,
        ConfigureServerResponse = 103,
        
        DeleteUserRequest = 104,
        DeleteUserResponse = 105,
        
        RefreshTokenRequest = 106,
        RefreshTokenResponse = 107,

        ValidatePurchaseRequest = 108,
        ValidatePurchaseResponse = 109,

        AdSeenRequest = 110,
        AdSeenResponse = 111,

        AuthInfoRequest = 112,
        AuthInfoResponse = 113,

        LinkAccountRequest = 114,
        LinkAccountResponse = 115,

        CancelAccountDeletionOfServiceAuthRequest = 116,
        CancelAccountDeletionOfServiceAuthResponse = 117,

        CancelAccountDeletionRequest = 118,
        CancelAccountDeletionResponse = 119,

        AcceptPrivacyStatusRequest = 120,
        AcceptPrivacyStatusResponse = 121,
        
        DeleteAccountRequest = 122,
        DeleteAccountResponse = 123,

        SetGlobalVariableRequest = 124,
        SetGlobalVariableResponse = 125,

        SavePresetRequest = 126,
        SavePresetResponse = 127,
        
        // game
        DispatchForResourceRequest = 1_000,
        DispatchForResourceResponse = 1_001,
        
        DispatchToStoryContractRequest = 1_002,
        DispatchToStoryContractResponse = 1_003,
        
        CollectDispatchedVehicleRequest = 1_004,
        CollectDispatchedVehicleResponse = 1_005,
        
        ClaimDecadePassTierRequest = 1_006,
        ClaimDecadePassTierResponse = 1_007,
        
        ClaimYearRequest = 1_008,
        ClaimYearResponse = 1_009,
        
        AddXpRequest = 1_010,
        AddXpResponse = 1_011,

        SendRateUsFeedbackRequest = 1_012,
        SendRateUsFeedbackResponse = 1_013,
        
        EditUserSocialRequest = 1_014,
        EditUserSocialResponse = 1_015,

        RemoveResourceRequest = 1_016,
        RemoveResourceResponse = 1_017,
        
        ClaimContractRequest = 1_018,
        ClaimContractResponse = 1_019,
        
        ActivateContractRequest = 1_020,
        ActivateContractResponse = 1_021,

        UpgradeVehicleRequest = 1_022,
        UpgradeVehicleResponse = 1_023,

        BuyVehicleRequest = 1_024,
        BuyVehicleResponse = 1_025,

        SpeedupDispatchRequest = 1_026,
        SpeedupDispatchResponse = 1_027,

        UpgradeWarehouseRequest = 1_028,
        UpgradeWarehouseResponse = 1_029,

        SpeedupWarehouseUpgradeRequest = 1_030,
        SpeedupWarehouseUpgradeResponse = 1_031,

        ClaimWarehouseUpgradeRequest = 1_032,
        ClaimWarehouseUpgradeResponse = 1_033,

        UpgradeCityRequest = 1_034,
        UpgradeCityResponse = 1_035,

        SpeedupCityUpgradeRequest = 1_036,
        SpeedupCityUpgradeResponse = 1_037,

        ClaimCityUpgradeRequest = 1_038,
        ClaimCityUpgradeResponse = 1_039,
        
        UpgradeFactoryRequest = 1_040,
        UpgradeFactoryResponse = 1_041,
        
        CraftInFactoryRequest = 1_042,
        CraftInFactoryResponse = 1_043,
        
        CollectCraftedFactoryProductRequest = 1_044,
        CollectCraftedFactoryProductResponse = 1_045,
        
        BuyFactorySlotRequest = 1_046,
        BuyFactorySlotResponse = 1_047,
        
        RepairFactoryRequest = 1_048,
        RepairFactoryResponse = 1_049,

        ClaimFactoryUpgradeRequest = 1_050,
        ClaimFactoryUpgradeResponse = 1_051,

        BuyBuildingPlotRequest = 1_052,
        BuyBuildingPlotResponse = 1_053,
        
        PlaceSpecialBuildingRequest = 1_054,
        PlaceSpecialBuildingResponse = 1_055,

        BuySpecialBuildingRequest = 1_056,
        BuySpecialBuildingResponse = 1_057,

        InitiatePurchaseRequest = 1_058,
        InitiatePurchaseResponse = 1_059,

        StartFuelStationUpgradeRequest = 1_060,
        StartFuelStationUpgradeResponse = 1_061,

        ClaimFuelStationUpgradeRequest = 1_062,
        ClaimFuelStationUpgradeResponse = 1_063,

        UseConsumableRequest = 1_064,
        UseConsumableResponse = 1_065,

        CancelPurchaseRequest = 1_066,
        CancelPurchaseResponse = 1_067,

        DispatchToPassengerContractRequest = 1_068,
        DispatchToPassengerContractResponse = 1_069,

        GetMoreMaterialRequest = 1_070,
        GetMoreMaterialResponse = 1_071,

        RepairVehicleRequest = 1_072,
        RepairVehicleResponse = 1_073,
        
        StartVehicleDepoUpgradeRequest = 1_074,
        StartVehicleDepoUpgradeResponse = 1_075,
        
        ClaimVehicleDepoUpgradeRequest = 1_076,
        ClaimVehicleDepoUpgradeResponse = 1_077,
        
        SpeedupVehicleDepoUpgradeRequest = 1_078,
        SpeedupVehicleDepoUpgradeResponse = 1_079,
        
        SpeedupFactoryUpgradeRequest = 1_080,
        SpeedupFactoryUpgradeResponse = 1_081,
        
        SpeedupFuelStationUpgradeRequest = 1_082,
        SpeedupFuelStationUpgradeResponse = 1_083,
        
        SetIntroTutorialStepRequest = 1_086,
        SetIntroTutorialStepResponse = 1_087,
        
        SetWarehouseTutorialStepRequest = 1_088,
        SetWarehouseTutorialStepResponse = 1_089,
        
        SetDispatchCenterTutorialStepRequest = 1_090,
        SetDispatchCenterTutorialStepResponse = 1_091,
        
        CheatValuableRequest = 1_092,
        CheatValuableResponse = 1_093,
        
        SetVehicleDepotTutorialStepRequest = 1_094,
        SetVehicleDepotTutorialStepResponse = 1_095,
        
        SetFactoryTutorialStepRequest = 1_098,
        SetFactoryTutorialStepResponse = 1_099,
        
        SetGetMoreMaterialTutorialStepRequest = 1_100,
        SetGetMoreMaterialTutorialStepResponse = 1_101,
        
        SetPlayerCityTutorialStepRequest = 1_102,
        SetPlayerCityTutorialStepResponse = 1_103,
        
        ClaimEventPassTierRequest = 1_104,
        ClaimEventPassTierResponse = 1_105,
        
        SetContractsTutorialStepRequest = 1_106,
        SetContractsTutorialStepResponse = 1_107,
        
        MarkOfferAsSeenRequest = 1_108,
        MarkOfferAsSeenResponse = 1_109,
        
        SetOpenCityPlotTutorialStepRequest = 1_110,
        SetOpenCityPlotTutorialStepResponse = 1_111,
        
        FactorySeenRequest = 1_112,
        FactorySeenResponse = 1_113,
        
        MarkContractAsSeenRequest = 1_114,
        MarkContractAsSeenResponse = 1_115,
        
        MarkVehicleAsSeenRequest = 1_116,
        MarkVehicleAsSeenResponse = 1_117,
        
        CheatVehicleRequest = 1_118,
        CheatVehicleResponse = 1_119,
        
        CreateStoryContractRequest = 1_120,
        CreateStoryContractResponse = 1_121,
        
        GoNextRegionRequest = 1_122,
        GoNextRegionResponse = 1_123,
        
        MarkEventIntroAsSeenRequest = 1_124,
        MarkEventIntroAsSeenResponse = 1_125,

        ClaimEventLeaderboardRequest = 1_126,
        ClaimEventLeaderboardResponse = 1_127,
        
        SetEventPointSeenRequest = 1_128,
        SetEventPointSeenResponse = 1_129,

        SetSeenCompletedPiggyContractsRequest = 1_130,
        SetSeenCompletedPiggyContractsResponse = 1_131,
        
        GetUserDetailRequest = 1_132,
        GetUserDetailResponse = 1_133,
        
        ClaimDailyTaskRequest = 1_134,
        ClaimDailyTaskResponse = 1_135,
        
        ClaimWeeklyTaskRequest = 1_136,
        ClaimWeeklyTaskResponse = 1_137,
        
        ClaimFinalDailyRewardRequest = 1_138,
        ClaimFinalDailyRewardResponse = 1_139,

        MarkEventAsSeenRequest = 1_140,
        MarkEventAsSeenResponse = 1_141,

        ClaimEventPassBonusBankRequest = 1_142,
        ClaimEventPassBonusBankResponse = 1_143,
        
        ClaimProjectRequest = 1_144,
        ClaimProjectResponse = 1_145,

        DispatchTransportFleetRequest = 1_146,
        DispatchTransportFleetResponse = 1_147,

        ClaimPromoCodeRequest = 1_148,
        ClaimPromoCodeResponse = 1_149,

        ClaimMessageRequest = 1_150,
        ClaimMessageResponse = 1_151,

        ClaimCommunityNewsRequest = 1_152,
        ClaimCommunityNewsResponse = 1_153,

        SetEventLastSeenRankRequest = 1_154,
        SetEventLastSeenRankResponse = 1_155,

        ClaimExcavationSiteRequest = 1_156,
        ClaimExcavationSiteResponse = 1_157,
        ClaimSpecialDeliveryRequest = 1_158, // reserved for future
        ClaimSpecialDeliveryResponse = 1_159, // reserved for future

        // successHits
        AccountDeletionPending = 8_000,
        
        #if !UNITY_CLIENT
        
		WatchdogInfoRequest = 9_000,
		WatchdogInfoResponse = 9_001,
		
		// data server
		RefreshPricingTierRequest = 10_000,
		RefreshPricingTierResponse = 10_001,
		
		InitialUserRequest = 10_004,
		InitialUserResponse = 10_005,

		DailyUserRefreshRequest = 10_006,
		DailyUserRefreshResponse = 10_007,

		FetchStaticOffersRequest = 10_008,
		FetchStaticOffersResponse = 10_009,
		
		OnContractCompletedRequest = 10_010,
		OnContractCompletedResponse = 10_011,
		
		GenerateEventOffersRequest = 10_012,
		GenerateEventOffersResponse = 10_013,

		OnYearClaimedRequest = 10_014,
		OnYearClaimedResponse = 10_015,

		// admin
		AdminSearchByUidRequest = 20_000,
		AdminSearchByUidResponse = 20_001,
        
		AdminSearchByNickNameRequest = 20_002,
		AdminSearchByNickNameResponse = 20_003,
        
		AdminSearchByPublicIdShortRequest = 20_004,
		AdminSearchByPublicIdShortResponse = 20_005,
        
		AdminCopyUserToDatabaseRequest = 20_006,
		AdminCopyUserToDatabaseResponse = 20_007,
        
		AdminDeleteUserRequest = 20_008,
		AdminDeleteUserResponse = 20_009,

		AdminGetDetailUserInfoRequest = 20_010,
		AdminGetDetailUserInfoResponse = 20_011,

		AdminEditUserRequest = 20_012,
		AdminEditUserResponse = 20_013,
        
		AdminGetGameConfigRequest = 20_014,
		AdminGetGameConfigResponse = 20_015,
        
		AdminSearchQueryRequest = 20_016,
		AdminSearchQueryResponse = 20_017,
        
		AdminSearchByPublicIdRequest = 20_018,
		AdminSearchByPublicIdResponse = 20_019,
        
		AdminSetDateRequest = 20_020,
		AdminSetDateResponse = 20_021,
        
		AdminAddTimeRequest = 20_022,
		AdminAddTimeResponse = 20_023,

		AdminAddPromoCodeRequest = 20_024,
		AdminAddPromoCodeResponse = 20_025,

		AdminUpdatePromoCodeRequest = 20_026,
		AdminUpdatePromoCodeResponse = 20_027,

		AdminGetAllPromoCodesRequest = 20_028,
		AdminGetAllPromoCodesResponse = 20_029,

		AdminDeletePromoCodeRequest = 20_030,
		AdminDeletePromoCodeResponse = 20_031,

		AdminSendMessageRequest = 20_032,
		AdminSendMessageResponse = 20_033,

		AdminGetAllMessagesRequest = 20_034,
		AdminGetAllMessagesResponse = 20_035,

		AdminEditMessageRequest = 20_036,
		AdminEditMessageResponse = 20_037,

		AdminSendCommunityNewsRequest = 20_038,
		AdminSendCommunityNewsResponse = 20_039,

		AdminGetAllCommunityNewsRequest = 20_040,
		AdminGetAllCommunityNewsResponse = 20_041,

		AdminEditCommunityNewsRequest = 20_042,
		AdminEditCommunityNewsResponse = 20_043

        #endif
	}
}