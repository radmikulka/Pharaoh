// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.02.2026
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiFrameValuableComponent : CUiValuableComponent<IValuable>, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentText _frameText;

		private ITranslation _translation;

		[Inject]
		private void Inject(ITranslation translation)
		{
		    _translation = translation;
		}
		
		protected override void SetValue(IValuable value)
		{
			string text = _translation.GetText("Generic.CompanyFrame");
			_frameText.SetValue(text);
		}
		
		protected override bool IsValidValuable(IValuable valut)
		{
			return valut is CFrameValuable;
		}
	}
}