// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Collections.Generic;

namespace ServerData
{
    public class CStoryContractBuilder : CContractBuilder<CStoryContractBuilder>
    {
        private readonly EStaticContractId _id;
        private EMovementType _overrideMovementType;
        
        private EDialogueId _storyDialogueId = EDialogueId.None;
        private EDialogueId _dispatchDialogueId = EDialogueId.None;

        public CStoryContractBuilder(EStaticContractId id)
        {
            _id = id;
        }
        
        public CStoryContractBuilder SetStoryDialogue(EDialogueId dialogueId)
        {
            _storyDialogueId = dialogueId;
            return this;
        }
        
        public CStoryContractBuilder SetDispatchDialogue(EDialogueId dialogueId)
        {
            _dispatchDialogueId = dialogueId;
            return this;
        }

        public CStoryContractBuilder AddBlockingContract(EStaticContractId contract)
        {
            UnlockRequirements.Add(IUnlockRequirement.Contract(contract));
            return this;
        }
        
        public CStoryContractBuilder AddContractFullyLoadedRequirement(EStaticContractId contract)
        {
            UnlockRequirements.Add(IUnlockRequirement.ContractFullyLoaded(contract));
            return this;
        }

        public CStoryContractBuilder SetMovementType(EMovementType movementType)
        {
            _overrideMovementType = movementType;
            return this;
        }

        public CStoryContractConfig Build()
        {
            CContractTask[] tasks = BuildTasks();
            CStoryContractConfig config = new(
                Region, 
                UnlockRequirements,  
                Customer, 
                tasks, 
                _id,
                TripPrice.Build(),
                _storyDialogueId,
                _dispatchDialogueId,
                _overrideMovementType
                );
            return config;
        }
    }
}