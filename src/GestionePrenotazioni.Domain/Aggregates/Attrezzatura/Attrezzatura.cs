using System;
using GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Events;
using Jarvis.Framework.Kernel.Engine;

namespace GestionePrenotazioni.Domain.Aggregates.Attrezzatura
{
    public class Attrezzatura : AggregateRoot<AttrezzaturaState>
    {
        public void Registra(String nome)
        {
            if (String.IsNullOrWhiteSpace(nome))
            {
                ThrowDomainException("Il nome di una attrezzatura non può essere nullo.");
            }

            if (InternalState.Censita)
            {
                ThrowDomainException("Attrezzatura già registrata con nome differente");
            }

            RaiseEvent(new AttrezzaturaRegistrata(nome));
        }
    }
}
