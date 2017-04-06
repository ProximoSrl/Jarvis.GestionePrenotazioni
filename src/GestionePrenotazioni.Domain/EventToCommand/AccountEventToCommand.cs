using GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Events;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.Commands;
using Rebus;

namespace GestionePrenotazioni.Domain.EventToCommand
{
    public class AccountEventToCommand : AbstractProjection,
        IEventHandler<AttrezzaturaRegistrata>
    {
        public AccountEventToCommand(IInProcessCommandBus inProcessBus)
        {
            _inProcessBus = inProcessBus;
        }

        private readonly IInProcessCommandBus _inProcessBus;

        public override void Drop()
        {
        }

        public override void SetUp()
        {
        }

        public void On(AttrezzaturaRegistrata e)
        {
            //Avoid reapplying logic during rebuild.
            if (IsReplay)
                return;

            //Launch whatever command you want
        }
    }
}
