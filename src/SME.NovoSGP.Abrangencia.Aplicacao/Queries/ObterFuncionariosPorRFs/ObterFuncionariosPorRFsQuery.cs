using FluentValidation;
using MediatR;
using SME.NovoSGP.Abrangencia.Infra.Dtos;

namespace SME.NovoSGP.Abrangencia.Aplicacao.Queries.ObterFuncionariosPorRFs;

public class ObterFuncionariosPorRFsQuery : IRequest<IEnumerable<ProfessorResumoDto>>
{
    public IEnumerable<string> CodigosRf { get; set; }

    public ObterFuncionariosPorRFsQuery(IEnumerable<string> codigosRf)
    {
        CodigosRf = codigosRf;
    }
}

public class ObterListaNomePorListaRFQueryValidator : AbstractValidator<ObterFuncionariosPorRFsQuery>
{
    public ObterListaNomePorListaRFQueryValidator()
    {
        RuleForEach(x => x.CodigosRf)
            .NotEmpty()
            .WithMessage("O código do usuário deve ser informado.");
    }
}