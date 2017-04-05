using System;

namespace GestionePrenotazioni.Domain.Bus.Messages
{
    public class SampleMessage : BaseMessage
    {
        public SampleMessage(String message)
        {
            Message = message;
        }

        public String Message { get; }
    }
}
