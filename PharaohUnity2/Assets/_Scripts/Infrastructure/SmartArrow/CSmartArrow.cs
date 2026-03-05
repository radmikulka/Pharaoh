// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.03.2022
// =========================================

using UnityEngine;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder
{
	public class CSmartArrow : MonoBehaviour, IInitializable, ISmartArrow
	{
		[SerializeField] protected RectTransform[] _blockedRects;
		[SerializeField] protected RectTransform _arrowRect;
		[SerializeField] protected CanvasGroup _canvasGroup;

		private readonly Vector3[] _tempRectCorners = new Vector3[4];
		private readonly List<Vector3> _tempIntersectionPoints = new();
		private IMainCameraProvider _cameraProvider;
		private float? _timeWhenPointAppearInFrustum;
		private Vector2 _screenSpaceOffset;
		private Vector3 _targetWorldPos;
		private bool _isVisible = true;

		[Inject]
		private void Inject(IMainCameraProvider cameraProvider)
		{
			_cameraProvider = cameraProvider;
		}

		public void Initialize()
		{
			SetActive(false);
		}

		private void SetActive(bool state)
		{
			gameObject.SetActiveObject(state);
		}

		public void DestroySelf()
		{
			Destroy(gameObject);
		}

		public void SetVisible(bool state, float blendDuration)
		{
			if(_isVisible == state)
				return;
			_isVisible = state;
			_canvasGroup.DOKill();
			float duration = blendDuration * (state ? 1f - _canvasGroup.alpha : _canvasGroup.alpha);
			_canvasGroup.DOFade(state ? 1f : 0f, duration);
		}

		private void SetVisible(bool state)
		{
			_isVisible = state;
			_canvasGroup.DOKill();
			_canvasGroup.alpha = state ? 1f : 0f;
		}

		public void ShowAt(Vector3 targetWorldPos, Vector2 screenSpaceOffset)
		{
			_screenSpaceOffset = screenSpaceOffset;
			_targetWorldPos = targetWorldPos;
			SetActive(true);
			UpdatePosition(1f);
			SetVisible(false);
		}

		private void LateUpdate()
		{
			UpdatePosition(0f);
		}

		private void UpdatePosition(float deltaT)
		{
			Vector3 screenSize = GetScreenSize();
			Vector3 screenPos = GetTargetScreenPos();

			Vector4 offsetLeftTopRightBot = new(50f, 50f, 50f, 50f);
			bool isPointOnScreen = screenPos.x >= offsetLeftTopRightBot.x 
			                       && screenPos.x <= screenSize.x - offsetLeftTopRightBot.z 
			                       && screenPos.y >= offsetLeftTopRightBot.w 
			                       && screenPos.y <= screenSize.y - offsetLeftTopRightBot.y 
			                       && screenPos.z > 0f;
			
			if (isPointOnScreen && !IsPointInBlockedRect(screenPos))
			{
				_timeWhenPointAppearInFrustum ??= Time.timeSinceLevelLoad;
				
				float positionT = CMath.Clamp((Time.timeSinceLevelLoad - _timeWhenPointAppearInFrustum.Value) / 0.2f, 0f, 1f);
				_arrowRect.position = Vector2.Lerp(_arrowRect.position, screenPos, positionT + deltaT);
				_arrowRect.localRotation = Quaternion.Slerp(_arrowRect.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * 8f + deltaT);
				return;
			}
			
			_timeWhenPointAppearInFrustum = null;
			
			Vector2 pointOnBounds = GetPointOnBounds(screenPos, offsetLeftTopRightBot);
			
			Vector3 screenCenter = GetScreenCenter();
			Vector3 correctedScreenPos = screenPos.z < 0f ? -screenPos : screenPos;
			Vector3 screenPosMinusMid = correctedScreenPos - screenCenter;
			float angleInRad = CMath.Atan2(screenPosMinusMid.y, screenPosMinusMid.x);
			float angle = angleInRad - 90f * CMath.Deg2Rad;
			
			_arrowRect.position = Vector2.Lerp(_arrowRect.position, pointOnBounds, Time.deltaTime * 10f + deltaT);
			_arrowRect.localRotation = Quaternion.Slerp(_arrowRect.localRotation, Quaternion.Euler(0f, 0f, 180f + angle * CMath.Rad2Deg), Time.deltaTime * 5f + deltaT);
		}

		private Vector3 GetTargetScreenPos()
		{
			return _cameraProvider.Camera.WorldToScreenPoint(_targetWorldPos) + new Vector3(_screenSpaceOffset.x, _screenSpaceOffset.y, 0f);
		}

		private Vector2 GetPointOnBounds(Vector3 screenPos, Vector4 offsetLeftTopRightBot)
		{
			Vector2 rectSize = GetScreenSize();
			Vector3 screenCenter = GetScreenCenter();
			
			Vector3 pointOnBounds = GetCornerPoint(screenPos, offsetLeftTopRightBot, screenCenter, rectSize);
			Vector3 clampedPoint = ClampByBlockRects(pointOnBounds, screenCenter);

			return clampedPoint;
		}
		
		private Vector3 GetScreenCenter()
		{
			Vector2 rectSize = GetScreenSize();
			return new Vector3(rectSize.x / 2f, rectSize.y / 2f, 0f);
		}
		
		private Vector2 GetScreenSize()
		{
			return new Vector2(Screen.width, Screen.height);
		}

		private Vector3 GetCornerPoint(Vector3 screenPos, Vector4 offsetLeftTopRightBot, Vector3 screenCenter, Vector2 screenSize)
		{
			screenPos -= screenCenter;
			if (screenPos.z < 0f)
				screenPos *= -1f;

			float angleInRad = CMath.Atan2(screenPos.y, screenPos.x);
			float angle = angleInRad - 90f * Mathf.Deg2Rad;
			float cos = CMath.Cos(angle);
			float m = cos / -CMath.Sin(angle);
			Vector3 screenBounds = screenCenter;

			if (cos > 0)
			{
				// up
				screenPos = new Vector3(screenBounds.y / m, screenBounds.y, 0f);
			}
			else
			{
				// down
				screenPos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0f);
			}

			if (screenPos.x > screenBounds.x)
			{
				// right
				screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0f);
			}
			else if (screenPos.x < -screenBounds.x)
			{
				// left
				screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0f);
			}
			
			screenPos += screenCenter;
			
			screenPos.x = CMath.Clamp(screenPos.x, offsetLeftTopRightBot.x, screenSize.x - offsetLeftTopRightBot.z);
			screenPos.y = CMath.Clamp(screenPos.y, offsetLeftTopRightBot.w, screenSize.y - offsetLeftTopRightBot.y);
			return screenPos;
		}

		private Vector3 ClampByBlockRects(Vector3 pointOnBounds, Vector3 screenCenter)
		{
			_tempIntersectionPoints.Clear();

			for (int i = 0; i < _blockedRects.Length; i++)
			{
				// order is lb, lt, rt, rb
				_blockedRects[i].GetWorldCorners(_tempRectCorners);

				for (int j = 0; j < _tempRectCorners.Length; j++)
				{
					Vector2 pointA = _tempRectCorners[j];
					Vector2 pointB = _tempRectCorners[j == _tempRectCorners.Length - 1 ? 0 : j + 1];
					
					Vector2? intersectionPoint = LineLineIntersection(screenCenter, pointOnBounds, pointA, pointB);

					if (intersectionPoint.HasValue 
					    && Vector2.Dot((intersectionPoint.Value - screenCenter.XY()).normalized, (pointOnBounds - screenCenter).normalized) > 0.95f 
					    && IsInBetween(intersectionPoint.Value.x, pointA.x, pointB.x) 
					    && IsInBetween(intersectionPoint.Value.y, pointA.y, pointB.y))
					{
						_tempIntersectionPoints.Add(intersectionPoint.Value);
					}
					continue;

					bool IsInBetween(float number, float a, float b)
					{
						if (CMath.Approx(a, b))
							return true;
						
						if (a > b)
						{
							(a, b) = (b, a);
						}

						return number >= a && number <= b;
					}
				}
			}
			
			Vector2? closestIntersectionPoint = null;
			float closestPointDistance = float.PositiveInfinity;
			
			for (int j = _tempIntersectionPoints.Count - 1; j >= 0; j--)
			{
				if (!closestIntersectionPoint.HasValue)
				{
					closestIntersectionPoint = _tempIntersectionPoints[j];
					closestPointDistance = Vector2.Distance(screenCenter, closestIntersectionPoint.Value);
					continue;
				}

				float distance = Vector2.Distance(screenCenter, _tempIntersectionPoints[j]);
				if (distance < closestPointDistance)
				{
					closestIntersectionPoint = _tempIntersectionPoints[j];
					closestPointDistance = distance;
				}
			}
			
			if (closestIntersectionPoint.HasValue)
			{
				return closestIntersectionPoint.Value;
			}
			
			return pointOnBounds;
		}

		private Vector2? LineLineIntersection(Vector2 ax, Vector2 ay, Vector2 bx, Vector2 @by)
		{
			// Line AB represented as a1x + b1y = c1 
			float a1 = ay.y - ax.y;
			float b1 = ax.x - ay.x;
			float c1 = a1 * ax.x + b1 * ax.y;

			// Line CD represented as a2x + b2y = c2 
			float a2 = @by.y - bx.y;
			float b2 = bx.x - @by.x;
			float c2 = a2 * bx.x + b2 * bx.y;

			float determinant = a1 * b2 - a2 * b1;

			if (determinant == 0)
			{
				// The lines are parallel. This is simplified 
				// by returning a pair of FLT_MAX 
				return null;
			}

			float x = (b2 * c1 - b1 * c2) / determinant;
			float y = (a1 * c2 - a2 * c1) / determinant;
			return new Vector2(x, y);
		}
		
		private bool IsPointInBlockedRect(Vector2 point)
		{
			for (int i = 0; i < _blockedRects.Length; i++)
			{
				RectTransform rect = _blockedRects[i];
				bool contains = RectTransformUtility.RectangleContainsScreenPoint(rect, point);
				if (contains)
				{
					return true;
				}
			}

			return false;
		}
	}
}