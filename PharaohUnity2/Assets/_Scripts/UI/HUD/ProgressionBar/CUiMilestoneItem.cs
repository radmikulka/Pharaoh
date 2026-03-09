using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pharaoh
{
    public class CUiMilestoneItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _rewardIcon;

        private float _threshold;
        private bool _claimed;
        private Action<float> _onClaimClicked;

        public float Threshold => _threshold;

        public void Init(float threshold, Sprite rewardSprite, Action<float> onClaimClicked)
        {
            _threshold      = threshold;
            _onClaimClicked = onClaimClicked;

            if (_rewardIcon != null && rewardSprite != null)
            {
                _rewardIcon.sprite = rewardSprite;
            }

            _button.onClick.AddListener(OnButtonClicked);
            SetReachable(false, false);
        }

        public void SetReachable(bool reached, bool claimed)
        {
            _claimed = claimed;
            _button.interactable = reached && !claimed;
        }

        private void OnButtonClicked()
        {
            if (!_claimed)
            {
                _onClaimClicked?.Invoke(_threshold);
            }
        }
    }
}
