// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Pharaoh
{
	public class CGoToFsmStack
	{
		private readonly List<CGoToFsmState> _states = new();
        
		public int Count => _states.Count;
        
		public CGoToFsmStack AddState(CGoToFsmState state)
		{
			_states.Add(state);
			return this;
		}
        
		[CanBeNull]
		public CGoToFsmState GetState(int index)
		{
			return _states[index];
		}

		public void InsertState(int index, CGoToFsmState state)
		{
			_states.Insert(index, state);
		}
	}
}