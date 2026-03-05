// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.07.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTabContentBase : MonoBehaviour
	{
		public virtual void Show()
		{
			gameObject.SetActive(true);
		}
		
		public virtual void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}