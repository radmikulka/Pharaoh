// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using AldaEngine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pharaoh.Ui
{
	public class CScreenCloseHandler : MonoBehaviour, IPointerClickHandler
	{
		private TextMeshProUGUI _text;
		private IEventBus _eventBus;

		public void SetEventBus(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void SetText(TextMeshProUGUI text)
		{
			_text = text;
			_text.maskable = false;
			_text.richText = false;
		}

		public void Toggle(bool enable)
		{
			_text.raycastTarget = enable;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			_eventBus.Send(new CEscapePressedSignal());
		}
	}
}