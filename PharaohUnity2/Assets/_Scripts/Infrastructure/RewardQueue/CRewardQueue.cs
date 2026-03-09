using System.Linq;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using Zenject;

namespace Pharaoh
{
    public class CRewardQueue : IRewardQueue
    {
        private IEventBus _eventBus;
        private CUser     _user;

        [Inject]
        private void Inject(IEventBus eventBus, CUser user)
        {
            _eventBus = eventBus;
            _user     = user;
        }

        public void EnqueueReward(IValuable reward, IRewardParams[] data)
        {
            switch (reward.Id)
            {
                case EValuable.HardCurrency:
                    ProcessHardCurrency(reward, data);
                    break;

                case EValuable.SoftCurrency:
                    ProcessSoftCurrency(reward);
                    break;
            }
        }

        private void ProcessHardCurrency(IValuable reward, IRewardParams[] data)
        {
            CParticleSourceRewardParams particleSource = data?
                .OfType<CParticleSourceRewardParams>()
                .FirstOrDefault()

            if (particleSource != null)
            {
                CValueModifyParams modifyParams = new CValueModifyParams().Add(new CAnimatedChangeParam());
                _user.OwnedValuables.ModifyValuableInternal(reward, modifyParams);

                ICountableValuable countable = (ICountableValuable)reward;
                CConsumableValuable consumable = CValuableFactory.Consumable(reward.Id, countable.Value);
                _eventBus.ProcessTaskAsync(
                    new CRunWorldConsumableParticleTask(particleSource.WorldPosition, consumable),
                    CancellationToken.None
                ).Forget();
            }
            else
            {
                _user.OwnedValuables.ModifyValuableInternal(reward);
            };
        }

        private void ProcessSoftCurrency(IValuable reward)
        {
            ICountableValuable countable = (ICountableValuable)reward;
            _eventBus.ProcessTask(new CAddSoftCurrencyTask(countable.Value));
        }
    }
}
