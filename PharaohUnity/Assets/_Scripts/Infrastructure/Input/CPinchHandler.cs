// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.12.2023
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pharaoh
{
	public class CPinchHandler
	{
		private readonly CTouchesDb _touchesDb;

		private PointerEventData _previousTouchA;
		private PointerEventData _previousTouchB;
		private Vector2 _previousTouchAPos;
		private Vector2 _previousTouchBPos;

		private PointerEventData _newTouchA;
		private PointerEventData _newTouchB;

		public CPinchHandler(CTouchesDb touchesDb)
		{
			_touchesDb = touchesDb;
			_touchesDb.OnPointersCountChanged.Subscribe(OnPointersCountChanged);
		}

		private void OnPointersCountChanged()
		{
			Reset();
		}

		public float RefreshAndGetPinch()
		{
			(_newTouchA, _newTouchB) = _touchesDb.GetFirstTwoPointers();
			TryResetFingers();

			float prevTouchDistance = (_previousTouchAPos - _previousTouchBPos).magnitude;
			SetNewTouchData();
			float currentTouchDistance = GetTouchDistance();

			float result = currentTouchDistance - prevTouchDistance;
			return result;
		}

		private float GetTouchDistance()
		{
			return (_previousTouchA.position - _previousTouchB.position).magnitude;
		}

		private void TryResetFingers()
		{
			if (_previousTouchA == null || _previousTouchB == null)
			{
				SetNewTouchData();
				return;
			}

			if (_newTouchA.pointerId != _previousTouchA.pointerId)
			{
				SetNewTouchData();
				return;
			}

			if (_newTouchB.pointerId != _previousTouchB.pointerId)
			{
				SetNewTouchData();
			}
		}

		private void SetNewTouchData()
		{
			SetPreviousTouches(_newTouchA, _newTouchB);
		}

		private void SetPreviousTouches(PointerEventData touchA, PointerEventData touchB)
		{
			_previousTouchA = touchA;
			_previousTouchB = touchB;
			_previousTouchAPos = _newTouchA.position;
			_previousTouchBPos = _newTouchB.position;
		}

		private void Reset()
		{
			_previousTouchA = null;
			_previousTouchB = null;
			_newTouchA = null;
			_newTouchB = null;
		}
	}
}