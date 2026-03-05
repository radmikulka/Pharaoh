// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CUiDevicePictureVisual : ValidatedMonoBehaviour
	{
		[SerializeField, Self] private CUiComponentImage _image;
		[SerializeField] private Sprite _appleSprite;
		[SerializeField] private Sprite _androidSprite;

		public void SetPlatform(EPlatform platform)
		{
			switch (platform)
			{
				case EPlatform.Android:
					SetImage(_androidSprite);
					break;
				case EPlatform.Apple:
					SetImage(_appleSprite);
					break;
			}
		}
		
		private void SetImage(Sprite sprite)
		{
			_image.SetSprite(sprite);
		}
	}
}