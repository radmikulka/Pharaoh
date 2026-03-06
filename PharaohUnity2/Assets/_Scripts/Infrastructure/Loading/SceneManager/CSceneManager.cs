// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ServerData;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Pharaoh
{
    public class CSceneManager : IInitializable, IDestroyable, ISceneManager
    {
        private readonly CSceneLoadingTasksBuffer _tasksBuffer = new();
        private readonly CAldaFramework _aldaFramework;
        private readonly IBundleManager _bundleManager;
        private readonly CLoadedScenes _loadedScenes;
        private readonly CResourceConfigs _configs;
        private readonly CEventSystem _eventSystem;
        private readonly IEventBus _eventBus;

        private bool _baseSceneActivationAllowed;

        public CSceneManager(
            CAldaFramework aldaFramework, 
            IBundleManager bundleManager, 
            CEventSystem eventSystem, 
            CResourceConfigs configs, 
            IEventBus eventBus
            )
        {
            _loadedScenes = new CLoadedScenes(configs.Scenes.GetConfigs().ToArray());
            _aldaFramework = aldaFramework;
            _bundleManager = bundleManager;
            _eventSystem = eventSystem;
            _eventBus = eventBus;
            _configs = configs;
        }

        public void Initialize()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;

            AddCurrentScene();
        }

        private void AddCurrentScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            _loadedScenes.AddScene(activeScene);
        }

        public void LoadConnectingScene()
        {
            _tasksBuffer.Enqueue(async () =>
            {
                ResetGame();
                await LoadSceneAsync(ESceneId.Connecting, LoadSceneMode.Single, CancellationToken.None);
                _eventBus.Clear();
            });
        }
        
        public async UniTask<Scene> LoadSceneAsync(ESceneId sceneId, LoadSceneMode mode, CancellationToken ct)
        {
            Scene scene = await LoadOrActivateSceneAsync(sceneId, mode, ct);
            UnloadResources();
            return scene;
        }

        public void SetActiveScene(ESceneId sceneId)
        {
            CSceneResourceConfig sceneConfig = _configs.Scenes.GetConfig(sceneId);
            Scene scene = SceneManager.GetSceneByName(sceneConfig.SceneName);
            SceneManager.SetActiveScene(scene);
        }

        public async UniTask StartBaseSceneLoadingAsync(CancellationToken ct)
        {
            _baseSceneActivationAllowed = false;
            
            CSceneResourceConfig sceneConfig = _configs.Scenes.GetConfig(ESceneId.BaseGame);
            AsyncOperation loadProcess = SceneManager.LoadSceneAsync(sceneConfig.SceneName, LoadSceneMode.Additive);
            
            Assert.IsNotNull(loadProcess);
            
            loadProcess.allowSceneActivation = false;
            while (loadProcess.progress < 0.9f)
            {
                await UniTask.DelayFrame(1, cancellationToken: ct);
            }

            await UniTask.WaitUntil(() => _baseSceneActivationAllowed, cancellationToken: ct);
            loadProcess.allowSceneActivation = true;
            await loadProcess;
        }
        
        public void AllowBaseSceneActivation()
        {
            _baseSceneActivationAllowed = true;
        }
        
        public async UniTask UnloadSceneAsync(ESceneId sceneId, CancellationToken ct)
        {
            CSceneResourceConfig sceneConfig = _configs.Scenes.GetConfig(sceneId);
            _aldaFramework.DestroyScene(sceneConfig.SceneName);
			
            await TryUnloadScene();	// sometimes unity may throw exception when unloading scene without reason - it is not problem - its just unity
            return;

            async UniTask TryUnloadScene()
            {
                try
                {
                    Scene scene = SceneManager.GetSceneByName(sceneConfig.SceneName);
                    if(!scene.isLoaded)
                        return;
                    await SceneManager.UnloadSceneAsync(sceneConfig.SceneName).WithCancellation(ct);

                    EBundleId[] unusedBundles = GetUnusedBundles(sceneConfig.BundleIds).ToArray();
                    foreach (EBundleId bundleId in unusedBundles)
                    {
                        _bundleManager.UnloadBundle((int)bundleId, true);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        private IEnumerable<EBundleId> GetUnusedBundles(EBundleId[] sceneBundles)
        {
            foreach (EBundleId sceneBundle in sceneBundles)
            {
                bool isBundleUsed = _loadedScenes.IsBundleUsed(sceneBundle);
                if (isBundleUsed)
                    continue;
                yield return sceneBundle;
            }
        }
        
        private async UniTask<Scene> LoadOrActivateSceneAsync(ESceneId sceneId, LoadSceneMode mode, CancellationToken ct)
        {
            CSceneResourceConfig sceneConfig = _configs.Scenes.GetConfig(sceneId);
            if (!_loadedScenes.IsSceneLoaded(sceneConfig.Id) || mode == LoadSceneMode.Single)
            {
                await SceneManager.LoadSceneAsync(sceneConfig.SceneName, mode).WithCancellation(ct);
                _eventBus.Send(new CSceneLoadedSignal(sceneConfig.Id));
                await UniTask.DelayFrame(1, cancellationToken: ct);
                return SceneManager.GetSceneByName(sceneConfig.SceneName);
            }
			
            Scene scene = SceneManager.GetSceneByName(sceneConfig.SceneName);
            SetActiveScene(scene, true);
            return scene;
        }
        
        private void SetActiveScene(Scene scene, bool state)
        {
            GameObject[] sceneRoots = scene.GetRootGameObjects();
            foreach (GameObject root in sceneRoots)
            {
                root.SetActiveObject(state);
            }
        }

        public void OnContextDestroy(bool appExits)
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneUnloaded -= SceneUnloaded;
            _loadedScenes.Clear();
        }
        
        public void ResetGame()
        {
            DOTween.KillAll();
            _eventSystem.ClearAll();
            _aldaFramework.ExitApp();
			
            SceneManager.LoadScene("_Blank");
        }
        
        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _loadedScenes.AddScene(scene);
        }

        private void SceneUnloaded(Scene scene)
        {
            _loadedScenes.Remove(scene);
        }
        
        private void UnloadResources()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}