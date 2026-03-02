using System.Text;
using AldaEngine;
using ServerData;
using UnityEngine;
using UnityEngine.UI;

namespace Pharaoh.Building
{
	public class CBuildingMenuItem : MonoBehaviour
	{
		[SerializeField] private Image _icon;
		[SerializeField] private CUiComponentText _nameText;
		[SerializeField] private CUiComponentText _costText;
		[SerializeField] private Button _selectButton;
		[SerializeField] private CanvasGroup _canvasGroup;

		private EBuildingId _buildingId;
		private SCellCoord _cell;
		private IEventBus _eventBus;
		private CBuildingMenuPanel _panel;
		private bool _affordable;

		public void Initialize(EBuildingId buildingId, Sprite icon, string displayName, SCellCoord cell, IEventBus eventBus, CBuildingMenuPanel panel, SResource[] cost, bool affordable)
		{
			_buildingId = buildingId;
			_cell = cell;
			_eventBus = eventBus;
			_panel = panel;
			_affordable = affordable;

			_icon.sprite = icon;
			_nameText.SetValue(displayName);
			_selectButton.onClick.AddListener(OnSelectClicked);

			if (_costText != null)
				_costText.SetValue(FormatCost(cost));

			if (_canvasGroup != null)
				_canvasGroup.alpha = affordable ? 1f : 0.4f;

			_selectButton.interactable = affordable;
		}

		private void OnDestroy()
		{
			_selectButton.onClick.RemoveListener(OnSelectClicked);
		}

		private void OnSelectClicked()
		{
			if (!_affordable)
				return;

			_eventBus.Send(new CBuildingPlacementRequestSignal(_buildingId, _cell));
			_panel.Close();
		}

		private static string FormatCost(SResource[] cost)
		{
			if (cost == null || cost.Length == 0)
				return string.Empty;

			var sb = new StringBuilder();
			for (int i = 0; i < cost.Length; i++)
			{
				if (i > 0)
					sb.Append(", ");
				sb.Append(cost[i].Amount);
				sb.Append(' ');
				sb.Append(cost[i].Id.ToString());
			}
			return sb.ToString();
		}
	}
}
