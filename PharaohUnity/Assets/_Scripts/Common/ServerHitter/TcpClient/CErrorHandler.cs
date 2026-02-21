// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.11.2023
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Pharaoh.Signal;
using ServerData;
using ServerData.Hits;
using ServiceEngine.Purchasing;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pharaoh
{
	public class CErrorHandler : IInitializable
	{
		private readonly IRestartGameHandler _restartGameHandler;
		private readonly IInAppUpdate _inAppUpdate;
		private readonly ICtsProvider _ctsProvider;
		private readonly IEventBus _eventBus;

		public CErrorHandler(
			IRestartGameHandler restartGameHandler,
			IInAppUpdate inAppUpdate,
			ICtsProvider ctsProvider, 
			IEventBus eventBus
			)
		{
			_restartGameHandler = restartGameHandler;
			_inAppUpdate = inAppUpdate;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CPurchaseFailedSignal>(OnPurchaseFailed);
		}

		private void OnPurchaseFailed(CPurchaseFailedSignal signal)
		{
			if(signal.FailureReason != EPurchaseFailureReason.Error)
				return;
			ShowUnknownErrorDialog("PurchaseFailed");
		}

		public void HandleErrorResponse(CResponseHit responseHit)
		{
			switch (responseHit)
			{
				case CBadGameVersionResponse badGameVersionResponse:
					float timePassedSinceRelease = (badGameVersionResponse.ServerStartTime - badGameVersionResponse.CurrentServerTime) / (float)CTimeConst.Second.InMilliseconds;
					float timeToPublishedState = CMath.Max(0f, CTimeConst.Minute.InSeconds * 5 - timePassedSinceRelease);
					HandleBadGameVersionAsync(timeToPublishedState).Forget();
					return;
				case CInvalidAppVersionResponse:
					ShowInvalidAppVersionDialog();
					return;
			}
			
			ShowServerErrorDialog((int)EErrorCode.Internal);
		}

		private async UniTaskVoid HandleBadGameVersionAsync(float timeToPublishedState)
		{
			bool inAppUpdateSucceeded = await _inAppUpdate.TryImmediateUpdate();

			if (!inAppUpdateSucceeded)
			{
				ShowBadGameVersionDialog(timeToPublishedState);
			}
		}

		public void HandleInternalError(EErrorCode errorCode)
		{
			ShowServerErrorDialog(errorCode);
		}

		public void ShowInvalidAppVersionDialog()
		{
			/*CShowDialogTask task = new CShowDialogTask()
					.SetHeaderLocalized("Dialog.InvalidAppVersion.Title")
					.SetContentLocalized("Dialog.InvalidAppVersion.Content")
					.SetOverlay()
					.SetCanBeClosed(false)
					.SetAnalyticsId("InvalidAppVersionDialog")
					.SetOneButton(new CDialogButtonData("Dialog.InvalidAppVersion.Button", RestartGame, EDialogButtonColor.Green, true))
					;
			_eventBus.ProcessTask(task);*/
		}

		private void ShowBadGameVersionDialog(float timeToPublishedState)
		{
			COpenBadGameVersionScreenTask taskOpenScreen = new(timeToPublishedState);
			_eventBus.ProcessTask(taskOpenScreen);
		}

		private void ShowUnknownErrorDialog(string error)
		{
			/*CShowDialogTask task = new CShowDialogTask()
					.SetHeaderLocalized("Server.Dialog.UnknownError.Title")
					.SetContentLocalized("Server.Dialog.UnknownError.Content", error)
					.SetOverlay()
					.SetCanBeClosed(false)
					.SetAnalyticsId("UnknownErrorDialog")
					.SetOneButton(new CDialogButtonData("Generic.Restart", RestartGame, EDialogButtonColor.Blue, true))
					;
			_eventBus.ProcessTask(task);*/
		}
		
		private void ShowServerErrorDialog(EErrorCode errorCode)
		{
			/*switch (errorCode)
			{
				case EErrorCode.Internal:
				{
					CShowDialogTask task = new CShowDialogTask()
						.SetHeaderLocalized("Server.Dialog.InternalError.Title")
						.SetContentLocalized("Server.Dialog.InternalError.Content")
						.SetOverlay()
						.SetCanBeClosed(false)
						.SetAnalyticsId("InternalServerErrorDialog")
						.SetOneButton(new CDialogButtonData("Generic.Restart", RestartGame, EDialogButtonColor.Blue, true))
						;
					_eventBus.ProcessTask(task);
					return;
				}
				default:
				{
					CShowDialogTask task = new CShowDialogTask()
						.SetHeaderLocalized("Server.Dialog.UnknownServerError.Title")
						.SetContentLocalized("Server.Dialog.UnknownServerError.Content", errorCode)
						.SetOverlay()
						.SetCanBeClosed(false)
						.SetAnalyticsId("UnknownServerErrorDialog")
						.SetOneButton(new CDialogButtonData("Generic.Restart", RestartGame, EDialogButtonColor.Blue, true))
						;
					_eventBus.ProcessTask(task);
					return;
				}
			}*/
		}

		private void RestartGame()
		{
			_restartGameHandler.RestartGame(null);
		}
	}
}