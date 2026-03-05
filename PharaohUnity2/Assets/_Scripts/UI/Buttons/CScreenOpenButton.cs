// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CScreenOpenButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, SearchableEnum] private EScreenId _screenId;
		[SerializeField, Self] private CUiButton _button;
		
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);
		}

		private void OnClick()
		{
			_eventBus.ProcessTask(new CShowScreenTask(_screenId));
		}
	}
}