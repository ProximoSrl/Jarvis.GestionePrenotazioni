using GestionePrenotazioni.Domain.Bus.Messages;
using Rebus;

namespace GestionePrenotazioni.Domain.Bus.Handlers
{
    public class SampleMessageMessageHandler : IHandleMessages<SampleMessage>
    {
        public void Handle(SampleMessage message)
        {
            //Do your logic here.
        }
    }
}