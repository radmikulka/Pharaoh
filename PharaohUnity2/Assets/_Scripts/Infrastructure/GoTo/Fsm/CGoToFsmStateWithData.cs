// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public abstract class CGoToFsmStateWithData<T> : CGoToFsmState
	{
		protected CGoToFsmStateWithData(IEventBus eventBus) : base(eventBus)
		{
		}

		public abstract void SetData(T data);
	}
}