using Jarvis.Framework.Shared.IdentitySupport;

namespace GestionePrenotazioni.Domain.Aggregates.Attrezzatura
{
    public class AttrezzaturaId : EventStoreIdentity
    {
        public AttrezzaturaId(long id) : base(id) { }
        public AttrezzaturaId(string id) : base(id) { }
    }
}
