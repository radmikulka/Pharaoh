// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.08.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using ServerData.Dto;

namespace TycoonBuilder
{
	public class CGlobalVariables : CBaseUserComponent
	{
		private readonly Dictionary<EGlobalVariable, CGlobalVariable> _variables = new();

		private readonly IMapper _mapper;

		public CGlobalVariables(IMapper mapper)
		{
			_mapper = mapper;
		}

		public void InitialSync(CGlobalVariablesDto dto)
		{
			foreach (CGlobalVariableDto variable in dto.GlobalVariables)
			{
				CGlobalVariable newVariable = _mapper.Map<CGlobalVariableDto, CGlobalVariable>(variable);
				_variables.Add(newVariable.Id, newVariable);
			}
		}
		
		public CGlobalVariable GetOrCreate(EGlobalVariable id)
		{
			if(_variables.TryGetValue(id, out var variable))
			{
				return variable;
			}

			CGlobalVariable newVariable = new(id);
			_variables.Add(id, newVariable);
			return newVariable;
		}

		public void SetVariable(EGlobalVariable id, string value)
		{
			CGlobalVariable variable = GetOrCreate(id);
			variable.SetValue(value);
		}
		
		public void SetVariable(EGlobalVariable id, bool value)
		{
			CGlobalVariable variable = GetOrCreate(id);
			variable.SetValue(value);
		}
	}
}