// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.01.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CTrafficPath
	{
		private readonly ITrafficPhase[] _phases;
		public readonly long StartTime;

		public CTrafficPath(long startTime, ITrafficPhase[] phases)
		{
			StartTime = startTime;
			_phases = phases;
		}

		public long GetPhaseStartTime(ITrafficPhase phase)
		{
			long phaseStartTime = 0;
			foreach (ITrafficPhase p in _phases)
			{
				if (p == phase)
				{
					return StartTime + phaseStartTime;
				}
				
				phaseStartTime += p.Duration;
			}

			return StartTime;
		}

		public int GetRelevantPhasesSequenceNoAlloc(long time, ERegion region, CTravelTrafficPhase[] target)
		{
			int? phaseIndex = GetPhaseIndexInTime(time);
			if(!phaseIndex.HasValue)
				return 0;

			int result = 0;
			for (int i = phaseIndex.Value; i < _phases.Length; i++)
			{
				if (_phases[i] is not CTravelTrafficPhase travelPhase)
					return result;

				if (travelPhase.Region != ERegion.None && travelPhase.Region != region)
					return result;

				target[result++] = travelPhase;
			}

			return result;
		}

		public IEnumerable<CTravelTrafficPhase> GetRelevantPhasesSequence(long time, ERegion region)
		{
			int? phaseIndex = GetPhaseIndexInTime(time);
			if(!phaseIndex.HasValue)
				yield break;

			for (int i = phaseIndex.Value; i < _phases.Length; i++)
			{
				if (_phases[i] is not CTravelTrafficPhase travelPhase)
					yield break;

				if (travelPhase.Region != ERegion.None && travelPhase.Region != region)
					yield break;

				yield return travelPhase;
			}
		}

		private int? GetPhaseIndexInTime(long time)
		{
			long phaseStartTime = StartTime;
			for (int i = 0; i < _phases.Length; i++)
			{
				ITrafficPhase phase = _phases[i];
				bool containsTime = phaseStartTime <= time && time < phaseStartTime + phase.Duration;
				phaseStartTime += phase.Duration;

				if (containsTime)
					return i;
			}

			return null;
		}
	}
}