// =========================================
// AUTHOR: Juraj Joscak
// DATE:   12.09.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public interface IUiSimpleRowItem
	{
		public Transform Transform { get; }
		public bool IsActive { get; }
		public void Set(object data);
		public void SetActiveObject(bool active);
	}
	
	public class CUiSimpleRow : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private GameObject _itemPrefab;
		[SerializeField] private bool _edgeToEdge;
		
		[Tooltip("Will not spawn any more instances, than already present")] [SerializeField]
		private bool _static;

		private CAldaInstantiator _aldaInstantiator;
		private DiContainer _diContainer;
		
		private readonly List<IUiSimpleRowItem> _items = new();
		private RectTransform _rectTransform;
		
		public IReadOnlyList<IUiSimpleRowItem> Items => _items;
		
		[Inject]
		private void Inject(CAldaInstantiator aldaInstantiator, DiContainer diContainer)
		{
			_aldaInstantiator = aldaInstantiator;
			_diContainer = diContainer;
		}
		
		public void Construct()
		{
			_rectTransform = (RectTransform)transform;

			if (_static)
			{
				_items.AddRange(GetComponentsInChildren<IUiSimpleRowItem>());
			}
		}
		
		public void Initialize()
		{
			if(_static)
				return;
			
			_itemPrefab.gameObject.SetActiveObject(false);
			if(!_itemPrefab.TryGetComponent(out IUiSimpleRowItem _))
				throw new Exception("Item prefab must implement IUiSimpleRowItem interface");
		}
		
		public void Set(IReadOnlyList<object> data)
		{
			for (int i = 0; i < data.Count; i++)
			{
				if(_items.Count <= i)
				{
					if(_static)
						throw new Exception("Not enough items in static CUiSimpleRow to set the provided data.");
					
					SpawnNewItem();
				}
				
				_items[i].SetActiveObject(true);
				_items[i].Set(data[i]);
			}
			
			for(int i = data.Count; i < _items.Count; i++)
			{
				_items[i].SetActiveObject(false);
			}

			if (_edgeToEdge)
			{
				SetPositionsEdgeToEdge(data.Count);
			}
			else
			{
				SetPositions(data.Count);
			}
		}
		
		private void SetPositions(int count)
		{
			float width = _rectTransform.rect.width;
			float segment = width / count;
			
			for (int i = 0; i < count; i++)
			{
				RectTransform rewardTransform = (RectTransform)_items[i].Transform;
				rewardTransform.localPosition = new Vector3(segment / 2 + segment * i, rewardTransform.localPosition.y, 0);
			}
		}
		
		private void SetPositionsEdgeToEdge(int count)
		{
			float itemWidth = ((RectTransform)_items[0].Transform).rect.width;
			float width = _rectTransform.rect.width - itemWidth;
			float segment = width / (count - 1);
			
			for (int i = 0; i < count; i++)
			{
				RectTransform rewardTransform = (RectTransform)_items[i].Transform;
				rewardTransform.localPosition = new Vector3((itemWidth/2) + segment * i, rewardTransform.localPosition.y, 0);
			}
		}
		
		private void SpawnNewItem()
		{
			IUiSimpleRowItem newItem = _aldaInstantiator.Instantiate(_itemPrefab, _itemPrefab.transform.parent, _diContainer).GetComponent<IUiSimpleRowItem>();
			_items.Add(newItem);
		}
	}
}