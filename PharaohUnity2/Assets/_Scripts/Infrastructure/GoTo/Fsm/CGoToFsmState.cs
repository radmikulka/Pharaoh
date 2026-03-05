// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Threading;
using AldaEngine;

namespace TycoonBuilder
{
	public abstract class CGoToFsmState
	{
		protected readonly IEventBus EventBus;
		
		protected CancellationToken CancellationToken;
		protected CGoToContext Context;

		public bool IsCompleted { get; protected set; }

		protected CGoToFsmState(IEventBus eventBus)
		{
			EventBus = eventBus;
		}

		public void Init(CGoToContext context, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Context = context;
		}
        
		public virtual void Start()
		{
            
		}

		public virtual void Tick()
		{
            
		}

		public virtual void End()
		{
            
		}
	}
}