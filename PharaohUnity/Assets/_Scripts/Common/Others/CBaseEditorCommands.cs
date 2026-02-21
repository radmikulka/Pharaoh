// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.07.2025
// =========================================

using ServerData;
using UnityEngine;

namespace Pharaoh
{
	public abstract class CBaseEditorCommands : MonoBehaviour
	{
		public abstract void SavePreset();
		public abstract void RunMemoryDebug();
	}
}