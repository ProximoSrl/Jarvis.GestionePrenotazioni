using System;
using Jarvis.Framework.Shared.Commands;

namespace GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Commands
{
    public class RegistraAttrezzatura : Command<AttrezzaturaId>
    {
        public RegistraAttrezzatura(AttrezzaturaId id, string nome) : base(id)
        {
            if (String.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Il nome non può essere nullo", nameof(nome));

            Nome = nome;
        }

        public String Nome { get; private set; }
    }
}
