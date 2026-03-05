// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.07.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public abstract class CBaseEditorCommands : MonoBehaviour
	{
		public abstract void SavePreset();
		public abstract void ForceInternalHitError();
		public abstract void AddXp(float normalizedValue);
		public abstract void CheatVehicle(EVehicle vehicle);
		public abstract void RunMemoryDebug();
	}
}