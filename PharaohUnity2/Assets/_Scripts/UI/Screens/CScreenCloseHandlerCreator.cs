// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using Zenject;

namespace Pharaoh.Ui
{
	public class CScreenCloseHandlerCreator : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CPharaohScreen _screen;
		
		private IEventBus _eventBus;
		
		private CScreenCloseHandler _currentCloseHandler;
		private TextMeshProUGUI _textComponent;
		private bool _isActive = true;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CScreenOpenEndSignal>(OnScreenOpenEnd);
			_eventBus.Subscribe<CScreenCloseStartSignal>(OnScreenCloseStart);
		}
		
		public void Toggle(bool isActive)
		{
			_isActive = isActive;
		}

		private void OnScreenOpenEnd(CScreenOpenEndSignal signal)
		{
			if (!_isActive)
				return;
			
			if (signal.MenuId != _screen.Id)
				return;
			
			TryCreateMenuCloseHandler();
			StretchToFullScreen();
			
			_currentCloseHandler.SetText(_textComponent);
			_currentCloseHandler.Toggle(true);
		}

		private void OnScreenCloseStart(CScreenCloseStartSignal signal)
		{
			if (!_isActive)
				return;
			
			if (signal.MenuId != _screen.Id)
				return;
			
			_currentCloseHandler.Toggle(false);
		}

		private void TryCreateMenuCloseHandler()
		{
			if (_currentCloseHandler)
				return;
			
			GameObject closeHandlerGameObject = new ()
			{
				layer = CObjectLayer.Ui
			};
			_textComponent = closeHandlerGameObject.AddComponent<TextMeshProUGUI>();
			_currentCloseHandler = closeHandlerGameObject.AddComponent<CScreenCloseHandler>();
			_currentCloseHandler.transform.SetParent(transform);
			_currentCloseHandler.transform.SetSiblingIndex(0);
			_currentCloseHandler.SetEventBus(_eventBus);
			closeHandlerGameObject.name = "MenuCloseHandler";
		}

		private void StretchToFullScreen()
		{
			RectTransform rectTransform = _currentCloseHandler.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.offsetMin = new Vector2(-450, -100);
			rectTransform.offsetMax = new Vector2(450, 100);
			rectTransform.localScale = Vector3.one;
		}
	}
}