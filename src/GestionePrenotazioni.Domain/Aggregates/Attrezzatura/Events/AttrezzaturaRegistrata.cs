using System;
using Jarvis.Framework.Shared.Events;

namespace GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Events
{
    public class AttrezzaturaRegistrata : DomainEvent
    {
        public AttrezzaturaRegistrata(string nome)
        {
            if (String.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Il nome non può essere nullo", nameof(nome));

            Nome = nome;
        }

        public String Nome { get; private set; }
    }
}
