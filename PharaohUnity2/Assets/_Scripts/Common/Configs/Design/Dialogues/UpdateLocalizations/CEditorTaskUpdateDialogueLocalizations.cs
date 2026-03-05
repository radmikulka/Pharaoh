using System.Threading.Tasks;
using AldaEngine;
using UnityEngine;

// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.09.2025
// =========================================

#if UNITY_EDITOR
namespace TycoonBuilder.Configs.Design
{
    [CreateAssetMenu(fileName = "UpdateDialogueLocalizations",
        menuName = "____TycoonBuilder/EditorTasks/Steps/UpdateDialogueLocalizations")]
    internal class CEditorTaskUpdateDialogueLocalizations : CBaseEditorTaskStep
    {
        public override Task Execute()
        {
            CEditorEpisodesLocalizationUpdater episodeUpdater = new ();
            episodeUpdater.UpdateLocalizations();
            
            Debug.Log("Dialogue localizations update completed.");
            return Task.CompletedTask;
        }
    }
}
#endif