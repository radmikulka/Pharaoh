// =========================================
// AUTHOR: Jan Krejsa
// DATE:   23.10.2025
// =========================================

using System;
using System.Text;
using Editor.ServerLauncher;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.EditorToolbarExtensions
{
	public class CServerLauncherEntryDisplay: VisualElement
	{
		private const float CornerRadius = 5f;
		
		private readonly CServerLauncherInstance _linkedInstance;
		private readonly Button _button;
		private readonly Label _serverNameLabel;
		private readonly Label _stateIndicator;

		public CServerLauncherEntryDisplay(CServerLauncherInstance instance)
		{
			_linkedInstance = instance;
			CServerLauncherConfig serverLauncher = CServerLauncherConfig.Instance;
			
			_button = new Button(HandleButtonClicked)
			{
				style =
				{
					borderTopLeftRadius = CornerRadius,
					borderTopRightRadius = CornerRadius,
					borderBottomLeftRadius = CornerRadius,
					borderBottomRightRadius = CornerRadius,
					flexDirection = FlexDirection.Row,
					overflow = Overflow.Visible,
				}
			};

			_serverNameLabel = new Label(instance.ServerName)
			{
				style =
				{
					fontSize = 9f
				}
			};
			_button.Add(_serverNameLabel);
			_stateIndicator = new Label("●");
			_button.Add(_stateIndicator);
			
			_button.AddManipulator(new ContextualMenuManipulator(evt =>
			{
				evt.menu.AppendAction("Build And Run", _ => instance.BuildAndRun(), instance.HasCommandPath ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
				evt.menu.AppendAction("Start Hot Reload", _ => instance.StartHotReload(), instance.HasCommandPath ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
				evt.menu.AppendAction("Kill", _ => instance.KillIfRunning(), instance.CachedState is EServerLauncherInstanceState.Running or EServerLauncherInstanceState.Launching ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
				
				evt.menu.AppendSeparator();
				
				AppendEnumSelectorSubmenu("On Click", 
					() => serverLauncher.BehaviourOnClick, 
					value => serverLauncher.BehaviourOnClick = value);
				AppendEnumSelectorSubmenu("On Play", 
					() => serverLauncher.BehaviourOnPlay, 
					value => serverLauncher.BehaviourOnPlay = value);
				
				evt.menu.AppendSeparator();
				
				evt.menu.AppendAction("Edit config", _ => Selection.activeObject = serverLauncher);
				evt.menu.AppendAction("Open solution in IDE", _ => _linkedInstance.OpenSolutionInIde(), instance.HasSolutionPath ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

				void AppendEnumSelectorSubmenu<TEnum>(string submenuName, Func<TEnum> getter, Action<TEnum> setter)
					where TEnum : struct
				{
					var availableValues = Enum.GetValues(typeof(TEnum)) as TEnum[];
					var currentValue = getter();
					string currentValueLabel = ObjectNames.NicifyVariableName(currentValue.ToString());
					foreach (var value in availableValues)
					{
						evt.menu.AppendAction($"{submenuName} - {currentValueLabel}/{ObjectNames.NicifyVariableName(value.ToString())}", 
							_ =>
							{
								setter?.Invoke(value);
								RefreshState();
							}, 
							currentValue.Equals(value) 
								? DropdownMenuAction.Status.Checked 
								: DropdownMenuAction.Status.Normal);
					}
				}
			}));
			
			_button.RegisterCallback<MouseEnterEvent>(HandleMouseEnter);
			_button.RegisterCallback<MouseLeaveEvent>(HandleMouseExit);

			instance.StateChanged -= RefreshState;
			instance.StateChanged += RefreshState;
			
			RefreshState();
			Add(_button);
		}

		private void HandleButtonClicked()
		{
			_linkedInstance.UpdateState();
			GetClickAction().Action?.Invoke();
			RefreshState();
		}

		private void HandleMouseEnter(MouseEnterEvent evt) => _linkedInstance.UpdateState();
		private void HandleMouseExit(MouseLeaveEvent evt) => _linkedInstance.UpdateState();

		private void RefreshState()
		{
			_button.tooltip = GetTooltipText();
			_stateIndicator.style.color = GetIndicatorColorByState(_linkedInstance.CachedState);
		}

		private (Action Action, string Description) GetClickAction()
		{
			if (IsLogWindowOpened())
			{
				return (CServerLauncherLogWindow.CurrentWindowInstance.Close, "close server console window");
			}
			
			switch (CServerLauncherConfig.Instance.BehaviourOnClick)
			{
				case EServerLauncherClickBehaviour.OpenSolution:
					return (() => _linkedInstance.OpenSolutionInIde(), "open solution in IDE");
				
				case EServerLauncherClickBehaviour.BuildAndRun:
					if (_linkedInstance.CachedState == EServerLauncherInstanceState.NotRunning)
					{
						return (() =>
						{
							{
								_linkedInstance.BuildAndRun();
							}
							CServerLauncherLogWindow.OpenForInstance(_linkedInstance, _button);
						}, "build and run the server");
					}

					return (() => CServerLauncherLogWindow.OpenForInstance(_linkedInstance, _button)
						, "open server console window");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private StyleColor GetIndicatorColorByState(EServerLauncherInstanceState state)
		{
			switch (state)
			{
				case EServerLauncherInstanceState.UnknownState:
					return new StyleColor(new Color(.5f, .5f, .5f, 1f));
				case EServerLauncherInstanceState.NotRunning:
					return new StyleColor(new Color(.9844f, .4257f, .25f, 1f)); 
				case EServerLauncherInstanceState.Running:
					return new StyleColor(new Color(0.3f, .8f, .2f, 1f));
				case EServerLauncherInstanceState.Launching:
					return new StyleColor(new Color(0.8f, .7f, .2f, 1f));
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
		
		private string GetTooltipText()
		{
			string stateDescription = ObjectNames.NicifyVariableName(_linkedInstance.CachedState.ToString());
			string clickActionDescription = GetClickAction().Description;
			return $"{stateDescription}. Click to {clickActionDescription}.";
		}
		
		private bool IsLogWindowOpened()
		{
			CServerLauncherLogWindow currentWindowInstance = CServerLauncherLogWindow.CurrentWindowInstance;
			return currentWindowInstance != null &&
			       currentWindowInstance.LinkedServerInstance == _linkedInstance;
		}
	}
}
