// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using UnityEngine;

namespace Pharaoh
{
    public interface IMainCameraProvider
    {
        IMainCamera Camera { get; }
        
        void SetMainCamera(IMainCamera camera);
    }
}