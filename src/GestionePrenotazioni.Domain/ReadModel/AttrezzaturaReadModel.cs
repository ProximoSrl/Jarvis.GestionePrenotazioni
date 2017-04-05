using Jarvis.Framework.Shared.ReadModel;
using System;
using System.Collections.Generic;

namespace GestionePrenotazioni.Domain.ReadModel
{
    public class AttrezzaturaReadModel : AbstractReadModel<String>
    {
        public string Nome { get; set; }
    }
}
