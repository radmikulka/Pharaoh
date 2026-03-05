// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CSceneLightingLock
	{
		public enum ESceneLightingLockPriority
		{
			None,
			Region,
			ShopDetail,
			VehicleDetail,
			VehicleShowcase
		}
		
		public readonly ESceneId SceneId;
		public readonly ESceneLightingLockPriority Priority;

		public CSceneLightingLock(ESceneId sceneId, ESceneLightingLockPriority priority)
		{
			SceneId = sceneId;
			Priority = priority;
		}

		public static CSceneLightingLock VehicleDetail()
		{
			return new CSceneLightingLock(ESceneId.VehicleDetail, ESceneLightingLockPriority.VehicleDetail);
		}
		
		public static CSceneLightingLock VehicleShowcase()
		{
			return new CSceneLightingLock(ESceneId.VehicleShowcase, ESceneLightingLockPriority.VehicleShowcase);
		}
		
		public static CSceneLightingLock ShopDetail()
		{
			return new CSceneLightingLock(ESceneId.ShopDetailScene, ESceneLightingLockPriority.ShopDetail);
		}
		
		public static CSceneLightingLock Region(ESceneId regionScene)
		{
			return new CSceneLightingLock(regionScene, ESceneLightingLockPriority.Region);
		}
	}
}