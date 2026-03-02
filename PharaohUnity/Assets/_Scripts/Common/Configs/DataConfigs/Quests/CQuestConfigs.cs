using System;
using ServerData;

namespace Pharaoh
{
	public class CQuestRuntimeData
	{
		/// <summary>
		/// Cumulative counter incremented by resource events.
		/// Ignored for snapshot-based quests (CHaveBuildingsQuestConfig).
		/// </summary>
		public float AccumulatedProgress;
	}

	public abstract class CQuestConfigBase
	{
		public string Description;
		public SResource[] Reward;

		public abstract float TargetValue { get; }
		public abstract void OnResourceChanged(EResource resource, float delta, CQuestRuntimeData data);
		public abstract void OnBuildingUpgraded(EBuildingId buildingId, int newLevel, CQuestRuntimeData data);
		public abstract void OnBuildingPlaced(EBuildingId buildingId, CQuestRuntimeData data);
		public abstract float GetCurrentProgress(CQuestRuntimeData data, Func<EBuildingId, int> getCount);

		public bool IsCompleted(CQuestRuntimeData data, Func<EBuildingId, int> getCount)
			=> GetCurrentProgress(data, getCount) >= TargetValue;
	}

	/// <summary>Cumulative resource harvested since quest activation.</summary>
	public class CHarvestResourceQuestConfig : CQuestConfigBase
	{
		public EResource Resource;
		public int Amount;

		public override float TargetValue => Amount;

		public override void OnResourceChanged(EResource resource, float delta, CQuestRuntimeData data)
		{
			if (resource == Resource && delta > 0)
				data.AccumulatedProgress += delta;
		}

		public override void OnBuildingUpgraded(EBuildingId buildingId, int newLevel, CQuestRuntimeData data) { }
		public override void OnBuildingPlaced(EBuildingId buildingId, CQuestRuntimeData data) { }

		public override float GetCurrentProgress(CQuestRuntimeData data, Func<EBuildingId, int> getCount)
			=> data.AccumulatedProgress;
	}

	/// <summary>Cumulative resource spent since quest activation.</summary>
	public class CSpendResourceQuestConfig : CQuestConfigBase
	{
		public EResource Resource;
		public int Amount;

		public override float TargetValue => Amount;

		public override void OnResourceChanged(EResource resource, float delta, CQuestRuntimeData data)
		{
			if (resource == Resource && delta < 0)
				data.AccumulatedProgress += -delta;
		}

		public override void OnBuildingUpgraded(EBuildingId buildingId, int newLevel, CQuestRuntimeData data) { }
		public override void OnBuildingPlaced(EBuildingId buildingId, CQuestRuntimeData data) { }

		public override float GetCurrentProgress(CQuestRuntimeData data, Func<EBuildingId, int> getCount)
			=> data.AccumulatedProgress;
	}

	/// <summary>Count of upgrade events matching given building/level criteria.</summary>
	public class CUpgradeBuildingQuestConfig : CQuestConfigBase
	{
		/// <summary>Use default(EBuildingId) to match any building.</summary>
		public EBuildingId BuildingId;
		/// <summary>Use 0 to match any level.</summary>
		public int TargetLevel;
		public int Count;

		public override float TargetValue => Count;

		public override void OnResourceChanged(EResource resource, float delta, CQuestRuntimeData data) { }

		public override void OnBuildingUpgraded(EBuildingId buildingId, int newLevel, CQuestRuntimeData data)
		{
			bool buildingMatches = BuildingId == default || buildingId == BuildingId;
			bool levelMatches = TargetLevel == 0 || newLevel >= TargetLevel;
			if (buildingMatches && levelMatches)
				data.AccumulatedProgress++;
		}

		public override void OnBuildingPlaced(EBuildingId buildingId, CQuestRuntimeData data) { }

		public override float GetCurrentProgress(CQuestRuntimeData data, Func<EBuildingId, int> getCount)
			=> data.AccumulatedProgress;
	}

	/// <summary>Snapshot: current count of a specific building type on the map.</summary>
	public class CHaveBuildingsQuestConfig : CQuestConfigBase
	{
		public EBuildingId BuildingId;
		public int Count;

		public override float TargetValue => Count;

		public override void OnResourceChanged(EResource resource, float delta, CQuestRuntimeData data) { }
		public override void OnBuildingUpgraded(EBuildingId buildingId, int newLevel, CQuestRuntimeData data) { }
		public override void OnBuildingPlaced(EBuildingId buildingId, CQuestRuntimeData data) { }

		public override float GetCurrentProgress(CQuestRuntimeData data, Func<EBuildingId, int> getCount)
			=> getCount(BuildingId);
	}
}
