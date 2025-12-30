using Microsoft.Extensions.Logging;
using SME.NovoSGP.Abrangencia.Dominio.Constantes;
using SME.NovoSGP.Abrangencia.Dominio.Excecoes;
using System.Text.RegularExpressions;

namespace SME.NovoSGP.Abrangencia.Dominio.Entidades
{
    public class Usuario : EntidadeBase
    {
        public string CodigoRf { get; set; }

        public DateTime? ExpiracaoRecuperacaoSenha { get; set; }

        public string Login { get; set; }

        public string Nome { get; set; }

        public Guid PerfilAtual { get; set; }

        public Guid? TokenRecuperacaoSenha { get; set; }

        public DateTime UltimoLogin { get; set; }

        private string Email { get; set; }
    }
}
