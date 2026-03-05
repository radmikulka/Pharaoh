// =========================================
// AUTHOR: Marek Karaba
// DATE:   08.08.2025
// =========================================

using System.Globalization;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class CGlobalVariablesHandler : IInitializable
    {
        private readonly CHitBuilder _hitBuilder;
        private readonly IEventBus _eventBus;
        private readonly CUser _user;

        public CGlobalVariablesHandler(CHitBuilder hitBuilder, IEventBus eventBus, CUser user)
        {
            _hitBuilder = hitBuilder;
            _eventBus = eventBus;
            _user = user;
        }
        
        public void Initialize()
        {
            _eventBus.Subscribe<CIntroFinishedSignal>(OnIntroFinished);
        }
        
        private void OnIntroFinished(CIntroFinishedSignal signal)
        {
            SaveGlobalVariableBool(EGlobalVariable.IntroSeen, true);
        }

        public string GetGlobalVariableString(EGlobalVariable variable)
        {
            string stringValue = _user.GlobalVariables.GetOrCreate(variable).StringValue;
            return stringValue;
        }

        public void SaveGlobalVariableString(EGlobalVariable variable, string value)
        {
            bool alreadySaved = _user.GlobalVariables.GetOrCreate(variable).StringValue == value;
            if (alreadySaved)
                return;
			
            SaveGlobalVariable(variable, value);
        }

        public float GetGlobalVariableFloat(EGlobalVariable variable)
        {
            string stringValue = _user.GlobalVariables.GetOrCreate(variable).StringValue;
            if (stringValue.IsNullOrEmpty())
                return 0;
			
            float value = float.Parse(stringValue, CultureInfo.InvariantCulture);
            return value;
        }

        public void SaveGlobalVariableFloat(EGlobalVariable variable, float value)
        {
            bool alreadySaved = CMath.Approximately(GetGlobalVariableFloat(variable), value);
            if (alreadySaved)
                return;
			
            string stringValue = value.ToString(CultureInfo.InvariantCulture);
            SaveGlobalVariable(variable, stringValue);
        }

        public bool GetGlobalVariableBool(EGlobalVariable variable)
        {
            bool value = _user.GlobalVariables.GetOrCreate(variable).BoolValue;
            return value;
        }

        public void SaveGlobalVariableBool(EGlobalVariable variable, bool value)
        {
            bool alreadySaved = _user.GlobalVariables.GetOrCreate(variable).BoolValue == value;
            if (alreadySaved)
                return;
			
            _user.GlobalVariables.SetVariable(variable, value);
            CGlobalVariableDto globalVariableDto = new (variable, value);
            CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CSetGlobalVariableRequest(globalVariableDto));
            hit.BuildAndSend();
        }

        public long GetGlobalVariableLong(EGlobalVariable variable)
        {
            string stringValue = _user.GlobalVariables.GetOrCreate(variable).StringValue;
            if (stringValue.IsNullOrEmpty())
                return 0;
			
            long value = long.Parse(stringValue);
            return value;
        }

        public void SaveGlobalVariableLong(EGlobalVariable variable, long value)
        {
            bool alreadySaved = GetGlobalVariableLong(variable) == value;
            if (alreadySaved)
                return;
			
            string stringValue = value.ToString();
            SaveGlobalVariable(variable, stringValue);
        }

        public int GetGlobalVariableInt(EGlobalVariable variable)
        {
            string stringValue = _user.GlobalVariables.GetOrCreate(variable).StringValue;
            if (stringValue.IsNullOrEmpty())
                return 0;
			
            int value = int.Parse(stringValue);
            return value;
        }

        public void SaveGlobalVariableInt(EGlobalVariable variable, int value)
        {
            bool alreadySaved = GetGlobalVariableInt(variable) == value;
            if (alreadySaved)
                return;
			
            string stringValue = value.ToString();
            SaveGlobalVariable(variable, stringValue);
        }

        private void SaveGlobalVariable(EGlobalVariable variable, string value)
        {
            bool alreadySaved = _user.GlobalVariables.GetOrCreate(variable).StringValue == value;
            if (alreadySaved)
                return;
			
            _user.GlobalVariables.SetVariable(variable, value);
            CGlobalVariableDto globalVariableDto = new (variable, value);
            CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CSetGlobalVariableRequest(globalVariableDto));
            hit.BuildAndSend();
            SendVariableChangedSignal(variable);
        }

        private void SendVariableChangedSignal(EGlobalVariable variable)
        {
            _eventBus.Send(new CGlobalVariableChangedSignal(variable));
        }
    }
}