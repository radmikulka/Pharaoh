// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using Pharaoh.GoToStates;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CFsmStateFactory
	{
		private readonly DiContainer _container;
		
		public CFsmStateFactory(DiContainer container)
		{
			_container = container;
		}

		public CCloseAllMenusState CloseAllMenus => Create<CCloseAllMenusState>();
		public CKillMenuState KillMenuState => Create<CKillMenuState>();

		private T Create<T>() where T : CGoToFsmState
		{
			try
			{
				return _container.Resolve<T>();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				throw;
			}
		}
	}
}