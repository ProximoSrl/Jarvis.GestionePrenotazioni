using Jarvis.Framework.Kernel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Commands;

namespace GestionePrenotazioni.Domain.CommandHandler.Attrezzatura
{
    public class RegistraAttrezzaturaCommandHandler :
        RepositoryCommandHandler<Aggregates.Attrezzatura.Attrezzatura, RegistraAttrezzatura>
    {
        protected override void Execute(RegistraAttrezzatura cmd)
        {
            FindAndModify(cmd.AggregateId, a =>
                {
                    if (!a.HasBeenCreated)
                        a.Registra(cmd.Nome);
                },
                createIfNotExists: true
            );
        }
    }
}
