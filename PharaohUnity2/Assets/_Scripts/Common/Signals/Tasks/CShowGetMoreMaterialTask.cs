// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CShowGetMoreMaterialTask
	{
		public readonly SResource RequiredResource;
		public readonly string Source;
		
		public CShowGetMoreMaterialTask(SResource requiredResource, string source)
		{
			RequiredResource = requiredResource;
			Source = source;
		}
	}
}