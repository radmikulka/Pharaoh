// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.10.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using KBCore.Refs;
using NaughtyAttributes;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CTutorialGetMoreButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private CanvasGroup _canvasGroup;
		[SerializeField, Required, Child] private CParticleTargetPulser _particleTargetPulser;
		[SerializeField, Self] private CUiButton _button;
		
		private IEventBus _eventBus;
		private CUser _user;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user)
		{
			_eventBus = eventBus;
			_user = user;
		}
		
		public void Initialize()
		{
			_eventBus.AddTaskHandler<CActivateGetMoreButtonTask>(HandleActivateGetMoreButtonTask);
			HandleInitialState();
		}
		
		private void HandleActivateGetMoreButtonTask(CActivateGetMoreButtonTask task)
		{
			_canvasGroup.gameObject.SetActive(true);
			_canvasGroup.DOFade(1f, 0.2f)
				.From(0f)
				.OnComplete(() =>
			{
				_particleTargetPulser.ParticleStepFinished(CValuableFactory.Null);
				_button.SetInteractable(true);
			});
		}

		private void HandleInitialState()
		{
			bool shouldBeVisible = _user.Tutorials.IsTutorialCompleted(EGetMoreMaterialTutorialStep.Completed) || 
			                       _user.Tutorials.IsTutorialCompleted(EDispatchCenterTutorialStep.Completed);
			
			_canvasGroup.gameObject.SetActive(shouldBeVisible);
			_button.SetInteractable(shouldBeVisible);
		}
	}
}