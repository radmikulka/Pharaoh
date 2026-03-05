// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.09.2025
// =========================================

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AldaEngine;
using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder.Configs.Design
{
    [CreateAssetMenu(fileName = "DialogueValidation", menuName = "____TycoonBuilder/EditorTasks/Steps/DialogueValidation")]
    internal class CEditorTaskDialogueValidation : CBaseEditorTaskStep
    {
        public override Task Execute()
        {
            Validate();
            return Task.CompletedTask;
        }

        private void Validate()
        {
            CEditorTaskDialogueValidationForRegions regionsValidator = new ();
            regionsValidator.ValidateRegions();
            
            Debug.Log("Validation Completed!");
        }
    }
}
#endif