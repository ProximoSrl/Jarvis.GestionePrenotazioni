using Castle.MicroKernel;
using Jarvis.Framework.Kernel.Commands;
using Jarvis.Framework.Shared.Commands;
using Jarvis.Framework.Shared.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.Support
{
    public class GestionePrenotazioniInProcessCommandBus : InProcessCommandBus
    {
        public GestionePrenotazioniInProcessCommandBus(IMessagesTracker messagesTracker)
            : base(messagesTracker)
        {
        }

        public GestionePrenotazioniInProcessCommandBus(IKernel kernel, IMessagesTracker messagesTracker)
            : base(kernel, messagesTracker)
        {
        }

        protected override bool UserIsAllowedToSendCommand(ICommand command, string userName)
        {
            return true;
        }
    }
}