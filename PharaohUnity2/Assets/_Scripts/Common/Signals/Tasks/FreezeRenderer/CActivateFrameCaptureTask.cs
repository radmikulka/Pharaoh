// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.08.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CActivateFrameCaptureTask
	{
		public readonly CLockObject LockObject;

		public CActivateFrameCaptureTask(CLockObject lockObject)
		{
			LockObject = lockObject;
		}
	}
}