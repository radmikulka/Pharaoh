// =========================================
// DATE:   02.03.2026
// =========================================

using ServerData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pharaoh.Ui
{
	public class CResearchItemView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _nameLabel;
		[SerializeField] private TextMeshProUGUI _costLabel;
		[SerializeField] private Button _buyButton;
		[SerializeField] private GameObject _purchasedOverlay;
		[SerializeField] private GameObject _lockedOverlay;

		private EResearchId _researchId;
		private System.Action<EResearchId> _onBuyClicked;

		private void Awake()
		{
			_buyButton.onClick.AddListener(OnBuyClicked);
		}

		public void Setup(CResearchConfig config, EResearchState state, System.Action<EResearchId> onBuyClicked)
		{
			_researchId = config.Id;
			_onBuyClicked = onBuyClicked;

			_nameLabel.text = config.DisplayName;
			_costLabel.text = BuildCostText(config);

			_purchasedOverlay.SetActive(state == EResearchState.Purchased);
			_lockedOverlay.SetActive(state == EResearchState.Locked);
			_buyButton.interactable = state == EResearchState.Available;
		}

		private static string BuildCostText(CResearchConfig config)
		{
			if (config.Cost == null || config.Cost.Length == 0)
				return string.Empty;

			System.Text.StringBuilder sb = new();
			foreach (SResource cost in config.Cost)
				sb.Append($"{cost.Amount} {cost.Id}  ");
			return sb.ToString().TrimEnd();
		}

		private void OnBuyClicked()
		{
			_onBuyClicked?.Invoke(_researchId);
		}

		private void OnDestroy()
		{
			_buyButton.onClick.RemoveListener(OnBuyClicked);
		}
	}

	public enum EResearchState
	{
		Locked,
		Available,
		Purchased,
	}
}
