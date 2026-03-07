// =========================================
// AUTHOR: Radek Mikulka
// DATE:   2023-09-08
// =========================================

using System.Text;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.Tcp;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CUiSessionPanel : MonoBehaviour, IInitializable
	{
		private const string LastKnownShortUserIdKey = "lkuid";
		
		[SerializeField] private CUiComponentText _text;

		private CServerEndpointProvider _endPointProvider;
		private CUser _user;

		[Inject]
		private void Inject(CUser user, CServerEndpointProvider endpointProvider)
		{
			_endPointProvider = endpointProvider;
			_user = user;
		}

		public void Initialize()
		{
			CacheUserId();
			Repaint();
		}

		private void CacheUserId()
		{
			if(!_user.IsValid)
				return;
			CPlayerPrefs.Set(LastKnownShortUserIdKey, _user.Account.PublicId);
		}

		private string GetServerType()
		{
			switch (_endPointProvider.ActiveEndPoint.ServerType)
			{
				case EServerType.Develop:
					return "d";
				case EServerType.Master:
					return "m";
				case EServerType.Approval:
					return "a";
				default: return string.Empty;
			}
		}

		private void Repaint()
		{
			StringBuilder sb = new();
			
			string version = CConfigVersion.Instance.FullVersion;
			string serverType = GetServerType();
			string shortUserId = CPlayerPrefs.Get(LastKnownShortUserIdKey, string.Empty);

			sb.Append(version);
			if (!serverType.IsNullOrEmpty())
			{
				sb.Append(" ");
				sb.Append(serverType);
			}
			if (!shortUserId.IsNullOrEmpty())
			{
				sb.Append(" ");
				sb.Append(shortUserId);
			}
			
			_text.SetValue(sb.ToString());
		}
	}
}