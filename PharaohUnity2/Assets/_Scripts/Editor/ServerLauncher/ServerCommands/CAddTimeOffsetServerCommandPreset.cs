// =========================================
// AUTHOR: Jan Krejsa
// DATE:   31.12.2025
// =========================================

using System;
using AldaEngine;
using UnityEditor;
using UnityEngine;

namespace Editor.ServerLauncher
{
	[Serializable]
	public class CAddTimeOffsetServerCommandPreset: IServerCommandPreset
	{
		public string Label => "Add Time Offset";
		public string Tooltip => "Additively adds offset to current time";
		public CServerCommandInstance CreateInstance() => new CAddTimeOffsetServerCommandInstance(Label);
	}
	
	public class CAddTimeOffsetServerCommandInstance: CServerCommandInstance
	{
		private TimeSpan _timeOffset = TimeSpan.FromDays(1);

		public CAddTimeOffsetServerCommandInstance(string label) : base(label)
		{
		}

		public override void OnGui(Rect rect)
		{
			_timeOffset = CEditorGUI.TimeSpanField(rect, _timeOffset);
		}

		public override float GetGuiHeight() => EditorGUIUtility.singleLineHeight;

		public override string GetCommand()
		{
			if (_timeOffset.Seconds != 0)
				return $"atos {(long) _timeOffset.TotalSeconds}";
			if (_timeOffset.Minutes != 0)
				return $"atom {(long) _timeOffset.TotalMinutes}";
			if (_timeOffset.Hours != 0)
				return $"atoh {(long) _timeOffset.TotalHours}";
			if (_timeOffset.Days != 0)
				return $"atod {(long) _timeOffset.TotalDays}";
			return "atos 0";
		}
	}
}
