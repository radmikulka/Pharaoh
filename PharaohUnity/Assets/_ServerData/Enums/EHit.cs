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
        InvalidAppVersion = 6,
        DataServerUnreachable = 7,
        TooManyRequests = 8,
        InternalErrorRequest = 9,
        
        // client
        ConnectRequest = 100,
        ConnectResponse = 101,
        
        ConfigureServerRequest = 102,
        ConfigureServerResponse = 103,
        
        DeleteUserRequest = 104,
        DeleteUserResponse = 105,
        
        ValidatePurchaseRequest = 108,
        ValidatePurchaseResponse = 109,

        AuthInfoRequest = 112,
        AuthInfoResponse = 113,

        LinkAccountRequest = 114,
        LinkAccountResponse = 115,

        SavePresetRequest = 126,
        SavePresetResponse = 127,
        
        // game
        
        #if !UNITY_CLIENT

		// data server

		InitialUserRequest = 10_000,
		InitialUserResponse = 10_001,

		DailyUserRefreshRequest = 10_002,
		DailyUserRefreshResponse = 10_003,

        #endif
	}
}