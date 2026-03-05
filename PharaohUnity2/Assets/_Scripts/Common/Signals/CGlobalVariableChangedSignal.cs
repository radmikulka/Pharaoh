// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CGlobalVariableChangedSignal : IEventBusSignal
	{
		public readonly EGlobalVariable GlobalVariable;

		public CGlobalVariableChangedSignal(EGlobalVariable globalVariable)
		{
			GlobalVariable = globalVariable;
		}
	}
}