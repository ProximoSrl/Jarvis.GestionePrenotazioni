using GestionePrenotazioni.Domain.ReadModel;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.ProjectionEngine;
using MongoDB.Driver;
using System;
using GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Events;

namespace GestionePrenotazioni.Domain.EventHandlers
{
    public class AttrezzaturaProjection : AbstractProjection,
        IEventHandler<AttrezzaturaRegistrata>
    {
        private readonly ICollectionWrapper<AttrezzaturaReadModel, String> _attrezzatura;

        public AttrezzaturaProjection(
            ICollectionWrapper<AttrezzaturaReadModel, String> attrezzatura)
        {
            _attrezzatura = attrezzatura;
            _attrezzatura.Attach(this, true);
        }

        public override int Priority
        {
            get { return 10; }
        }

        public override void Drop()
        {
            _attrezzatura.Drop();
        }

        public override void SetUp()
        {
            _attrezzatura.CreateIndex("Nome",
                Builders<AttrezzaturaReadModel>.IndexKeys.Ascending(r => r.Nome));
        }

        public void On(AttrezzaturaRegistrata e)
        {
            _attrezzatura.Insert(e, new AttrezzaturaReadModel()
            {
                Id = e.AggregateId.AsString(),
                Nome = e.Nome,
            });
        }
    }
}
