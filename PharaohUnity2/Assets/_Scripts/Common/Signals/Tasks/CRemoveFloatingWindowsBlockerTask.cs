// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.08.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CRemoveFloatingWindowsBlockerTask
	{
		public CLockObject Blocker { get; }
		
		public CRemoveFloatingWindowsBlockerTask(CLockObject blocker)
		{
			Blocker = blocker;
		}
	}
}