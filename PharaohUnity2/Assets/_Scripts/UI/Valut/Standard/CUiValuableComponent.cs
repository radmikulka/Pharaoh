// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public abstract class CUiValuableComponent<T> : MonoBehaviour, IUiValuableComponent where T : class, IValuable
	{
		public void Init(IValuable valuable)
		{
			bool isValid = IsValidValuable(valuable);
			gameObject.SetActiveObject(isValid);
			if (isValid)
			{
				SetValue(valuable as T);
			}
		}

		protected abstract void SetValue(T value);
		protected abstract bool IsValidValuable(IValuable valuable);
	}
}