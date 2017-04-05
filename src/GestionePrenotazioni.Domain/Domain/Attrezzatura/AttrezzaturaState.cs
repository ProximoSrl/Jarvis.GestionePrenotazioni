using GestionePrenotazioni.Domain.Domain.Attrezzatura.Events;
using Jarvis.Framework.Kernel.Engine;
using System;

namespace GestionePrenotazioni.Domain.Domain.Attrezzatura
{
    [Serializable]
    public class AttrezzaturaState : AggregateState
    {
        internal string Nome { get; private set; }
        internal Boolean Created { get; private set; }

        private void When(AttrezzaturaRegistrata evt)
        {
            Nome = evt.Nome;
            Created = true;
        }
    }
}
