// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.08.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CHudShowTask
	{
		public CLockObject Locker { get; }
		public bool Instant { get; }
		public bool IncludeCurrencies { get; } 
		
		public CHudShowTask(CLockObject locker)
		{
			Locker = locker;
			Instant = false;
			IncludeCurrencies = false;
		}
		
		public CHudShowTask(CLockObject locker, bool instant, bool includeCurrencies)
		{
			Locker = locker;
			Instant = instant;
			IncludeCurrencies = includeCurrencies;
		}
	}
	
	public class CHudHideTask
	{
		public CLockObject Locker { get; }
		public bool Instant { get; }
		public bool IncludeCurrencies { get; }

		public CHudHideTask(CLockObject locker)
		{
			Locker = locker;
			Instant = false;
			IncludeCurrencies = false;
		}

		public CHudHideTask(CLockObject locker, bool instant, bool includeCurrencies)
		{
			Locker = locker;
			Instant = instant;
			IncludeCurrencies = includeCurrencies;
		}
	}
}