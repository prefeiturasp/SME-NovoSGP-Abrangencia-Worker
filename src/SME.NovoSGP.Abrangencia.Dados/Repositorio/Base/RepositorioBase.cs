using Dapper;
using Dommel;
using Npgsql;
using SME.NovoSGP.Abrangencia.Dados.Interfaces;
using SME.NovoSGP.Abrangencia.Dominio;
using SME.NovoSGP.Abrangencia.Dominio.Entidades;
using SME.NovoSGP.Abrangencia.Dominio.Extensoes;
using SME.NovoSGP.Abrangencia.Infra.EnvironmentVariables;
using SME.NovoSGP.Abrangencia.Infra.Interfaces;
using System.Data;

namespace SME.NovoSGP.Abrangencia.Dados.Repositorio.Base;

public abstract class RepositorioBase
{
    private readonly IContextoAplicacao contextoAplicacao;

    protected RepositorioBase(IContextoAplicacao contextoAplicacao)
    {
        this.contextoAplicacao = contextoAplicacao;
    }

    protected string UsuarioLogado => contextoAplicacao.UsuarioLogado;

    protected string UsuarioLogadoNomeCompleto => contextoAplicacao.NomeUsuario;

    protected string PerfilUsuario => contextoAplicacao.PerfilUsuario;

    protected string UsuarioLogadoRF => contextoAplicacao.ObterVariavel<string>("RF") ?? "0";

    protected string Administrador => contextoAplicacao.Administrador;

}
