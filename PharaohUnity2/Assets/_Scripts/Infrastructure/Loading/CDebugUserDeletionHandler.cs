// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.1.2024
// =========================================

using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;

namespace Pharaoh
{
	public class CDebugUserDeletionHandler
	{
		private static bool _userDeleted;

		private readonly ICtsProvider _ctsProvider;
		private readonly CRequestSender _hitBuilder;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void OnBeforeSceneLoadRuntimeMethod()
		{
			_userDeleted = false;
			TryDeleteCache();
		}
		
		public static bool WillDeleteUserInThisSession()
		{
			return CServerConfig.Instance.DeleteUser && !_userDeleted;
		}

		public CDebugUserDeletionHandler(CRequestSender hitBuilder, ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
			_hitBuilder = hitBuilder;
		}

		private static void TryDeleteCache()
		{
			bool willDelete = WillDeleteUserInThisSession();
			if (!willDelete)
				return;
			
			CPlayerPrefs.DeleteAll();
			Caching.ClearCache();
		}
		
		public async UniTask TryDeleteUserAsync(CAuthDataRequestDto auth)
		{
			bool willDelete = WillDeleteUserInThisSession();
			if (!willDelete)
				return;

			_userDeleted = true;
			
			bool? connectionFailed = null;
			
			CDeleteUserRequest deleteUserRequest = new(auth);
			
			CRequestBuilder hitRecordBuilder = _hitBuilder.GetBuilder(deleteUserRequest)
				.SetOnSuccess<CDeleteUserResponse>(_ => connectionFailed = false)
				.SetOnFail(_ => connectionFailed = true)
				.SetSuppressAutomaticErrorHandling()
				.SetSendAsSingleHit()
				.SetExecuteImmediately();

			_hitBuilder.BuildAndSend(hitRecordBuilder);
			
			await UniTask.WaitUntil(() => connectionFailed.HasValue, cancellationToken: _ctsProvider.Token);
		}
	}
}