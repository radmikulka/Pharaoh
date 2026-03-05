// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CStonesOnRoadAnimation : MonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private CinemachineCamera _cinemachineCamera;
		[SerializeField] private GameObject[] _objectsToActivateOnStart;
		[SerializeField] private GameObject _allGraphicsParent;
		[SerializeField] private CAnimation _animation;
		
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Construct()
		{
			SetActiveCamera(false);
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CContractActivatedSignal>(OnContractActivated);
		}

		private void OnContractActivated(CContractActivatedSignal signal)
		{
			if(signal.StoryContract.StaticData.ContractId != EStaticContractId._1930_StonesOnTheRoad)
				return;
			_allGraphicsParent.SetActive(false);
		}

		public void PrepareStonesOnRoadAnimation()
		{
			SetActiveBaseObjects(true);
		}

		public void ReleaseFirstTaskTutorialCamera()
		{
			SetActiveCamera(false);
		}

		private void SetActiveBaseObjects(bool state)
		{
			foreach (GameObject o in _objectsToActivateOnStart)
			{
				o.SetActive(state);
			}
		}

		public async Task PlayStonesOnRoadAnimation(CancellationToken ct)
		{
			await PlayAnim(ct);
		}

		private async UniTask PlayAnim(CancellationToken ct)
		{
			SetActiveBaseObjects(false);
			SetActiveCamera(true);
			float totalDuration = _animation.CalculateTotalDuration();
			_animation.Play(ct).Forget();
			await UniTask.WaitForSeconds(totalDuration - 1.5f, cancellationToken: ct);
		}
		
		private void SetActiveCamera(bool active)
		{
			_cinemachineCamera.enabled = active;
		}
	}
}