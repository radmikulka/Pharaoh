// =========================================
// AUTHOR: Juraj Joscak
// DATE:   18.12.2025
// =========================================

using System.Threading;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.Infrastructure
{
	public class CBuildingRewardHandler : BaseRewardHandler, IAldaFrameworkComponent
	{
		private readonly CUser _user;

		public CBuildingRewardHandler(CUser user)
		{
			_user = user;
		}

		public void Claim(CBuildingValuable building, CValueModifyParams modifyParams)
		{
			_user.OwnedValuables.ModifyValuableInternal(building, modifyParams);
		}
	}
}