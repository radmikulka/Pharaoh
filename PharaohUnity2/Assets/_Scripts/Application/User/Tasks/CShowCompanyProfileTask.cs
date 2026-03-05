// =========================================
// AUTHOR: Marek Karaba
// DATE:   27.01.2026
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CShowCompanyProfileTask
	{
		public readonly string UserUid;

		public CShowCompanyProfileTask(string userUid)
		{
			UserUid = userUid;
		}
	}
}