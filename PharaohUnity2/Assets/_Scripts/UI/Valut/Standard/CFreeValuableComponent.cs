// =========================================
// AUTHOR: Juraj Joscak
// DATE:   22.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CFreeValuableComponent : CUiValuableComponent<IValuable>, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentText _text;

		private ITranslation _translation;
		
		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}
		
		protected override void SetValue(IValuable value)
		{
			_text.SetValue(_translation.GetText("Generic.Free"));
		}

		protected override bool IsValidValuable(IValuable valuable)
		{
			return valuable is CFreeValuable or CFreeNoHitValuable;
		}
	}
}