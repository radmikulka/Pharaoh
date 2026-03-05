// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CMenuLevelSetter : MonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentText _text;
		
		private ITranslation _translation;
		
		 [Inject]
		 private void Inject(ITranslation translation)
		 {
			 _translation = translation;
		 }
		 
		 public void SetLevel(int level)
		 {
			 string text = _translation.GetText("Generic.Lvl", level);
			 _text.SetValue(text);
		 }
	}
}