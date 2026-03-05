// =========================================
// AUTHOR: Juraj Joscak
// DATE:   01.07.2025
// =========================================

using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using ServerData.Hits;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Testing
{
	public class CJurajTestButton : ValidatedMonoBehaviour, IInitializable
	{
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
			_eventBus.ProcessTask(new CShowScreenTask(EScreenId.CompanyGrowth));
		}
	}
}