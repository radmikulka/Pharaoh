// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CLoadActiveRegionButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CUiButton _button;
		
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		private CUser _user;
		
		[Inject]
		private void Inject(IEventBus eventBus, ICtsProvider ctsProvider, CUser user)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_button.AddClickListener(OnButtonClicked);
		}

		private void OnButtonClicked()
		{
			CCoreGameGameModeData modeData = new(_user.Progress.Region);
			_eventBus.ProcessTaskAsync(new CLoadGameModeTask(modeData), _ctsProvider.Token).Forget();
		}
	}
}