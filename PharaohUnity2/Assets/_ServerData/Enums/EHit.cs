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
        
        CancelPurchaseRequest = 110,
        CancelPurchaseResponse = 111,
        
        InitiatePurchaseRequest = 112,
        InitiatePurchaseResponse = 113,

        AcceptPrivacyStatusRequest = 120,
        AcceptPrivacyStatusResponse = 121,

        SavePresetRequest = 126,
        SavePresetResponse = 127,
        
        // game

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
		
        #endif
	}
}