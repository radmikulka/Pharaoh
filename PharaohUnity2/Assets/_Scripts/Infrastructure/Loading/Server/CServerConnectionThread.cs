// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.Tcp;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using TycoonBuilder;
using UnityEngine;

namespace TycoonBuilder
{
	public class CServerConnectionThread
	{
		private readonly CDebugUserDeletionHandler _debugUserDeletionHandler;
		private readonly CServerEndpointProvider _serverEndpointProvider;
		private readonly CHitsDispatcher _hitsDispatcher;
		private readonly ITranslation _translation;
		private readonly IActiveAuth _activeAuth;
		private readonly CHitBuilder _hitBuilder;

		public CServerConnectionThread(
			CDebugUserDeletionHandler debugUserDeletionHandler, 
			CServerEndpointProvider serverEndpointProvider,
			CHitsDispatcher hitsDispatcher, 
			ITranslation translation, 
			IActiveAuth activeAuth,  
			CHitBuilder hitBuilder
			)
		{
			_debugUserDeletionHandler = debugUserDeletionHandler;
			_serverEndpointProvider = serverEndpointProvider;
			_hitsDispatcher = hitsDispatcher;
			_translation = translation;
			_activeAuth = activeAuth;
			_hitBuilder = hitBuilder;
		}

		public async UniTask<CResponseHit> ConnectAsync(CancellationToken ct)
		{
			await _serverEndpointProvider.Reload(ct);
			_hitsDispatcher.CreateNewTcpClient(_serverEndpointProvider.ActiveEndPoint);
			
			await TryConfigureServerAsync(ct);

			CAuthDataRequestDto auth = GetAuthData();
			await _debugUserDeletionHandler.TryDeleteUserAsync(auth);
			
			CResponseHit response = await SendConnectHitAsync(ct);
			return response;
		}
		
		private async UniTask TryConfigureServerAsync(CancellationToken ct)
		{
			bool shouldSendConfigureServerHit = ShouldSendConfigureServerHit();
			if (!shouldSendConfigureServerHit)
				return;
			
			bool? connectionFailed = null;
			
			CConfigureServerRequest configureServerRequest = new(CServerConfig.Instance.FakeDelayInSecs, CServerConfig.Instance.OverrideEvent);
			
			CHitRecordBuilder hitRecordBuilder = _hitBuilder.GetBuilder(configureServerRequest)
				.SetOnSuccess<CConfigureServerResponse>(_ => connectionFailed = false)
				.SetOnFail(_ => connectionFailed = true)
				.SetSuppressAutomaticErrorHandling()
				.SetSendAsSingleHit()
				.SetExecuteImmediately();

			_hitBuilder.BuildAndSend(hitRecordBuilder);
			
			await UniTask.WaitUntil(() => connectionFailed.HasValue, cancellationToken: ct);
		}
		
		private bool ShouldSendConfigureServerHit()
		{
			if (!CPlatform.IsDebug)
				return false;
			
			return CServerConfig.Instance.FakeDelayInSecs > 0 || CServerConfig.Instance.OverrideEvent != ELiveEvent.None;
		}

		private CDebugAuthDataDto GetDebugAuthDataOrDefault()
		{
			if (!CPlatform.IsDebug)
				return null;
			
			CServerConfig debug = CServerConfig.Instance;
			
			if (debug.PresetId == EUserPresetId.None 
			    && debug.ManualPresetId.IsNullOrEmpty()
			    && debug.OverrideUid.IsNullOrEmpty()
			    && debug.LikeABoss == false)
			{
				return null;
			}
			
			string presetName = debug.PresetId == EUserPresetId.None 
				? debug.ManualPresetId 
				: debug.PresetId.ToString();
			
			return new CDebugAuthDataDto(
				debug.OverrideUid, 
				presetName, 
				debug.LikeABoss, 
				debug.OverrideYear,
				CDebugConfig.Instance.TutorialSkip,
				debug.OverrideContract,
				debug.LikeABossVehicles
				);
		}
		
		private async UniTask<CResponseHit> SendConnectHitAsync(CancellationToken ct)
		{
			CAuthDataRequestDto auth = GetAuthData();
			CDebugAuthDataDto debugAuthData = GetDebugAuthDataOrDefault();
			CDeviceDataDto deviceData = GetDeviceData();
			
			CConnectRequest connectRequest = new(
				auth, 
				debugAuthData,
				deviceData
				);
			
			CAsyncHitResponse<CResponseHit> response = await _hitBuilder.GetBuilder(connectRequest)
				.SetSuppressAutomaticErrorHandling()
				.SetSendAsSingleHit()
				.BuildAndSendAsync<CResponseHit>(ct);
			
			return response.Response;
		}
		
		private CDeviceDataDto GetDeviceData()
		{
			long timeZoneOffset = GetTimeZoneOffsetInSecs();
			string installerName = GetInstallerName();
			
			return new CDeviceDataDto(
				_translation.CurrentLanguage,
				timeZoneOffset,
				SystemInfo.operatingSystem,
				installerName,
				CPlatform.Platform,
				SystemInfo.systemMemorySize,
				SystemInfo.deviceModel
				);
		}

		private long GetTimeZoneOffsetInSecs()
		{
			long timeZoneOffset = (long) TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
			return timeZoneOffset;
		}

		private string GetInstallerName()
		{
			if(CPlatform.IsEditor)
				return "UnityEditor";
			return Application.installerName;
		}
		
		private CAuthDataRequestDto GetAuthData()
		{
			string authToken = _activeAuth.AuthUid;
			string deviceId = SystemInfo.deviceUniqueIdentifier;
			return new CAuthDataRequestDto(authToken, deviceId);
		}
	}
}