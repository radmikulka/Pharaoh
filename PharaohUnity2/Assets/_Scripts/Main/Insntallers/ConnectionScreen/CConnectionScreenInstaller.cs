// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using TycoonBuilder.Loading;

namespace TycoonBuilder
{
    public class CConnectionScreenInstaller : CSceneDiInstaller
    {
        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public override void InstallBindings()
        {
            InstallCoreLogic();
            base.InstallBindings();
        }

        private void InstallCoreLogic()
        {
            Container.AddSingleton<CAldaInstantiator>();
            Container.AddSingleton<CServerConnectionThread>();
            Container.AddSingleton<CServiceConnectionThread>();
            Container.AddSingleton<CValidServerConnectionPostprocess>();
        }
    }
}