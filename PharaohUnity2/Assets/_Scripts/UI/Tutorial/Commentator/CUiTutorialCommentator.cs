// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTutorialCommentator : ValidatedMonoBehaviour, ITutorialCommentator, IInitializable, IConstructable
	{
		[SerializeField, Child] private CUiTutorialCommentatorSideGraphics[] _sideGraphics;
		[SerializeField] private CUiTutorialCommentatorCharacter _character;
		[SerializeField] private CUiTutorialCommentatorBubble _bubble;
		[SerializeField] private CTweener _tapAnywhereTweener;

		private readonly CInputLock _inputLock = new("TutorialCommentator", EInputLockLayer.TutorialCommentator);
		private CTutorialCommentatorProxy _proxy;
		private GameObject _tapAnywhereObject;
		private CEventSystem _eventSystem;

		public bool IsRunning { get; private set; }

		[Inject]
		private void Inject(CTutorialCommentatorProxy commentator, CEventSystem eventSystem)
		{
			_eventSystem = eventSystem;
			_proxy = commentator;
		}

		public void Construct()
		{
			_tapAnywhereObject = _tapAnywhereTweener.gameObject;
			SetActive(false);
		}

		public void Initialize()
		{
			_proxy.SetInstance(this);
		}

		public async UniTask ShowCommentator(ITutorialCommentator.ESide side, string langKey, bool showTapAnywhere, CancellationToken ct)
		{
			SetActive(true);
			_bubble.ResetState();
			SetSide(side);
			await _character.Show(side, ct);
			await ShowText(langKey, showTapAnywhere, ct);
		}
		
		public async UniTask ShowText(string langKey, bool showTapAnywhere, CancellationToken ct)
		{
			_tapAnywhereObject.SetActiveObject(showTapAnywhere);
			if (showTapAnywhere)
			{
				_tapAnywhereTweener.Enable();
			}
			
			UniTask animateTextTask = _bubble.SetText(langKey, ct);
			await UniTask.WaitUntil(() => !_bubble.IsAnimating, cancellationToken: ct);
			UniTask waitForInputTask = CWaitForSignal.WaitForInputAsync(ct);
			await UniTask.WhenAny(animateTextTask, waitForInputTask);
			_bubble.FinishAnimation();
			
			await CWaitForSignal.WaitForInputAsync(ct);
		}

		public async UniTask Hide(CancellationToken ct)
		{
			UniTask hideCharacter = _character.Hide(ct);
			UniTask hideBubble = _bubble.Hide(ct);
			await UniTask.WhenAll(hideCharacter, hideBubble);
			SetActive(false);
			_tapAnywhereTweener.Disable();
		}
		
		private void SetSide(ITutorialCommentator.ESide side)
		{
			foreach (CUiTutorialCommentatorSideGraphics graphics in _sideGraphics)
			{
				graphics.SetSide(side);
			}
		}

		private void SetActive(bool state)
		{
			IsRunning = state;
			gameObject.SetActive(state);

			if (state)
			{
				_eventSystem.AddInputLocker(_inputLock);
				return;
			}
			_eventSystem.RemoveInputLocker(_inputLock);
		}
	}
}