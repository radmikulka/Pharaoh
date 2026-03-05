// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.11.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public interface ISmartArrowLocker
	{
		bool IsLocked { get; }
		void AddLock(CLockObject lockObject);
		void RemoveLock(CLockObject lockObject);
	}
}