// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.08.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CAddFloatingWindowsBlockerTask
	{
		public CLockObject Blocker { get; }
		
		public CAddFloatingWindowsBlockerTask(CLockObject blocker)
		{
			Blocker = blocker;
		}
	}
}