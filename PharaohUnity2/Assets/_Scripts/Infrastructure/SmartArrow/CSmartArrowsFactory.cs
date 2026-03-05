// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.10.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CSmartArrowsFactory : MonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private CSmartArrow _template;

		private CAldaInstantiator _aldaInstantiator;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus, CAldaInstantiator aldaInstantiator)
		{
			_aldaInstantiator = aldaInstantiator;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_template.gameObject.SetActive(false);
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CGetSmartArrowRequest, CGetSmartArrowResponse>(HandleGetSmartArrow);
		}

		private CGetSmartArrowResponse HandleGetSmartArrow(CGetSmartArrowRequest task)
		{
			CSmartArrow newInstance = _aldaInstantiator.Instantiate(_template, _template.transform.parent);
			return new CGetSmartArrowResponse(newInstance);
		}
	}
}