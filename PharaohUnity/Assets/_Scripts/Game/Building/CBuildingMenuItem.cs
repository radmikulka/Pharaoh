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
		[SerializeField] private Button _selectButton;

		private EBuildingId _buildingId;
		private SCellCoord _cell;
		private IEventBus _eventBus;
		private CBuildingMenuPanel _panel;

		public void Initialize(EBuildingId buildingId, Sprite icon, string displayName, SCellCoord cell, IEventBus eventBus, CBuildingMenuPanel panel)
		{
			_buildingId = buildingId;
			_cell = cell;
			_eventBus = eventBus;
			_panel = panel;
			_icon.sprite = icon;
			_nameText.SetValue(displayName);
			_selectButton.onClick.AddListener(OnSelectClicked);
		}

		private void OnDestroy()
		{
			_selectButton.onClick.RemoveListener(OnSelectClicked);
		}

		private void OnSelectClicked()
		{
			_eventBus.Send(new CBuildingPlacementRequestSignal(_buildingId, _cell));
			_panel.Close();
		}
	}
}
