// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CProfilePictureSetter : MonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentImage _frameImage;
		[SerializeField] private CUiComponentImage _avatarImage;
		
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		
		[Inject]
		private void Inject(CResourceConfigs resourceConfigs, IBundleManager bundleManager)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
		}
		
		public void SetFrame(EProfileFrame frameId)
		{
			CBundleLink bundleLink = _resourceConfigs.ProfileFrames.GetConfig(frameId).Sprite;
			Sprite sprite = _bundleManager.LoadItem<Sprite>(bundleLink, EBundleCacheType.Persistent);
			
			_frameImage.SetSprite(sprite);
		}
		
		public void SetAvatar(EProfileAvatar avatarId)
		{
			CBundleLink bundleLink = _resourceConfigs.ProfileAvatarConfigs.GetConfig(avatarId).Sprite;
			Sprite sprite = _bundleManager.LoadItem<Sprite>(bundleLink, EBundleCacheType.Persistent);
			
			_avatarImage.SetSprite(sprite);
		}
	}
}