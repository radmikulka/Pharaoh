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

namespace Pharaoh
{
	public class CMission : MonoBehaviour, IConstructable
	{
		[SerializeField] private EMissionId _missionId;

		private readonly Plane _cameraPlane = new(CVector3.Up, new Vector3(0f, 27f, 0f));
		private GameObject _sceneRoot;

		public EMissionId MissionId => _missionId;
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
	}
}