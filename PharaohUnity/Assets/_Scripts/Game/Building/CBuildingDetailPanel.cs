using System.Text;
using AldaEngine;
using AldaEngine.AldaFramework;
using Pharaoh.Map;
using ServerData;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Pharaoh.Building
{
	public class CBuildingDetailPanel : MonoBehaviour, IEscapable
	{
		[SerializeField] private Canvas _canvas;
		[SerializeField] private RectTransform _panelRoot;
		[SerializeField] private CUiComponentText _nameText;
		[SerializeField] private CUiComponentText _levelText;
		[SerializeField] private CUiComponentText _upgradeCostText;
		[SerializeField] private Button _upgradeButton;
		[SerializeField] private Button _outsideBlocker;

		private CBuildingManager _buildingManager;
		private CDesignBuildingsConfigs _buildingConfigs;
		private COwnedResources _ownedResources;
		private IMissionController _missionController;
		private IMainCameraProvider _mainCameraProvider;
		private CEscapeHandler _escapeHandler;
		private CMapInstance _mapInstance;
		private IEventBus _eventBus;

		private SCellCoord _currentCell;
		private bool _isOpen;

		[Inject]
		private void Inject(
			CBuildingManager buildingManager,
			CDesignBuildingsConfigs buildingConfigs,
			COwnedResources ownedResources,
			IMissionController missionController,
			IMainCameraProvider mainCameraProvider,
			CEscapeHandler escapeHandler,
			CMapInstance mapInstance,
			IEventBus eventBus)
		{
			_buildingManager = buildingManager;
			_buildingConfigs = buildingConfigs;
			_ownedResources = ownedResources;
			_missionController = missionController;
			_mainCameraProvider = mainCameraProvider;
			_escapeHandler = escapeHandler;
			_mapInstance = mapInstance;
			_eventBus = eventBus;

			_eventBus.Subscribe<COpenBuildingDetailSignal>(OnOpenBuildingDetail);
			_outsideBlocker.onClick.AddListener(Close);
			_upgradeButton.onClick.AddListener(OnUpgradeClicked);
			_canvas.enabled = false;
		}

		private void OnDestroy()
		{
			_outsideBlocker.onClick.RemoveListener(Close);
			_upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

			if (_isOpen)
				_escapeHandler.UnregisterEscapable(this);
		}

		private void OnOpenBuildingDetail(COpenBuildingDetailSignal signal)
		{
			_currentCell = signal.Cell;
			Show();
		}

		private void Show()
		{
			if (_isOpen)
				Close();

			CBuilding building = _buildingManager.GetBuildingAtCell(_currentCell);
			if (building == null)
				return;

			CBuildingConfig config = _buildingConfigs.GetBuilding(building.Id);
			if (config == null)
				return;

			_nameText.SetValue(config.DisplayName);
			_levelText.SetValue($"Level {building.Level}");

			RefreshUpgradeButton(building, config);

			_canvas.enabled = true;
			_escapeHandler.RegisterEscapable(this);
			_isOpen = true;
			UpdatePosition();
		}

		private void RefreshUpgradeButton(CBuilding building, CBuildingConfig config)
		{
			bool isMaxLevel = building.Level >= config.Levels.Length;

			if (isMaxLevel)
			{
				_upgradeButton.interactable = false;
				if (_upgradeCostText != null)
					_upgradeCostText.SetValue("Max Level");
				return;
			}

			SResource[] upgradeCost = config.Levels[building.Level].LevelCost;
			EMissionId missionId = _missionController.ActiveMissionId;
			bool canAfford = _ownedResources.HasEnough(missionId, upgradeCost);

			_upgradeButton.interactable = canAfford;

			if (_upgradeCostText != null)
				_upgradeCostText.SetValue(FormatCost(upgradeCost));
		}

		public void Close()
		{
			_canvas.enabled = false;
			_escapeHandler.UnregisterEscapable(this);
			_isOpen = false;
		}

		public void OnEscape()
		{
			Close();
		}

		private void LateUpdate()
		{
			if (_isOpen)
				UpdatePosition();
		}

		private void UpdatePosition()
		{
			Vector3 worldPos = new Vector3(_currentCell.X, 0f, _currentCell.Y);
			Vector3 screenPos = _mainCameraProvider.Camera.WorldToScreenPoint(worldPos);

			if (screenPos.z < 0)
			{
				Close();
				return;
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_canvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPoint);
			_panelRoot.anchoredPosition = localPoint;
		}

		private void OnUpgradeClicked()
		{
			_eventBus.Send(new CBuildingUpgradeRequestSignal(_currentCell));
			Close();
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
