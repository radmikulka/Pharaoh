// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTutorialHighlight : MonoBehaviour, IConstructable, IInitializable
	{
		private static readonly int CenterHash = Shader.PropertyToID("_Center");
		private static readonly int IsRectHash = Shader.PropertyToID("_IsRect");
		private static readonly int SizeHash = Shader.PropertyToID("_Size");

		private const float BlendTime = 0.2f;

		[SerializeField] private RectTransform _highlightRect;
		[SerializeField] private Image _highlightImage;

		private ITutorialHighlightPoint _targetRect;
		private Material _materialInstance;
		private CanvasGroup _canvasGroup;
		private Vector2 _anchoredOffset;
		private IEventBus _eventBus;
		private Canvas _canvas;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Construct()
		{
			_canvas = GetComponent<Canvas>();
			SetActive(false);
			
			_canvasGroup = GetComponent<CanvasGroup>();
			_canvasGroup.alpha = 0f;

			_materialInstance = Instantiate(_highlightImage.material);
			_highlightImage.material = _materialInstance;
			
			enabled = false;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CShowTutorialHighlightTask>(ShowTutorialHighlight);
			_eventBus.AddTaskHandler<CHideTutorialHighlightTask>(HideTutorialHighlight);
		}

		private void Update()
		{
			if (_targetRect == null)
				return;
			
			RecalculatePos();
		}

		private void HideTutorialHighlight(CHideTutorialHighlightTask task)
		{
			_canvasGroup.DOKill();
			_canvasGroup.DOFade(0f, BlendTime).OnComplete(() =>
			{
				SetActive(false);
				enabled = false;
			}).OnKill(() =>
			{
				SetActive(false);
				enabled = false;
			});
		}

		private void ShowTutorialHighlight(CShowTutorialHighlightTask task)
		{
			ShowAt(task.Target, task.SizeOffset, task.AnchoredOffset, task.Type);
		}

		private void ShowAt(ITutorialHighlightPoint target, Vector2 sizeOffset, Vector2 anchoredOffset, ETutorialHighlightRectType type)
		{
			_canvasGroup.DOKill();

			_materialInstance.SetFloat(IsRectHash, type == ETutorialHighlightRectType.Rectangle ? 1f : 0f);

			if (!_canvas.enabled)
			{
				SetValues();
			}
			
			SetActive(true);
			_canvasGroup.DOFade(0f, BlendTime * _canvasGroup.alpha).OnComplete(() =>
			{
				SetValues();
				_canvasGroup.DOFade(1f, BlendTime);
			});
			return;

			void SetValues()
			{
				_anchoredOffset = anchoredOffset;
				_targetRect = target;
				RecalculateSize(target, sizeOffset);
				RecalculatePos();
				enabled = true;
			}
		}

		private void RecalculatePos()
		{
			if (_targetRect == null)
				return;

			Vector2 targetCenterPos = _targetRect.Point;
			Vector2 offset = _anchoredOffset * _canvas.transform.lossyScale;
			_materialInstance.SetVector(CenterHash, targetCenterPos + offset);
		}

		private void SetActive(bool state)
		{
			_canvas.enabled = state;
			enabled = state;
		}

		private void RecalculateSize(ITutorialHighlightPoint target, Vector2 offset)
		{
			Vector2 targetSize = target.Size + offset;
			_materialInstance.SetVector(SizeHash, targetSize * _canvas.transform.lossyScale);
		}
	}
}