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

        SavePresetRequest = 126,
        SavePresetResponse = 127,
        
        // game

        #if !UNITY_CLIENT

		// data server
		InitialUserRequest = 10_004,
		InitialUserResponse = 10_005,

		DailyUserRefreshRequest = 10_006,
		DailyUserRefreshResponse = 10_007,

        #endif
	}
}