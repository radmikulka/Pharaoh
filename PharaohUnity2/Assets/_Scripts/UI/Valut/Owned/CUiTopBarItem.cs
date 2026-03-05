// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;

namespace TycoonBuilder
{
	public class CUiTopBarItem : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private ETopBarItem _id;
		[SerializeField, Child] private CUiTopBarItemIcon _topBarItemIcon;
		[SerializeField, Child(Flag.IncludeInactive)] protected CUiButton _button;
		
		private Transform _initialParent;

		public ETopBarItem Id => _id;
		
		public virtual void Construct()
		{
			_initialParent = transform.parent;
		}

		public virtual void Initialize()
		{
			InitIcon();
		}
		
		public void ShowButton(bool state)
		{
			_button.gameObject.SetActiveObject(state);
		}

		public void SetParent(Transform parent)
		{
			transform.SetParent(parent, true);
		}

		public void ResetParent()
		{
			transform.SetParent(_initialParent, true);
		}

		private void InitIcon()
		{
			_topBarItemIcon.SetIcon(_id);
		}
	}
}