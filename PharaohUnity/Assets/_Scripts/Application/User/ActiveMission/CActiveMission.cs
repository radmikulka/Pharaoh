using ServerData;

namespace Pharaoh
{
    public class CActiveMission : CBaseUserComponent
    {
        public EMissionId Mission { get; private set; }

        public void InitialSync(CActiveMissionDto dto)
        {
            
        }
    }
}