// =========================================
// AUTHOR: Juraj Joscak
// DATE:   22.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public interface IOfferUnavailableComponent
	{
		void SetUnavailable();
	}
	
	public class CAdPriceButtonChanger : CUiValuableComponent<CAdvertisementValuable>, IOfferUnavailableComponent
	{
		[SerializeField] private Sprite _adSprite;
		[SerializeField] private Sprite _defaultSprite;
		[SerializeField] private Sprite _unavailableSprite;
		[SerializeField] private CUiComponentImage _buttonImage;
		
		protected override void SetValue(CAdvertisementValuable value)
		{
			if (value != null)
			{
				_buttonImage.SetSprite(_adSprite);
			}
			else
			{
				_buttonImage.SetSprite(_defaultSprite);
			}
		}
		
		public void SetUnavailable()
		{
			_buttonImage.SetSprite(_unavailableSprite);
		}

		protected override bool IsValidValuable(IValuable valuable)
		{
			return true;
		}
	}
}