// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CRegionController : MonoBehaviour, IConstructable
	{
		[SerializeField] private CFogPlane _unlockRegionFogPlane;
		[SerializeField] private ERegion _regionId;

		private readonly Plane _cameraPlane = new(CVector3.Up, new Vector3(0f, 27f, 0f));
		private GameObject _sceneRoot;

		public ERegion RegionId => _regionId;
		public bool IsActive { get; private set; }

		public void Construct()
		{
			_sceneRoot = gameObject.scene.GetRootGameObjects()[0];
		}

		public Plane GetCameraPlane()
		{
			return _cameraPlane;
		}

		public void SetActive(bool state)
		{
			IsActive = state;
			_sceneRoot.SetActive(state);
		}
		
		public void ActivateCoveringFog()
		{
			_unlockRegionFogPlane.Show();
		}
		
		public async UniTaskVoid PlayUncoverAnimation(CancellationToken ct)
		{
			await _unlockRegionFogPlane.UncoverFog(ct);
		}
	}
}