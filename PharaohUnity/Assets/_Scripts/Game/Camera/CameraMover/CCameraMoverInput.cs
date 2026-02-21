// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pharaoh
{
	public class CCameraMoverInput
	{
		private class CTouches
		{
			private class CTouch
			{
				public readonly float StartTime;
				public Touch Touch;

				public CTouch(Touch touch)
				{
					StartTime = Time.realtimeSinceStartup;
					Touch = touch;
				}
			}
			
			private readonly List<CTouch> _touches = new();
		
			public void Add(Touch touch)
			{
				_touches.Add(new CTouch(touch));
			}
		
			public void Remove(Touch touch)
			{
				for (int i = 0; i < _touches.Count; i++)
				{
					CTouch touchData = _touches[i];
					if (touchData.Touch.fingerId == touch.fingerId)
					{
						_touches.RemoveAt(i);
						return;
					}
				}
			}

			public void Update(Touch touch)
			{
				for (int i = 0; i < _touches.Count; i++)
				{
					CTouch touchData = _touches[i];
					if (touchData.Touch.fingerId == touch.fingerId)
					{
						touchData.Touch = touch;
						return;
					}
				}
			}

			public Touch? GetOldestPointer()
			{
				CTouch candidate = null;
				foreach (CTouch touch in _touches)
				{
					if (candidate == null || touch.StartTime < candidate.StartTime)
					{
						candidate = touch;
					}
				}
				return candidate?.Touch;
			}

			public bool TryGetFinger(int fingerId, out Touch activeTouch)
			{
				foreach (CTouch touch in _touches)
				{
					if (touch.Touch.fingerId == fingerId)
					{
						activeTouch = touch.Touch;
						return true;
					}
				}
				activeTouch = default;
				return false;
			}
		}
		
		private readonly PointerEventData _tempPointer = new(EventSystem.current);
		private readonly List<RaycastResult> _raycastResults = new();
		private readonly CTouches _touches = new();

		private bool _mousePressedOverUi;
		private int? _prevFingerId;

		public event Action<Vector2> DragStarted;
		public event Action<Vector2> DragUpdated;
		public event Action DragEnded;
        
		public void Update()
		{
			if (CPlatform.IsEditor && !CPlatform.IsUnityRemoteConnected)
			{
				UpdateMouse();
				return;
			}
			
			UpdateTouches();
		}

		private void UpdateMouse()
		{
			bool mouseDown = Input.GetMouseButtonDown(0);
			bool mouseUp = Input.GetMouseButtonUp(0);
			bool isMousePressed = Input.GetMouseButton(0);

			Vector3 mousePosition = Input.mousePosition;
			if (mouseDown)
			{
				_mousePressedOverUi = IsPointerOverUi(mousePosition);
				if(_mousePressedOverUi)
					return;
				
				DragStarted?.Invoke(mousePosition);
			}
			
			if(_mousePressedOverUi)
				return;
			
			if (isMousePressed)
			{
				DragUpdated?.Invoke(mousePosition);
			}
			
			if (mouseUp)
			{
				DragEnded?.Invoke();
			}
		}

		private void UpdateTouches()
		{
			DetectTouches();

			if (_prevFingerId.HasValue)
			{
				if (_touches.TryGetFinger(_prevFingerId.Value, out Touch touch))
				{
					DragUpdated?.Invoke(touch.position);
					return;
				}
				
				DragEnded?.Invoke();
				_prevFingerId = null;

				Touch? oldTouch = _touches.GetOldestPointer();
				if (!oldTouch.HasValue) 
					return;
				
				_prevFingerId = oldTouch.Value.fingerId;
				DragStarted?.Invoke(oldTouch.Value.position);
				return;
			}
			
			Touch? newTouch = _touches.GetOldestPointer();
			if (!newTouch.HasValue) 
				return;
			_prevFingerId = newTouch.Value.fingerId;
			DragStarted?.Invoke(newTouch.Value.position);
		}

		private void DetectTouches()
		{
			if(Input.touchCount == 0)
				return;
			
			foreach (Touch touch in Input.touches)
			{
				switch (touch.phase)
				{
					case TouchPhase.Began:
						bool isOverUi = IsPointerOverUi(touch.position);
						if(isOverUi)
							return;
						_touches.Add(touch);
						break;
					case TouchPhase.Ended:
					case TouchPhase.Canceled:
						_touches.Remove(touch);
						break;
					case TouchPhase.Moved:
						_touches.Update(touch);
						break;
				}
			}
		}

		private bool IsPointerOverUi(Vector2 screenCoord)
		{
			_tempPointer.position = screenCoord;
			EventSystem.current.RaycastAll(_tempPointer, _raycastResults);
            
			for (int i = 0; i < _raycastResults.Count; ++i)
			{
				int layer = 1 << _raycastResults[i].gameObject.layer;

				if ((layer & CObjectLayerMask.Ui) != 0)
					return true;
			}

			return false;
		}
	}
}