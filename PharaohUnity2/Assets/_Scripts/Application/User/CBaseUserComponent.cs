// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace Pharaoh
{
    public class CBaseUserComponent
    {
        protected CUser User;

        public virtual void Initialize(CUser user)
        {
            User = user;
        }

        public virtual void Dispose()
        {
            
        }
    }
}