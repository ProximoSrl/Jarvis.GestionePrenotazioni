using Jarvis.Framework.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.Domain.Attrezzatura.Commands
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
