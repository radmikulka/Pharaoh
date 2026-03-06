// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using JetBrains.Annotations;
using ServerData;

namespace Pharaoh
{
	public abstract class COwnedValuable
	{
		public readonly EValuable Id;

		protected COwnedValuable(EValuable id)
		{
			Id = id;
		}

		public virtual void InitialSync(COwnedValuableData data)
		{
			
		}

		public virtual void Sync(IOwnedValuableData data)
		{
			
		}

		internal abstract void Modify(IValuable valuable, [CanBeNull] CValueModifyParams modifyParams);

		public abstract bool HaveValuable(IValuable valuable);

		public virtual void Dispose()
		{
			
		}
	}
}