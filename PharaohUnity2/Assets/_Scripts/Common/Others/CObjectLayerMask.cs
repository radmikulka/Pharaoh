// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.05.2025
// =========================================

using UnityEngine;

namespace Pharaoh
{
	public static class CObjectLayerMask
	{
		public static readonly int Default;
		public static readonly int RaycastTarget;
		public static readonly int Ui;
        
        static CObjectLayerMask()
        {
	        Default = 1 << CObjectLayer.Default;
	        RaycastTarget = 1 << CObjectLayer.RaycastTarget;
	        Ui = 1 << CObjectLayer.Ui;
        }
	}
	
	public static class CObjectLayer
	{
		public static readonly int Default;
		public static readonly int RaycastTarget;
		public static readonly int Ui;
		public static readonly int NoBlockingUi;
		
		static CObjectLayer()
		{
			Ui = LayerMask.NameToLayer("UI");
			Default = LayerMask.NameToLayer("Default");
			NoBlockingUi = LayerMask.NameToLayer("NoBlockingUi");
			RaycastTarget = LayerMask.NameToLayer("RaycastTarget");
		}
	}
}