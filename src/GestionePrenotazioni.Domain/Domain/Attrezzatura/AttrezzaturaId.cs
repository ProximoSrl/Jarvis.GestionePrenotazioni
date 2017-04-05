using Jarvis.Framework.Shared.IdentitySupport;

namespace GestionePrenotazioni.Domain.Domain.Attrezzatura
{
    public class AttrezzaturaId : EventStoreIdentity
    {
        public AttrezzaturaId(long id) : base(id) { }
        public AttrezzaturaId(string id) : base(id) { }
    }
}
