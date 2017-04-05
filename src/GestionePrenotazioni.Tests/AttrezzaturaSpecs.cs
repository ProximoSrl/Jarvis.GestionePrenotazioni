using GestionePrenotazioni.Domain.Domain.Attrezzatura;
using GestionePrenotazioni.Domain.Domain.Attrezzatura.Events;
using Jarvis.Framework.TestHelpers;
using Machine.Specifications;
namespace GestionePrenotazioni.Tests
{

    [Subject("Attrezzatura")]
    public class AttrezzaturaSpecs : AggregateSpecification<Attrezzatura, AttrezzaturaState>
    {
        protected static readonly AttrezzaturaId accountId1 = new AttrezzaturaId(1);     
        protected static readonly string nome = "username fornitore";
    }

    public class quando_viene_registrata_un_attrezzatura : AttrezzaturaSpecs
    {
        Establish context = () =>
        {
            Create(accountId1);
        };

        Because of = () =>
        {
            Aggregate.Registra(nome);
        };

        It deve_essere_generato_levento_di_registrazione = () =>
        {
            EventHasBeenRaised<AttrezzaturaRegistrata>().ShouldBeTrue();
        };

        It lo_stato_deve_essere_corretto = () =>
        {
            State.Created.ShouldBeTrue();
            State.Nome.ShouldEqual(nome);
        };

        It l_evento_di_registrazione_è_corretto = () =>
        {
            var e = RaisedEvent<AttrezzaturaRegistrata>();
            e.Nome.ShouldEqual(nome);
        };
    }
}
