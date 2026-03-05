// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2025
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
	public class CLiveEventRegionEntranceAnimation : MonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private GameObject[] _objectsToDeactivate;
		[SerializeField] private CAnimation _anim;

		private CCinematicModeHandler _cinematicModeHandler;
		private CEventRegionController _regionController;
		private IGoToHandler _goToHandler;
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		private CUser _user;

		[Inject]
		private void Inject(
			CCinematicModeHandler cCinematicModeHandler,
			CEventRegionController regionController, 
			IGoToHandler goToHandler, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus,
			CUser user
			)
		{
			_cinematicModeHandler = cCinematicModeHandler;
			_regionController = regionController;
			_ctsProvider = ctsProvider;
			_goToHandler = goToHandler;
			_eventBus = eventBus;
			_user = user;
		}

		public void Construct()
		{
			gameObject.SetActive(false);
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CRegionLiveEventGameModeLoadedSignal>(OnEventLoaded);
		}

		private void OnEventLoaded(CRegionLiveEventGameModeLoadedSignal signal)
		{
			if(signal.EventId != _regionController.LiveEvent)
				return;
			
			TryPlayAnim();
		}

		private void TryPlayAnim()
		{
			ELiveEvent liveEventId = _regionController.LiveEvent;
			bool introSeen = _user.LiveEvents.IsIntroSeen(liveEventId);
			if(introSeen)
				return;

			bool skipIntro = CDebugConfig.Instance.ShouldSkip(EEditorSkips.IntroCutscene);
			if (skipIntro)
				return;

			_goToHandler.TryKillActiveGoTo();
			_user.LiveEvents.MarkLiveEventIntroAsSeen(liveEventId);
			PlayIntroAsync(_ctsProvider.Token).Forget();
		}

		private async UniTask PlayIntroAsync(CancellationToken ct)
		{
			gameObject.SetActive(true);
			_cinematicModeHandler.SetActive(true, false);
			SetActiveObjectsToDeactivate(false);
			await CSkipAbleUniTask.Play(PlaySkipAblePartAsync, _eventBus, ct);
			SetActiveObjectsToDeactivate(true);
			_cinematicModeHandler.SetActive(false, true);
			gameObject.SetActive(false);
			
			await _eventBus.ProcessTaskAsync(new COpenEventOverviewTask(_regionController.LiveEvent), ct);
		}
		
		private async UniTask PlaySkipAblePartAsync(CancellationToken ct)
		{
			await _anim.Play(ct);
		}

		private void SetActiveObjectsToDeactivate(bool state)
		{
			foreach (GameObject o in _objectsToDeactivate)
			{
				o.SetActiveObject(state);
			}
		}
	}
}