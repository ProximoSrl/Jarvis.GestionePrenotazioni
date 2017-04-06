using System;
using GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Events;
using Jarvis.Framework.Kernel.Engine;

namespace GestionePrenotazioni.Domain.Aggregates.Attrezzatura
{
    [Serializable]
    public class AttrezzaturaState : AggregateState
    {
        public Boolean Censita { get; protected set; }

        private void When(AttrezzaturaRegistrata evt)
        {
            Censita = true;
        }
    }
}
