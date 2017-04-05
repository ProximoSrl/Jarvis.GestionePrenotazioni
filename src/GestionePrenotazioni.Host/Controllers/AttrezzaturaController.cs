using Jarvis.Framework.Kernel.Commands;
using Jarvis.Framework.Shared.ReadModel;
using Swashbuckle.Swagger.Annotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rebus;
using GestionePrenotazioni.Domain.ReadModel;
using GestionePrenotazioni.Client.Controllers.Models;
using GestionePrenotazioni.Domain.Domain.Attrezzatura;
using GestionePrenotazioni.Domain.Domain.Attrezzatura.Commands;
using Jarvis.Framework.Shared.IdentitySupport;
using GestionePrenotazioni.Host.Support;
using System.Collections.Generic;
using GestionePrenotazioni.Host.Controllers.Swagger;

namespace GestionePrenotazioni.Host.Controllers
{
    /// <summary>
    /// Controller di gestione degli account
    /// </summary>
    public class AttrezzaturaController : ApiController
    {
        private string username => User.Identity.Name;

        /// <summary>
        /// Constructor of attrezzatura controller
        /// </summary>
        /// <param name="inProcessBus"></param>
        /// <param name="identityManager"></param>
        /// <param name="reader"></param>
        public AttrezzaturaController(
            IInProcessCommandBus inProcessBus,
            IIdentityManager identityManager,
            IReader<AttrezzaturaReadModel, string> reader)
        {
            _inProcessBus = inProcessBus;
            _reader = reader;
            _identityManager = identityManager;
        }

        private readonly IInProcessCommandBus _inProcessBus;
        private readonly IReader<AttrezzaturaReadModel, string> _reader;
        private readonly IIdentityManager _identityManager;

        /// <summary>
        /// Registra una attrezzatura
        /// </summary>
        /// <param name="dto">Codice identificativo dell'account</param>
        [HttpPost]
        [Route("attrezzatura/registra")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerRequestExamples(typeof(RegistraAttrezzaturaModel), typeof(RegistraAttrezzaturaModelExample))]
        public HttpResponseMessage Attiva(RegistraAttrezzaturaModel dto)
        {
            var attrezzaturaId = _identityManager.New<AttrezzaturaId>();
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Non è possibile creare una attrezzatura con nome nullo");

            var command = new RegistraAttrezzatura(attrezzaturaId, dto.Nome);
            _inProcessBus.Send(command);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("attrezzatura/search")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable< AttrezzaturaModel>))]
        [SwaggerResponseExamples(typeof(IEnumerable<AttrezzaturaModel>), typeof(SearchAttrezzaturaExample))]
        public HttpResponseMessage Search()
        {
            var retValue = _reader
                .AllUnsorted
                .Select(a => new AttrezzaturaModel()
                {
                    Id = a.Id,
                    Nome = a.Nome
                })
                .OrderBy(a => a.Nome);
            return Request.CreateResponse(HttpStatusCode.OK, retValue);
        }
    }
}
