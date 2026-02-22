// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using UnityEngine;

namespace Pharaoh
{
	public interface IClickableItem
	{
		void OnClicked(RaycastHit hit);
	}
}