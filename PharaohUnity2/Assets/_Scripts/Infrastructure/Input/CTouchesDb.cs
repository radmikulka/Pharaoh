// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.12.2023
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pharaoh
{
	public class CTouchesDb
	{
		private readonly List<PointerEventData> _pointerEvents = new();

		public int Count => _pointerEvents.Count;
		
		public readonly CEvent OnPointersCountChanged = new("TouchesDb");

		public void Destroy()
		{
			OnPointersCountChanged.Clear();
		}
		
		public void Add(PointerEventData eventData)
		{
			_pointerEvents.Add(eventData);
			OnPointersCountChanged.Invoke();
		}
		
		public void Remove(PointerEventData eventData)
		{
			_pointerEvents.Remove(eventData);
			OnPointersCountChanged.Invoke();
		}
		
		public PointerEventData GetLastPointer()
		{
			List<PointerEventData> pointerEvents = new List<PointerEventData>(_pointerEvents);
			pointerEvents.Sort((a, b) => b.clickTime.CompareTo(a.clickTime));
			return pointerEvents.FirstOrDefault();
		}

		public (PointerEventData _newTouchA, PointerEventData _newTouchB) GetFirstTwoPointers()
		{
			List<PointerEventData> pointerEvents = new List<PointerEventData>(_pointerEvents);
			pointerEvents.Sort((a, b) => a.clickTime.CompareTo(b.clickTime));
			return (pointerEvents[0], pointerEvents[1]);
		}
	}
}