// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine.EventSystems;
using Zenject;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	public class CTouchInput : 
		MonoBehaviour, 
		IConstructable,
		IInitializable,
		IBeginDragHandler, 
		IEndDragHandler, 
		IDragHandler, 
		IPointerDownHandler, 
		IPointerUpHandler,
		IInitializePotentialDragHandler
	{
		private readonly CTouchesDb _touchesDb = new();
		private CPinchHandler _pinchHandler;
		private Vector2 _dragStartPos;
		private Vector2 _lastPointerDownPos;

		private int _appFocusStartFrame;
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Construct()
		{
			_pinchHandler = new CPinchHandler(_touchesDb);
		}

		public void Initialize()
		{
			
		}
		
		private void Update()
		{
			UpdateEditorMouseWheel();
			DetectTaps();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if(!hasFocus)
				return;
			_appFocusStartFrame = Time.frameCount;
		}

		private void OnDestroy()
		{
			_touchesDb.Destroy();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{

		}

		public void OnEndDrag(PointerEventData eventData)
		{

		}

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			eventData.useDragThreshold = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			int activeTouchesCount = _touchesDb.Count;
			if (activeTouchesCount == 1)
			{
				OnDragUpdate(eventData);
				return;
			}

			if (activeTouchesCount > 1)
			{
				OnPinchUpdate();
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_lastPointerDownPos = eventData.position;
			
			int previousTouchesCount = _touchesDb.Count;
			_touchesDb.Add(eventData);
			int actualTouchesCount = _touchesDb.Count;
			
			if (previousTouchesCount == 0 && actualTouchesCount == 1)
			{
				OnDragStart(eventData);
			}

			if (previousTouchesCount == 1 && actualTouchesCount == 2)
			{
				OnPinchStart();
			}

			_eventBus.Send(new CTouchInputStartSignal(eventData.position, eventData.pointerId));
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_eventBus.Send(new CTouchInputEndSignal(
				eventData.position, 
				eventData.pointerId, 
				_lastPointerDownPos,
				eventData.dragging));
			
			int previousTouchesCount = _touchesDb.Count;
			_touchesDb.Remove(eventData);
			int actualTouchesCount = _touchesDb.Count;

			if (previousTouchesCount == 1 && actualTouchesCount == 0)
			{
				OnDragStop(eventData);
				return;
			}

			if (previousTouchesCount == 2 && actualTouchesCount == 1)
			{
				OnPinchStop();
			}
		}

		private void OnPinchStart()
		{
			_eventBus.Send(new CPinchStartSignal());
		}

		private void OnPinchStop()
		{
			PointerEventData remainingPointer = _touchesDb.GetLastPointer();
			_eventBus.Send(new CPinchEndSignal(remainingPointer?.position ?? Vector2.zero));
		}

		private void OnPinchUpdate()
		{
			float pinch = _pinchHandler.RefreshAndGetPinch();
			_eventBus.Send(new CPinchUpdateSignal(pinch));
		}

		private void OnDragStart(PointerEventData eventData)
		{
			_eventBus.Send(new CDragStartSignal(eventData.position, eventData.pointerId));
			_dragStartPos = eventData.position;
		}

		private void OnDragStop(PointerEventData eventData)
		{
			_eventBus.Send(new CDragEndSignal(eventData.position, eventData.delta, eventData.pointerId));
		}

		private void OnDragUpdate(PointerEventData eventData)
		{
			_eventBus.Send(new CDragUpdateSignal(
				eventData.position, 
				eventData.pointerId, 
				eventData.delta,
				_dragStartPos));
		}

		private void UpdateEditorMouseWheel()
		{
			if (!CPlatform.IsEditor)
				return;
			
			bool isFirstFrameWithFocus = Time.frameCount == _appFocusStartFrame + 1;
			if(isFirstFrameWithFocus)
				return;
			
			float pinch = Input.mouseScrollDelta.y;
			if(pinch == 0f)
				return;
			
			_eventBus.Send(new CPinchUpdateSignal(pinch));
		}
		
		private void DetectTaps()
		{
			if (CPlatform.IsEditor)
			{
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					_eventBus.Send(new CTapSignal(Input.mousePosition));
					return;
				}
			}
			
			if (Input.touchCount <= 0)
				return;

			if (Input.GetTouch(0).phase != TouchPhase.Ended)
				return;
				
			_eventBus.Send(new CTapSignal(Input.GetTouch(0).position));
		}
	}
}