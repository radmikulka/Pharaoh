// =========================================
// AUTHOR: Marek Karaba
// DATE:   29.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Testing
{
	public class CBoulderCityTestButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Child] private CUiButton _button;
		
		private readonly ECity _cityId = ECity.R1_City2;
		
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
			_eventBus.ProcessTask(new COpenCityDispatchMenuTask(_cityId));
		}
	}
}