using MediatR;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterPerfisPorLogin
{
    public class ObterPerfisPorLoginQuery : IRequest<IEnumerable<Guid>>
    {
        public ObterPerfisPorLoginQuery(string login)
        {
            Login = login;
        }

        public string Login { get; set; }
    }
}
