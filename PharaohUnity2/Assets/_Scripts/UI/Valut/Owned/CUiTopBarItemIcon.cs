// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;

namespace TycoonBuilder
{
	public abstract class CUiTopBarItemIcon : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		internal abstract void SetIcon(ETopBarItem id);
	}
}