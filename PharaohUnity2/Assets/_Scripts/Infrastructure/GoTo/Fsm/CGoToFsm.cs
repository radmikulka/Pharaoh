
// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Threading;
using Zenject;

namespace TycoonBuilder
{
	public class CGoToFsm
	{
		private readonly CGoToFsmStack _fsmStack = new();
		private readonly CGoToContext _context = new();
		private readonly CancellationTokenSource _cts;
		private CGoToFsmState _current;
		private int _currentFsmIndex;
		public bool BlockInput { get; private set; }

		public virtual bool IsCompleted => _current == null;

		public CGoToFsm(CancellationToken ct)
		{
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		}
		
		public CGoToFsm SetBlockInput(bool blockInput)
		{
			BlockInput = blockInput;
			return this;
		}

		public CGoToFsm SetContextEntry<T>(EGoToContextKey key, T value)
		{
			_context.SetEntry(key, value);
			return this;
		}
		
		public CGoToFsm AddState(CGoToFsmState state)
		{
			_fsmStack.AddState(state);
			return this;
		}

		public CGoToFsm InsertState(int index, CGoToFsmState state)
		{
			_fsmStack.InsertState(index, state);
			return this;
		}

		public void Start()
		{
			CGoToFsmState nextState = _fsmStack.GetState(0);
			StartState(nextState);
		}

		public void Tick()
		{
			TryMoveNext();
			_current?.Tick();
		}

		public void Cancel()
		{
			_cts.Cancel();
		}

		private void TryMoveNext()
		{
			if (!_current.IsCompleted) 
				return;

			++_currentFsmIndex;

			if (_currentFsmIndex >= _fsmStack.Count)
			{
				_current.End();
				_current = null;
				return;
			}
            
			CGoToFsmState nextState = _fsmStack.GetState(_currentFsmIndex);
			StartState(nextState);
		}
        
		private void StartState(CGoToFsmState state)
		{
			_current?.End();
			_current = state;
			
			state.Init(_context, _cts.Token);
			_current.Start();
		}
	}
}