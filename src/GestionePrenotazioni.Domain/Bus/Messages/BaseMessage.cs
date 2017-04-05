using System;
using Jarvis.Framework.Shared.Messages;

namespace GestionePrenotazioni.Domain.Bus.Messages
{
    public abstract class BaseMessage
    {
        protected BaseMessage()
        {
            MessageId = Guid.NewGuid();
        }

        public Guid MessageId { get; }
    }
}