// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CDeactivateFrameCaptureTask
	{
		public readonly CLockObject LockObject;

		public CDeactivateFrameCaptureTask(CLockObject lockObject)
		{
			LockObject = lockObject;
		}
	}
}