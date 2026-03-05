// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.12.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Testing
{
	public class CTestSendCoalFromRegion2Button : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private GameObject _visual;
		[SerializeField, Child] private CUiButton _button;
		
		private ICtsProvider _ctsProvider;
		private IGoToHandler _goToHandler;
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(
			ICtsProvider ctsProvider,
			IGoToHandler goToHandler,
			IEventBus eventBus)
		{
			_ctsProvider = ctsProvider;
			_goToHandler = goToHandler;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CRegionActivatedSignal>(OnRegionLoaded);
			
			_button.AddClickListener(OnButtonClicked);
		}

		private void OnButtonClicked()
		{
			GoToRegionOffice(_ctsProvider.Token).Forget();
		}

		private async UniTask GoToRegionOffice(CancellationToken ct)
		{
			await _goToHandler.GoToRegionPoint(ERegionPoint.RegionalOffice, ERegion.Region2, ct);
			_eventBus.Send(new COpenDispatchMenuTriggeredSignal());
			_eventBus.ProcessTask(new COpenResourceDispatchMenuTask(EIndustry.CoalMine));
		}

		private void OnRegionLoaded(CRegionActivatedSignal signal)
		{
			bool active = signal.Region == ERegion.Region2;
			_visual.SetActive(active);
		}
	}
}