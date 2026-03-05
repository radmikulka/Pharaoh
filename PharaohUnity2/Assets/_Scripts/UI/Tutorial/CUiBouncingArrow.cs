// =========================================
// AUTHOR: Juraj Joscak
// DATE:   15.4.2024
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiBouncingArrow : MonoBehaviour, IConstructable
	{
		private const float BlendTime = 0.2f;
		
		[SerializeField] private RectTransform _arrowParent;
		[SerializeField] private Animation _arrowAnim;
		
		private CanvasGroup _canvasGroup;
		private ITutorialGraphicsTarget _target;
		private Vector2 _anchoredOffset;

		public bool IsActive => enabled;
		
		public void Construct()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
			SetActive(false);
		}
		
		private void Update()
		{
			if (_target == null)
				return;
			
			RecalculatePos();
		}

		protected void HideInstant()
		{
			_canvasGroup.DOKill();
			SetActive(false);
		}

		protected void Hide()
		{
			_canvasGroup.DOKill();
			_canvasGroup.DOFade(0f, BlendTime).OnComplete(() =>
			{
				SetActive(false);
			}).OnKill(() =>
			{
				SetActive(false);
			});
		}

		protected async UniTask ShowAt(
			ITutorialGraphicsTarget target, 
			float clockwiseRotation, 
			Vector2 anchoredOffset, 
			CancellationToken ct
		)
		{
			_canvasGroup.DOKill();

			if (!enabled)
			{
				SetValues();
			}

			SetActive(true);
			await _canvasGroup.DOFade(0f, BlendTime * _canvasGroup.alpha).WithCancellation(ct);
			SetValues();
			await _canvasGroup.DOFade(1f, BlendTime).WithCancellation(ct);
			return;

			void SetValues()
			{
				_anchoredOffset = anchoredOffset;
				_target = target;
				_arrowParent.rotation = Quaternion.Euler(0f, 0f, -clockwiseRotation);
				RecalculatePos();
				enabled = true;
			}
		}
		
		private void SetActive(bool state)
		{
			enabled = state;
			_arrowAnim.enabled = state;

			if (state)
			{
				_arrowAnim[_arrowAnim.clip.name].normalizedTime = 0f;
				_arrowAnim.Play();
			}
			else
			{
				_canvasGroup.alpha = 0f;
			}
		}
		
		private void RecalculatePos()
		{
			if (_target == null)
				return;

			Vector2 targetCenterPos = _target.GetScreenPosition();

			_arrowParent.position = targetCenterPos;
			_arrowParent.anchoredPosition += _anchoredOffset;
		}
	}
}