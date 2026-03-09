using AldaEngine;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public class CUiCurrency : MonoBehaviour
    {
        [SerializeField] private EValuable _valuable;
        [SerializeField] private CUiComponentText _valueText;

        private CAnimatedCurrency _animatedCurrency;

        [Inject]
        private void Inject(CUser user)
        {
            _animatedCurrency = user.AnimatedCurrencies.GetCurrency(_valuable);
            _animatedCurrency.ValueChanged += OnValueChanged;
            RefreshDisplay();
        }

        private void OnValueChanged(int value)
        {
            _valueText.SetValue(value.ToString());
        }

        private void RefreshDisplay()
        {
            _valueText.SetValue(_animatedCurrency.Value.ToString());
        }

        private void OnDestroy()
        {
            if (_animatedCurrency != null)
            {
                _animatedCurrency.ValueChanged -= OnValueChanged;
            }
        }
    }
}
