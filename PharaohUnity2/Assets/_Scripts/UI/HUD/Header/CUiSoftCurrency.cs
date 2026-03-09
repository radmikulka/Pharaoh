using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public class CUiSoftCurrency : MonoBehaviour
    {
        [SerializeField] private CUiComponentText _valueText;

        private IEventBus _eventBus;

        [Inject]
        private void Inject(IEventBus eventBus)
        {
            _eventBus = eventBus;

            _eventBus.Subscribe<CSoftCurrencyChangedSignal>(OnSoftCurrencyChanged);
            _eventBus.Subscribe<CMissionActivatedSignal>(OnMissionActivated);

            RefreshDisplay();
        }

        private void OnSoftCurrencyChanged(CSoftCurrencyChangedSignal signal)
        {
            _valueText.SetValue(signal.NewValue.ToString());
        }

        private void OnMissionActivated(CMissionActivatedSignal signal)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            CMissionDataResponse response = _eventBus.ProcessTask<CMissionDataRequest, CMissionDataResponse>();
            int value = response?.SoftCurrency ?? 0;
            _valueText.SetValue(value.ToString());
        }
    }
}
