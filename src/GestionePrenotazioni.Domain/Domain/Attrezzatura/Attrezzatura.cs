using GestionePrenotazioni.Domain.Domain.Attrezzatura.Events;
using Jarvis.Framework.Kernel.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.Domain.Attrezzatura
{
    public class Attrezzatura : AggregateRoot<AttrezzaturaState>
    {
        public void Registra(String nome)
        {
            if (String.IsNullOrEmpty(nome))
                ThrowDomainException("Il nome di una attrezzatura non può essere nullo.");

            if (InternalState.Created)
            {
                if (InternalState.Nome != nome)
                    ThrowDomainException("Attrezzatura già registrata con nome differente");

                return; //idempotency
            }

            RaiseEvent(new AttrezzaturaRegistrata(nome));
        }
    }
}
