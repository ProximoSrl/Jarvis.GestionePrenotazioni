using System;
using GestionePrenotazioni.Domain.Aggregates.Attrezzatura;
using GestionePrenotazioni.Domain.Aggregates.Attrezzatura.Events;
using Jarvis.Framework.TestHelpers;
using Machine.Specifications;

// ReSharper disable InconsistentNaming
namespace GestionePrenotazioni.Tests
{


    [Subject("Attrezzatura")]
    public class AttrezzaturaSpecs : AggregateSpecification<Attrezzatura, AttrezzaturaState>
    {
        [Serializable]
        public class Builder : AttrezzaturaState
        {
            public Builder Nuova()
            {
                return this;
            }

            public Builder Registrata()
            {
                base.Censita = true;
                return this;
            }
        }

        protected static readonly AttrezzaturaId IdAttrezzatura = new AttrezzaturaId(1);
        protected static readonly string Nome = "Notebook 13\"";
        protected static Attrezzatura Attrezzatura => Aggregate;
        protected static AttrezzaturaState Stato => State;

        protected static Builder Configura()
        {
            var builder = new Builder();
            SetUp(builder, IdAttrezzatura);
            return builder;
        }
    }

    public class quando_viene_registrata_un_attrezzatura : AttrezzaturaSpecs
    {
        Establish context = () =>
        {
            Configura().Nuova();
        };

        Because of = () =>
        {
            Attrezzatura.Registra(Nome);
        };

        //
        // controllo delle transizioni di stato
        //
        It lo_stato_deve_essere_corretto = () =>
        {
            Stato.Censita.ShouldBeTrue();
        };

        //
        // controllo degli eventi
        //
        It deve_essere_generato_levento_di_registrazione = () =>
        {
            EventHasBeenRaised<AttrezzaturaRegistrata>().ShouldBeTrue();
        };

        It l_evento_di_registrazione_contiene_il_nome = () =>
        {
            var e = RaisedEvent<AttrezzaturaRegistrata>();
            e.Nome.ShouldEqual(Nome);
        };
    }
}
