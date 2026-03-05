// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using ServiceEngine;

namespace TycoonBuilder
{
	public class CLoginService
	{
		public struct SProfileData
		{
			public readonly EAuthType AuthType;
			public readonly EYearMilestone YearMilestone;
			public readonly int HardCurrency;
			public readonly int SoftCurrency;

			public SProfileData(EAuthType authType, EYearMilestone yearMilestone, int hardCurrency, int softCurrency)
			{
				YearMilestone = yearMilestone;
				HardCurrency = hardCurrency;
				SoftCurrency = softCurrency;
				AuthType = authType;
			}
		}

		private readonly CApplicationCtsProvider _applicationCtsProvider;
		private readonly CErrorHandler _errorHandler;
		private readonly IRestartGameHandler _restartGameHandler;
		private readonly IFacebookService _facebookService;
		private readonly CAuthUidStorage _authUidStorage;
		private readonly IWaitingScreen _waitingScreen;
		private readonly IGoogleSignIn _googleSignIn;
		private readonly CEventSystem _eventSystem;
		private readonly ICtsProvider _ctsProvider;
		private readonly IAppleSignIn _appleSignIn;
		private readonly IAuthService _authService;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		private readonly CInputLock _lockObject = new("ServiceLock", EInputLockLayer.Default);

		public CLoginService(
			CApplicationCtsProvider applicationCtsProvider, 
			CErrorHandler errorHandler, 
			IRestartGameHandler restartGameHandler, 
			ICtsProvider gameCancellationToken, 
			IFacebookService facebookService, 
			IWaitingScreen waitingScreen, 
			IGoogleSignIn googleSignIn, 
			IActiveAuth authUidStorage, 
			IAuthService authService, 
			CEventSystem eventSystem, 
			IAppleSignIn appleSignIn,
			CHitBuilder hitBuilder,
			IEventBus eventBus, 
			CUser user
			)
		{
			_authUidStorage = (CAuthUidStorage) authUidStorage;
			_applicationCtsProvider = applicationCtsProvider;
			_errorHandler = errorHandler;
			_restartGameHandler = restartGameHandler;
			_ctsProvider = gameCancellationToken;
			_facebookService = facebookService;
			_waitingScreen = waitingScreen;
			_googleSignIn = googleSignIn;
			_eventSystem = eventSystem;
			_appleSignIn = appleSignIn;
			_authService = authService;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_user = user;
		}

		public async UniTask TrySignInAsync(EAuthType authType, CancellationToken ct)
		{
			_waitingScreen.Show();
			_eventSystem.AddInputLocker(_lockObject);
			
			CancellationToken appCancellationToken = _applicationCtsProvider.Token;

			try
			{
				SSignInResult signInResult = await GetServiceProviderToken(authType);
				await HandleLoginWithServer(authType, signInResult, ct);
			}
			finally
			{
				if (!appCancellationToken.IsCancellationRequested)
				{
					_waitingScreen.Hide();
					_eventSystem.RemoveInputLocker(_lockObject);
				}
			}
		}


		private async UniTask<SSignInResult> GetServiceProviderToken(EAuthType authType)
		{
			SSignInResult token;
			switch (authType)
			{
				case EAuthType.Google:
					token = await _googleSignIn.SignInAndGetToken();
					break;
				case EAuthType.Meta:
					token = await _facebookService.TryLogin(_ctsProvider.Token);
					break;
				case EAuthType.Apple:
					token = await _appleSignIn.SignInAndGetToken();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(authType), authType, null);
			}

			if (token.Token.IsNullOrEmpty())
				throw new Exception("Service login failed");
			return token;
		}

