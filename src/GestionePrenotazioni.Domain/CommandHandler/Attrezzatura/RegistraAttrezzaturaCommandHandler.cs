using GestionePrenotazioni.Domain.Domain.Attrezzatura.Commands;
using Jarvis.Framework.Kernel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Domain.CommandHandler.Attrezzatura
{
    public class RegistraAttrezzaturaCommandHandler : RepositoryCommandHandler<Domain.Attrezzatura.Attrezzatura, RegistraAttrezzatura>
    {
        protected override void Execute(RegistraAttrezzatura cmd)
        {
            FindAndModify(cmd.AggregateId, a => a.Registra(cmd.Nome), true);
        }
    }
}
