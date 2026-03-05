// =========================================
// AUTHOR: Jan Krejsa
// DATE:   31.12.2025
// =========================================

using System;
using UnityEngine;

namespace Editor.ServerLauncher
{
	[Serializable]
	public class CSimpleServerCommand: IServerCommandPreset
	{
		[SerializeField] private string _label;
		[SerializeField] private string _tooltip;
		[SerializeField] private string _command;

		public string Label => _label;
		public string Tooltip => _tooltip;
		public CServerCommandInstance CreateInstance()
		{
			return new CSimpleServerCommandInstance(_command, _label);
		}
	}
	
	public class CSimpleServerCommandInstance: CServerCommandInstance
	{
		private readonly string _command;
		
		public CSimpleServerCommandInstance(string command, string label) : base(label)
		{
			_command = command;
		}

		public override float GetGuiHeight() => 0f;
		public override string GetCommand() => _command;
	}
}
