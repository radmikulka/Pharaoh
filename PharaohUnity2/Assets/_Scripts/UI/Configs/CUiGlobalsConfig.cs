// =========================================
// AUTHOR: Juraj Joscak
// DATE:   01.08.2025
// =========================================

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TycoonBuilder.Configs
{
	[CreateAssetMenu(menuName = "AldaGames/TycoonBuilder/Configs/UiGlobalsConfig", fileName = "UiGlobalsConfig")]
	public class CUiGlobalsConfig : ScriptableObject
	{
		[Header("Currencies Text Colors")]
		[SerializeField] private Color _notEnoughCurrencyColor;
		[SerializeField] private Color _enoughCurrencyColor;
		[SerializeField] private Color _gainCurrencyColor;
		
		[Header("InactiveButtonSprites")] 
		[SerializeField] private Sprite _inactiveSpriteS;
		[SerializeField] private Sprite _inactiveSpriteM;
		[SerializeField] private Sprite _inactiveSpriteL;

		[Header("TextColors")] 
		[SerializeField] private Color _lightBlueTextColor;
		[SerializeField] private Color _goldTextColor;
		[SerializeField] private Color _warningColor;
		[SerializeField] private Color _unhighlightedTextColor;
		
		[Header("Materials")]
		[SerializeField] private Material _grayScaleMaterial;
		
		public Color NotEnoughCurrencyColor => _notEnoughCurrencyColor;
		public Color EnoughCurrencyColor => _enoughCurrencyColor;
		public Color GainCurrencyColor => _gainCurrencyColor;
		public Color LightBlueTextColor => _lightBlueTextColor;
		public Color GoldTextColor => _goldTextColor;
		public Color WarningColor => _warningColor;
		public Color UnhighlightedTextColor => _unhighlightedTextColor;
		public Material GrayScaleMaterial => _grayScaleMaterial;

		public Sprite GetInactiveSprite(EButtonSize size)
		{
			return size switch
			{
				EButtonSize.S => _inactiveSpriteS,
				EButtonSize.M => _inactiveSpriteM,
				EButtonSize.L => _inactiveSpriteL,
				_ => throw new NotImplementedException($"Button Size Inactive sprite not implemented: {size}")
			};
		}
	}
}