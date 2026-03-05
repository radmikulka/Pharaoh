// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.01.2026
// =========================================

using System;
using System.Collections.Generic;
using ServerData;

namespace TycoonBuilder
{
	public class CTrafficPathBuilder
	{
		private readonly List<ITrafficPhase> _phases = new();
		private readonly long _startTime;

		public CTrafficPathBuilder(long startTime)
		{
			_startTime = startTime;
		}
		
		public CTrafficPathBuilder AddTravelPhase(STrafficLineId trafficLine, long duration, bool leadingToTarget)
		{
			_phases.Add(new CTravelTrafficPhase(trafficLine, leadingToTarget, duration));
			return this;
		}

		public CTrafficPathBuilder AddWaitPhase(long duration)
		{
			_phases.Add(new CWaitTrafficPhase(duration));
			return this;
		}

		public CTrafficPath Build()
		{
			return new CTrafficPath(_startTime, _phases.ToArray());
		}
		
		private long GetTotalDuration()
		{
			long total = 0;
			foreach (ITrafficPhase phase in _phases)
			{
				total += phase.Duration;
			}

			return total;
		}
	}
}