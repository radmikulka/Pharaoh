// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.10.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CGetMoreButton : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private GameObject _plusButtonVisual;
		[SerializeField] private GameObject _checkMarkVisual;
		[SerializeField, Child] private CTycoonUiButton _button;
		[SerializeField] private RectTransform _buttonVisualRect;
		[SerializeField] private RectTransform _buttonVisualRect2;

		private CAnimationProvider _animationProvider;
		private ICtsProvider _ctsProvider;
		private IGoToHandler _goToHandler;
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private CUser _user;

		private IContract _currentContract;
		private CTycoonBuilderScreen _parentMenu;

		private EResource _currentResource;
		private int _missingAmount;
		private int _ownedAmount;
		private bool _animationPlaying;
		private Tween _punchTween;

		[Inject]
		private void Inject(
			CAnimationProvider animationProvider,
			ICtsProvider ctsProvider,
			IGoToHandler goToHandler,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user)
		{
			_animationProvider = animationProvider;
			_ctsProvider = ctsProvider;
			_goToHandler = goToHandler;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
		}

		public void Construct()
		{
			_button.AddClickListener(OnClick);
			_parentMenu = GetComponentInParent<CTycoonBuilderScreen>(true);
		}

		public void Initialize()
		{
			_eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			_eventBus.Subscribe<CNotEnoughResourcesSignal>(OnNotEnoughResources);
		}

		private void OnNotEnoughResources(CNotEnoughResourcesSignal signal)
		{
			if (_currentResource != signal.ResourceId || _animationPlaying)
				return;

			PlayStretchAnimation(_ctsProvider.Token).Forget();
		}

		private async UniTask PlayStretchAnimation(CancellationToken ct)
		{
			_animationPlaying = true;

			UniTask task1 = _animationProvider.StretchAnimation.Animate(_buttonVisualRect, ct);
			UniTask task2 = _animationProvider.StretchAnimation.Animate(_buttonVisualRect2, ct);
			
			await UniTask.WhenAll(task1, task2);
			_animationPlaying = false;
		}

		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			UpdateState();
		}

		public void SetContract(IContract contract)
		{
			_currentContract = contract;
			UpdateState();
		}

		public void Disable()
		{
			_currentContract = null;
			_currentResource = EResource.None;
		}

		private void UpdateState()
		{
			if (_currentContract == null)
				return;
			
			_currentResource = _currentContract.Requirement.Id;
			CalculateResources();
			bool fulfilled = _missingAmount <= 0;
			bool hasEnough = _ownedAmount >= _missingAmount;
			bool canBeFulFilled = fulfilled || hasEnough;
			_plusButtonVisual.SetActive(!canBeFulFilled);
			_checkMarkVisual.SetActive(canBeFulFilled);
		}

		private void OnClick()
		{
			if (_currentContract == null)
				return;
			
			HandleContract();
		}

		private void HandleContract()
		{
			CalculateResources();
			int diff = _missingAmount - _ownedAmount;

			if (_missingAmount <= 0)
			{
				if(_punchTween != null)
					return;
				
				_punchTween = _checkMarkVisual.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 0, 0)
						.OnComplete(() => _punchTween = null)
						;
				return;
			}
			
			_goToHandler.GetMoreMaterial(new SResource(_currentContract.Requirement.Id, diff <= 0 ? 10 : diff), _parentMenu.GetScreenName());
		}

		private void CalculateResources()
		{
			_missingAmount = _currentContract.Requirement.Amount - _currentContract.DeliveredAmount;

			if (_currentContract is CContract typedContract)
			{
				switch (typedContract.Type)
				{
					case EContractType.Passenger:
						CObservableRecharger recharger = _user.City.GetPassengersGenerator(_serverTime.GetTimestampInMs());
						_ownedAmount = recharger.CurrentAmount;
						break;
					case EContractType.Story:
					case EContractType.Event:
						_ownedAmount = _user.Warehouse.GetResourceAmount(_currentContract.Requirement.Id);
						break;
				}
			}
		}
	}
}