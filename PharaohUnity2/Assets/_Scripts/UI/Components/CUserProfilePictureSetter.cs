// =========================================
// AUTHOR: Marek Karaba
// DATE:   05.08.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using ServiceEngine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUserProfilePictureSetter : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CUiValuable _frameValuable;
		[SerializeField] private CUiComponentImage _avatarImage;
		
		private CFacebookAvatarGateway _facebookAvatarGateway;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		private CUser _user;

		private CancellationTokenSource _cts;
		
		[Inject]
		private void Inject(
			CFacebookAvatarGateway facebookAvatarGateway,
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			ICtsProvider ctsProvider,
			IEventBus eventBus,
			CUser user)
		{
			_facebookAvatarGateway = facebookAvatarGateway;
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
			_user = user;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CCustomizeMenuFrameSelectedSignal>(OnFrameSelected);
			_eventBus.Subscribe<CCustomizeMenuAvatarSelectedSignal>(OnAvatarSelected);
			_eventBus.Subscribe<CCustomizeMenuClosedSignal>(OnCustomizeMenuCanceled);
			
			SetFrameByUserData();
			SetAvatarByUserData();
		}

		private void SetFrameByUserData()
		{
			SetFrame(_user.Account.Frame);
		}

		private void SetAvatarByUserData()
		{
			SetAvatar(_user.Account.Avatar);
		}

		private void OnAvatarSelected(CCustomizeMenuAvatarSelectedSignal signal)
		{
			SetAvatar(signal.AvatarId);
		}

		private void OnFrameSelected(CCustomizeMenuFrameSelectedSignal signal)
		{
			SetFrame(signal.FrameId);
		}

		private void OnCustomizeMenuCanceled(CCustomizeMenuClosedSignal signal)
		{
			SetFrameByUserData();
			SetAvatarByUserData();
		}

		private void SetAvatar(EProfileAvatar avatarId)
		{
			if (avatarId == EProfileAvatar.Facebook)
			{
				string facebookId = _user.Account.GetFacebookUserId();
				if (!string.IsNullOrEmpty(facebookId))
				{
					_cts?.Cancel();
					_cts = _ctsProvider.GetNewLinkedCts();
					SetFacebookAvatarAsync(facebookId, _cts.Token).Forget();
					return;
				}
			}
			
			CBundleLink bundleLink = _resourceConfigs.ProfileAvatarConfigs.GetConfig(avatarId).Sprite;
			Sprite sprite = _bundleManager.LoadItem<Sprite>(bundleLink, EBundleCacheType.Persistent);
			
			_avatarImage.SetSprite(sprite);
		}
		
		private async UniTask SetFacebookAvatarAsync(string facebookUserId, CancellationToken cancellationToken)
		{
			Sprite fbSprite = await _facebookAvatarGateway.GetSpriteOrDefault(facebookUserId, cancellationToken);
			
			if (cancellationToken.IsCancellationRequested)
				return;

			if (fbSprite)
			{
				_avatarImage.SetSprite(fbSprite);
			}
		}

		private void SetFrame(EProfileFrame frameId)
		{
			_frameValuable.SetValue(CValuableFactory.Frame(frameId));
		}
	}
}