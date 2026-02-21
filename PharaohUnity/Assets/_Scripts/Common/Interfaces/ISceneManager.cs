// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine.SceneManagement;

namespace Pharaoh
{
    public interface ISceneManager
    {
        void ResetGame();
        void LoadConnectingScene();
        UniTask<Scene> LoadSceneAsync(ESceneId sceneId, LoadSceneMode mode, CancellationToken ct);
        UniTask StartBaseSceneLoadingAsync(CancellationToken ct);
        void SetActiveScene(ESceneId sceneId);
        void AllowBaseSceneActivation();
        UniTask UnloadSceneAsync(ESceneId sceneId, CancellationToken ct);
    }
}