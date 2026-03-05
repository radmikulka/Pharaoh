// =========================================
// AUTHOR: Juraj Joscak
// DATE:   24.07.2025
// =========================================

using System;
using AldaEngine;
using ServerData;

namespace TycoonBuilder.Ui
{
	public class CCurrencyParticleStartedSignal : IEventBusSignal
	{
		public readonly Guid Guid;
		public readonly EValuable CurrencyType;
		public readonly EResource Resource;
		
		public CCurrencyParticleStartedSignal(Guid guid, EValuable currencyType, EResource resource)
		{
			Guid = guid;
			CurrencyType = currencyType;
			Resource = resource;
		}
	}
	
	public class CCurrencyParticleFinishedSignal : IEventBusSignal
	{
		public readonly Guid Guid;
		public readonly EValuable CurrencyType;
		public readonly EResource Resource;
		
		public CCurrencyParticleFinishedSignal(Guid guid, EValuable currencyType, EResource resource)
		{
			Guid = guid;
			CurrencyType = currencyType;
			Resource = resource;
		}
	}
	
	public class CCurrencyParticleStepFinishedSignal : IEventBusSignal
	{
		public readonly Guid Guid;
		public readonly IValuable Diff;
		
		public CCurrencyParticleStepFinishedSignal(Guid guid, IValuable diff)
		{
			Guid = guid;
			Diff = diff;
		}
	}
}