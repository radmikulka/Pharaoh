// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.12.2023
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using UnityEngine;
using UnityEngine.Assertions;

namespace TycoonBuilder
{
	public class CInputLock : IInputLock
	{
		private readonly EInputLockLayer _layer;
		public string Name { get; }
		public int Layer => (int)_layer;
		public bool BlockBackButton { get; }
		public IReadOnlyList<Transform> AllowedObjects { get; }

		public CInputLock(string name, EInputLockLayer layer, IReadOnlyList<Transform> allowedObjects, bool blockBackButton = true)
		{
			_layer = layer;
			Name = name;
			AllowedObjects = allowedObjects;
			BlockBackButton = blockBackButton;
		}
		
		public CInputLock(string name, EInputLockLayer layer, RectTransform allowedObject, bool blockBackButton = true)
		{
			_layer = layer;
			Name = name;
			AllowedObjects = new List<Transform> { allowedObject };
			BlockBackButton = blockBackButton;
		}

		
		public CInputLock(string name, EInputLockLayer layer, GameObject allowedObject, bool blockBackButton = true)
		{
			_layer = layer;
			Name = name;
			RectTransform objectRect = allowedObject.GetComponent<RectTransform>();
			Assert.IsNotNull(objectRect);
			AllowedObjects = new List<Transform> { objectRect };
			BlockBackButton = blockBackButton;
		}

		public CInputLock(string name, EInputLockLayer layer, bool blockBackButton = true)
		{
			_layer = layer;
			Name = name;
			BlockBackButton = blockBackButton;
			AllowedObjects = ArraySegment<Transform>.Empty;
		}
	}
}