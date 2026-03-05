// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.10.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace TycoonBuilder
{
	public class CSmartArrowLocker : ISmartArrowLocker
	{
		private readonly HashSet<CLockObject> _locks = new();
		
		public bool IsLocked => _locks.Count > 0;

		public void AddLock(CLockObject lockObject)
		{
			_locks.Add(lockObject);
		}
		
		public void RemoveLock(CLockObject lockObject)
		{
			_locks.Remove(lockObject);
		}
	}
}