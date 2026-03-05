// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CActivateFreezeRendererTask
	{
		public readonly CLockObject LockObject;

		public CActivateFreezeRendererTask(CLockObject lockObject)
		{
			LockObject = lockObject;
		}
	}
}