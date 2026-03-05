// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.07.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CRestartGameHandler : CBaseRestartGameHandler
	{
		private readonly ISceneManager _sceneManager;
		
		public CRestartGameHandler(IEventBus eventBus, ISceneManager sceneManager) : base(eventBus)
		{
			_sceneManager = sceneManager;
		}

		protected override void LoadBaseScene()
		{
			_sceneManager.LoadConnectingScene();
		}
	}
}