using GestionePrenotazioni.Client.Controllers.Models;
using GestionePrenotazioni.Host.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionePrenotazioni.Host.Controllers.Swagger
{
    public class RegistraAttrezzaturaModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new RegistraAttrezzaturaModel { Nome = "PC 1"};
        }
    }

    public class SearchAttrezzaturaExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new List<AttrezzaturaModel>
            {
                new AttrezzaturaModel {Id = "Attrezzatura_1", Nome = "PC 1"},
                new AttrezzaturaModel {Id = "Attrezzatura_2", Nome = "Monitor 2"},
            };
        }
    }
}
