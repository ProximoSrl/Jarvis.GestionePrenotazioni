using Jarvis.Framework.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.Domain.Attrezzatura.Events
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
