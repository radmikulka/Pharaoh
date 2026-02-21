// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CDeactivateFreezeRendererTask
	{
		public readonly CLockObject LockObject;

		public CDeactivateFreezeRendererTask(CLockObject lockObject)
		{
			LockObject = lockObject;
		}
	}
}