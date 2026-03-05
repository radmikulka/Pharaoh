// =========================================
// AUTHOR: Radek Mikulka
// DATE:   31.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.UnityObjectPool;
using NaughtyAttributes;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Vehicle")]
	public class CVehicleResourceConfig : ScriptableObject, IResourceConfigBase<EVehicle>, IIHaveBundleLinks
	{
		[SerializeField] private EVehicle _id;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _sprite;
		[SerializeField, BundleLink(false, typeof(GameObject))] private CBundleLink _tractorPrefab;
		[SerializeField, BundleLink(false, typeof(GameObject))] private CBundleLink[] _trailerPrefabs;
		[SerializeField, BundleLink(false, typeof(GameObject))] private CBundleLink _menuPrefab;
		[SerializeField, BundleLink(false, typeof(GameObject))] private EVehicleShowcaseAnimation[] _showcaseAnimations =
		{
			EVehicleShowcaseAnimation.Generic_ZoomIn,
			EVehicleShowcaseAnimation.Generic_LeftPanorama,
			EVehicleShowcaseAnimation.Generic_BackLow,
			EVehicleShowcaseAnimation.Generic_ZoomOut,
		};

		[Header("Fullscreen Vehicle Detail")] 
		[SerializeField] private float _fullscreenDetailMinCameraDistance = 5f;
		[SerializeField] private float _fullscreenDetailMaxCameraDistance = 15f;
		[SerializeField] private float _fullscreenDetailDefaultCameraDistance = 10f;
		[SerializeField] private float _menuModelLenght = 10f;
		[SerializeField] private float _fullscreenDetailCameraYOffset = 2f;

		[Header("Vehicle Detail")] 
		[SerializeField] private Vector3 _detailCameraLocalPosition = new(0f, 2f, -30f);
		[SerializeField] private Vector3 _detailSceneModelLocalPosition;
		[SerializeField] private CVehicleDetailRecords _detailRecord;

		[Header("Shop Detail")] [SerializeField]
		private Vector3 _shopCameraLocalPosition;

		public EVehicle Id => _id;
		public CBundleLink Sprite => _sprite;
		public CBundleLink[] TrailerPrefabs => _trailerPrefabs;
		public CBundleLink TractorPrefab => _tractorPrefab;
		public CBundleLink MenuPrefab => _menuPrefab;
		public IReadOnlyList<EVehicleShowcaseAnimation> ShowcaseAnimations => _showcaseAnimations;
		public CVehicleDetailRecords DetailRecord => _detailRecord;
		public Vector3 DetailCameraLocalPosition => _detailCameraLocalPosition;
		public Vector3 DetailSceneModelLocalPosition => _detailSceneModelLocalPosition;
		public Vector3 ShopCameraLocalPosition => _shopCameraLocalPosition;
		public float FullscreenDetailMinCameraDistance => _fullscreenDetailMinCameraDistance;
		public float FullscreenDetailMaxCameraDistance => _fullscreenDetailMaxCameraDistance;
		public float FullscreenDetailDefaultCameraDistance => _fullscreenDetailDefaultCameraDistance;
		public float FullscreenDetailCameraYOffset => _fullscreenDetailCameraYOffset;
		public float MenuModelLenght => _menuModelLenght;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			AssignBundles();
			
			yield return _sprite;
			yield return _tractorPrefab;

			foreach (CBundleLink trailerPrefab in _trailerPrefabs)
			{
				yield return trailerPrefab;
			}
			
			if (_menuPrefab.IsObjectAssigned)
			{
				yield return _menuPrefab;
			}
		}

		[Button]
		private void AssignBundles()
		{
			#if UNITY_EDITOR
			int bundleId = GetBundle();

			int spriteBundle = bundleId;
			CVehicleConfig vehicleConfig = CStaticConfigs.Vehicle.GetConfig(_id);
			if (vehicleConfig.LiveEvent != ELiveEvent.None)
			{
				CLiveEventBundle eventConfig = CStaticConfigs.LiveEventBundles.GetConfig(vehicleConfig.LiveEvent);
				spriteBundle = (int)eventConfig.UiBundle;
			}
			_sprite.SetBundleId(spriteBundle);
			
			_tractorPrefab.SetBundleId(bundleId);
			
			_menuPrefab.SetBundleId(bundleId);
			#endif
		}

		#if UNITY_EDITOR
		private int GetBundle()
		{
			CVehicleConfig vehicleConfig = CStaticConfigs.Vehicle.GetConfig(_id);
			if (vehicleConfig.OverrideBundleId != EBundleId.None)
				return (int)vehicleConfig.OverrideBundleId;

			if (vehicleConfig.LiveEvent != ELiveEvent.None)
			{
				EBundleId eventBundle = CStaticConfigs.LiveEventBundles.GetConfig(vehicleConfig.LiveEvent).ContentBundle;
				return (int)eventBundle;
			}
			
			CRegionConfig regionConfig = CStaticConfigs.Regions.GetRegionConfig(vehicleConfig.Region);
			return (int)regionConfig.ContentBundleId;
		}
		#endif
	}
}