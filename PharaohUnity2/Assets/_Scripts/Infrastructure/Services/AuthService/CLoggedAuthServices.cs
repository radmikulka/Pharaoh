// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.11.2023
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace TycoonBuilder
{
	public class CLoggedAuthServices
	{
		private readonly HashSet<EAuthType> _loggedServices = new();

		public readonly CEvent OnAuthChanged = new("LoggedAuthServices");

		public bool Contains(EAuthType authType)
		{
			return _loggedServices.Contains(authType);
		}

		public void Replace(EAuthType[] authTypes)
		{
			_loggedServices.Clear();
			_loggedServices.UnionWith(authTypes);
			OnAuthChanged.Invoke();
		}
	}
}