// =========================================
// AUTHOR: Marek Karaba
// DATE:   28.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CScreenInfoOpenButton : ValidatedMonoBehaviour, IInitializable, IScreenOpenStart, IScreenCloseEnd
	{
		[SerializeField, SearchableEnum] private EScreenInfoId _screenInfoId;
		[SerializeField, Self] private CUiButton _button;
		[SerializeField, Self] private CTweener _tweener;
		[SerializeField] private GameObject _visual;
		
		private CGlobalVariablesInfoScreenHandler _globalVariablesHandler;
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(CGlobalVariablesInfoScreenHandler globalVariablesHandler, IEventBus eventBus)
		{
			_globalVariablesHandler = globalVariablesHandler;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);
			
			_eventBus.Subscribe<CScreenInfoSeenSignal>(OnScreenInfoSeen);
		}

		public void SetInfoScreen(EScreenInfoId infoScreenId)
		{
			_screenInfoId = infoScreenId;
			UpdateTweenerState();
		}

		public void HideInfoScreen()
		{
			DisableTweener();
			_visual.SetActive(false);
		}

		private void OnScreenInfoSeen(CScreenInfoSeenSignal signal)
		{
			if (signal.InfoScreen != _screenInfoId)
				return;

			DisableTweener();
		}

		private void OnClick()
		{
			_eventBus.ProcessTask(new CShowInfoScreenTask(_screenInfoId));
		}

		public void OnScreenOpenStart()
		{
			UpdateTweenerState();
		}

		private void UpdateTweenerState()
		{
			_visual.SetActive(true);
			bool seen = _globalVariablesHandler.GetGlobalVariable(_screenInfoId);
			if (seen)
			{
				DisableTweener();
				return;
			}
			
			_tweener.Enable();
		}

		public void OnScreenCloseEnd()
		{
			DisableTweener();
		}

		private void DisableTweener()
		{
			_tweener.Disable();
			transform.localScale = Vector3.one;
		}
		
		
	}
}