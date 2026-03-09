// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public abstract class CPharaohScreen : CBaseScreen<CPharaohScreen>, IUiScreen
    {
        private CEscapeHandler _escapeHandler;
        
        private CancellationTokenSource _cts;
        
        protected ICtsProvider CtsProvider;
        
        public bool IsOpened { get; private set; }
        
        [Inject]
        private void Inject(CEscapeHandler escapeHandler, ICtsProvider ctsProvider)
        {
            _escapeHandler = escapeHandler;
            CtsProvider = ctsProvider;
        }
        
        private void OnValidate()
        {
            this.ValidateRefs();
        }

        public void OnEscape()
        {
            bool canBeClosedByEscape = CanBeClosedByEscape();
            if (!canBeClosedByEscape)
            {
                ProcessDeniedEscapeTrigger();
                return;
            }
            CloseThisScreen();
        }
        
        public void TryCloseThisScreen()
        {
            bool canBeClosedByEscape = CanBeClosedByEscape();
            if (!canBeClosedByEscape)
            {
                ProcessDeniedEscapeTrigger();
                return;
            }
            CloseThisScreen();
        }

        public override void OnScreenOpenStart()
        {
            base.OnScreenOpenStart();
            EventBus.Send(new CScreenOpenedSignal(this));
            _escapeHandler.RegisterEscapable(this);
            
            IsOpened = true;
        }

        public override void OnScreenCloseEnd()
        {
            base.OnScreenCloseEnd();
            EventBus.Send(new CScreenClosedSignal(this));
            _escapeHandler.UnregisterEscapable(this);
            
            IsOpened = false;
        }
    }
}