		private async UniTask HandleLoginWithServer(EAuthType authType, SSignInResult signInResult, CancellationToken ct)
		{
			string authUid = _authService.GetAuthUidOrDefault();
			CAsyncHitResponse<CResponseHit> infoResponse = await _hitBuilder
				.GetBuilder(new CAuthInfoRequest(authType, signInResult.Token, authUid, signInResult.UserId))
				.SetExecuteImmediately()
				.BuildAndSendAsync<CResponseHit>(ct);

			ct.ThrowIfCancellationRequested();
			
			// is valid to return here - hit is not suppressing error dialogs so error is handled on upper level
			if (infoResponse.ErrorCode.HasValue)
				return;

			if (infoResponse == null)
				throw new Exception($"Failed to get auth info for {authType} with token {signInResult.Token} for user {signInResult.UserId}");

			switch (infoResponse.Response)
			{
				case CAccountDeletionPendingResponse accountDeletionResponse:
					_eventSystem.RemoveInputLocker(_lockObject);
					bool restoreAccount = await _errorHandler.ShowAccountDeletionPending(accountDeletionResponse.TimeToDeleteAccountInMs);
					if (!restoreAccount)
						return;
					
					await _hitBuilder.GetBuilder(new CCancelAccountDeletionOfServiceAuthRequest(authType, signInResult.Token, signInResult.UserId))
						.SetExecuteImmediately()
						.BuildAndSendAsync<CCancelAccountDeletionResponse>(ct);
					TrySignInAsync(authType, ct).Forget();
					return;
				case CAuthInfoResponse authInfoResponse:
					await HandleConflict(authType, signInResult, new CAuthInfoResponse(authInfoResponse.ConflictingUser), ct);
					break;
				case CLinkAccountResponse linkAccountResponse:
					_authService.InitAuth(linkAccountResponse.NewAuth.UserAuthUid, linkAccountResponse.NewAuth.LoggedServices);
					PostprocessUserAuth(authType, signInResult.UserId);
					break;
			}
		}

		private void PostprocessUserAuth(EAuthType type, string authUserId)
		{
			if (type == EAuthType.Meta)
			{
				_user.Account.SetFacebookUserId(authUserId);
			}
		}

		private async UniTask HandleConflict(
			EAuthType authType, 
			SSignInResult signInResult, 
			CAuthInfoResponse authInfoResponse, 
			CancellationToken ct
			)
		{
			_eventSystem.RemoveInputLocker(_lockObject);
			bool useRemoteAccount = await WaitForUserToSelectAccount(authInfoResponse.ConflictingUser, authType, ct);
			CLinkAccountResponse linkResponse = await ConfirmLinkAccount(authType, signInResult, useRemoteAccount, ct);
			
			PostprocessUserAuth(authType, signInResult.UserId);
			
			if (useRemoteAccount)
			{
				_authUidStorage.SetAuthUid(linkResponse.NewAuth.UserAuthUid);
				_restartGameHandler.RestartGame(null);
				return;
			}
			
			_authService.InitAuth(linkResponse.NewAuth.UserAuthUid, linkResponse.NewAuth.LoggedServices);
		}
		
		private async UniTask<bool> WaitForUserToSelectAccount(
			CProfileConflictingUserDto conflictingUser, 
			EAuthType authType,
			CancellationToken ct
			)
		{
			_waitingScreen.Hide();

			bool? useRemoteUser = null;
			bool? cancelled = null;

			SProfileData deviceData = GetDeviceData();
			SProfileData remoteData = GetRemoteData();
			_eventBus.ProcessTask(new COpenProfileConflictTask(
				deviceData, 
				remoteData, () =>
			{
				useRemoteUser = true;
			}, () =>
			{
				useRemoteUser = false;
			}, () =>
			{
				cancelled = true;
			}));

			await UniTask.WaitUntil(() => useRemoteUser.HasValue || cancelled.HasValue, cancellationToken: ct);
			ct.ThrowIfCancellationRequested();

			if (cancelled == true)
				throw new OperationCanceledException();

			_waitingScreen.Show();

			return useRemoteUser.Value;

			SProfileData GetDeviceData()
			{
				CConsumableOwnedValuable hardCurrency = _user.OwnedValuables.GetConsumable(EValuable.HardCurrency);
				CConsumableOwnedValuable softCurrency = _user.OwnedValuables.GetConsumable(EValuable.SoftCurrency);
				return new SProfileData(EAuthType.DeviceId, _user.Progress.Year, hardCurrency.Amount.LocalValue, softCurrency.Amount.LocalValue);
			}

			SProfileData GetRemoteData()
			{
				return new SProfileData(authType, conflictingUser.Year, conflictingUser.HardCurrency, conflictingUser.SoftCurrency);
			}
		}
		
		private async UniTask<CLinkAccountResponse> ConfirmLinkAccount(
			EAuthType authType,
			SSignInResult signInResult, 
			bool useRemoteUser, 
			CancellationToken ct)
		{
			string authUid = _authService.GetAuthUidOrDefault();
			
			CAsyncHitResponse<CLinkAccountResponse> response = await _hitBuilder
				.GetBuilder(new CLinkAccountRequest(authUid, authType, signInResult.Token, signInResult.UserId, useRemoteUser))
				.SetExecuteImmediately()
				.BuildAndSendAsync<CLinkAccountResponse>(ct);
			
			ct.ThrowIfCancellationRequested();
			
			if (response == null)
				throw new Exception($"Failed to link account {authType} with token {signInResult.Token}");

			return response.Response;
		}
	}
}