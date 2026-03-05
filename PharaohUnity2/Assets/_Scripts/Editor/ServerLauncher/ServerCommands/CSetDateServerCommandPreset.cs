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
	public class CSetDateServerCommandPreset: IServerCommandPreset
	{
		public string Label => "Set Date";
		public string Tooltip => "Sets the server date (and optionally time)";
		public CServerCommandInstance CreateInstance() => new CSetDateServerCommandInstance(Label);
	}
	
	public class CSetDateServerCommandInstance: CServerCommandInstance
	{
		private const float LineSpacing = 2f;
		
		private DateTime _dateTime = DateTime.UtcNow;
		private bool _setTime = true;

		public CSetDateServerCommandInstance(string label) : base(label)
		{
		}

		public override void OnGui(Rect rect)
		{
			Rect firstLineRect = new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			_dateTime = CEditorGUI.DateField(firstLineRect, _dateTime);
			Rect secondLineRect = new(rect.x, rect.y + EditorGUIUtility.singleLineHeight + LineSpacing, rect.width, EditorGUIUtility.singleLineHeight);
			_setTime = EditorGUI.ToggleLeft(secondLineRect, "Set Time", _setTime);
			Rect thirdLineRect = new(rect.x, rect.y + 2f * (EditorGUIUtility.singleLineHeight + LineSpacing), rect.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.BeginDisabledGroup(!_setTime);
			_dateTime = CEditorGUI.TimeField(thirdLineRect, _dateTime);
			EditorGUI.EndDisabledGroup();
		}

		public override float GetGuiHeight() => 3f * EditorGUIUtility.singleLineHeight + 2f * LineSpacing;
		
		public override string GetCommand()
		{
			if (_setTime)
			{
				return $"sdt {_dateTime:d.M.yyyy} {_dateTime:H:mm:ss}";
			}

			return $"sd {_dateTime:d.M.yyyy}";
		}
	}
}
