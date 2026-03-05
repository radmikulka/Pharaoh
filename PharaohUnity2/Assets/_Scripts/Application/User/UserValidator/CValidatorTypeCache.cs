// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TycoonBuilder
{
	public class CValidatorTypeCache
	{
		private readonly Dictionary<Type, bool> _validatableTypesCache = new();
		private readonly Dictionary<Type, List<MemberInfo>> _validatableMembers = new();
		
		public bool IsValidatable(Type type)
		{
			if (_validatableTypesCache.TryGetValue(type, out bool isValidatable))
				return isValidatable;

			isValidatable = type.GetCustomAttribute<ValidatableDataAttribute>() != null;
			_validatableTypesCache.Add(type, isValidatable);
			return isValidatable;
		}
		
		public IReadOnlyList<MemberInfo> GetValidatableMembers(Type type)
		{
			if (_validatableMembers.TryGetValue(type, out List<MemberInfo> members))
				return members;

			members = new();
			Type currentType = type;
			do
			{
				MemberInfo[] candidateMembers = currentType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
				MemberInfo[] validatableMembers = candidateMembers.Where(info => CustomAttributeExtensions.GetCustomAttribute<ValidatableDataAttribute>((MemberInfo)info) != null).ToArray();
				members.AddRange(validatableMembers);
				currentType = currentType.BaseType;
			} while (currentType != null && currentType != typeof(object));

			_validatableMembers.Add(type, members);
			return members;
		}
	}
}