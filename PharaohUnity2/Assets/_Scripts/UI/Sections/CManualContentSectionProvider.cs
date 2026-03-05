// =========================================
// AUTHOR: Marek Karaba
// DATE:   16.07.2025
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CManualContentSectionProvider : MonoBehaviour, ISectionContentProvider, IInitializable
	{
		[SerializeField] private CSectionContentBase[] _sectionContentBase;

		public void Initialize()
		{
			foreach (CSectionContentBase content in _sectionContentBase)
			{
				content.Hide();
			}
		}

		public CSectionContentBase[] GetContent()
		{
			return _sectionContentBase;
		}
	}
}