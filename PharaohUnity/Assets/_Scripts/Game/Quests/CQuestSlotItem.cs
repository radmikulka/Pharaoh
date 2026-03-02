using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pharaoh
{
	public class CQuestSlotItem : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _descriptionLabel;
		[SerializeField] private Slider _progressBar;
		[SerializeField] private Button _claimButton;
		[SerializeField] private GameObject _claimHighlight;

		private CQuestManager _questManager;
		private int _slotIndex;

		public void Initialize(CQuestManager questManager, int slotIndex)
		{
			_questManager = questManager;
			_slotIndex = slotIndex;
			_claimButton.onClick.AddListener(OnClaimClicked);
		}

		private void OnDestroy()
		{
			_claimButton.onClick.RemoveListener(OnClaimClicked);
		}

		public void Refresh(CActiveQuestState slot, CQuestConfigBase config, float currentProgress)
		{
			if (slot == null || config == null)
			{
				gameObject.SetActive(false);
				return;
			}

			gameObject.SetActive(true);
			_descriptionLabel.text = config.Description;

			float target = config.TargetValue;
			_progressBar.value = target > 0 ? Mathf.Clamp01(currentProgress / target) : 1f;

			bool claimable = slot.IsClaimable && !slot.IsClaimed;
			_claimButton.interactable = claimable;
			_claimHighlight.SetActive(claimable);
		}

		private void OnClaimClicked()
		{
			_questManager.ClaimQuest(_slotIndex);
		}
	}
}
