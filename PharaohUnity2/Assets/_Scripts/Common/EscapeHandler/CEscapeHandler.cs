// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
    public class CEscapeHandler : IInitializable
    {
        private readonly List<IEscapable> _activeEscapables = new();
        private readonly IEventBus _eventBus;

        public CEscapeHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<CEscapePressedSignal>(OnEscapePressed);
        }
        
        public void RegisterEscapable(IEscapable escapable)
        {
            _activeEscapables.Add(escapable);
        }
        
        public void UnregisterEscapable(IEscapable escapable)
        {
            _activeEscapables.Remove(escapable);
        }

        private void OnEscapePressed(CEscapePressedSignal signal)
        {
            if(_activeEscapables.Count == 0)
                return;
            _activeEscapables.Last().OnEscape();
        }
    }
}