// =========================================
// AUTHOR: Jan Krejsa
// DATE:   31.12.2025
// =========================================

using UnityEditor;
using UnityEngine;

namespace Editor.ServerLauncher
{
	public class CServerLauncherCommandPopUp: PopupWindowContent
	{
		private readonly CServerLauncherInstance _serverInstance;
		private readonly CServerCommandInstance _commandInstance;

		private const float WindowWidth = 250f;
		private const float Padding = 4f;
		private const float Spacing = 2f;
		private const float LabelHeight = 25f;
		private const float ButtonHeight = 20f;

		public CServerLauncherCommandPopUp(CServerLauncherInstance serverInstance, CServerCommandInstance commandInstance)
		{
			_serverInstance = serverInstance;
			_commandInstance = commandInstance;
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(WindowWidth, LabelHeight + _commandInstance.GetGuiHeight() + ButtonHeight + 2*Padding + 2*Spacing);
		}

		public override void OnGUI(Rect rect)
		{
			base.OnGUI(rect);
			rect.xMin += Padding;
			rect.xMax -= Padding;
			rect.yMin += Padding;
			rect.yMax -= Padding;
			
			Rect labelRect = new(rect.x, rect.y, rect.width, LabelHeight);
			EditorGUI.LabelField(labelRect, _commandInstance.Label, EditorStyles.boldLabel);
			rect.yMin += LabelHeight + Spacing;
			
			Rect buttonRect = new(rect.x, rect.yMax - ButtonHeight, rect.width, ButtonHeight);
			rect.yMax -= ButtonHeight + Spacing;
			if (GUI.Button(buttonRect, "Execute"))
			{
				string commandString = _commandInstance.GetCommand();
				_serverInstance.SendInputToBuildProcess(commandString);
				editorWindow.Close();
			}
			
			_commandInstance.OnGui(rect);
		}
	}
}
