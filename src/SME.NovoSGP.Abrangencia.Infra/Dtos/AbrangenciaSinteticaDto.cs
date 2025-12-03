using SME.NovoSGP.Abrangencia.Dominio.Constantes;

namespace SME.NovoSGP.Abrangencia.Infra.Dtos;

public class AbrangenciaSinteticaDto
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public string Login { get; set; }
    public long DreId { get; set; }
    public string CodigoDre { get; set; }
    public long UeId { get; set; }
    public string CodigoUe { get; set; }
    public long TurmaId { get; set; }
    public string CodigoTurma { get; set; }
    public bool Historico { get; set; }
    public Guid Perfil { get; set; }

    public bool EhPerfilProfessor()
      => EhProfessor()
      || EhProfessorCj()
      || EhProfessorInfantil()
      || EhProfessorCjInfantil()
      || EhProfessorPoa()
      || EhProfessorPaee()
      || EhProfessorPap()
      || EhProfessorPoei()
      || EhProfessorPoed()
      || EhProfessorPosl();

    public bool EhProfessorPaee()
        => Perfil == Perfis.PERFIL_PAEE;

    public bool EhProfessorPap()
        => Perfil == Perfis.PERFIL_PAP;

    public bool EhProfessorPoei()
        => Perfil == Perfis.PERFIL_POEI;

    public bool EhProfessorPoed()
        => Perfil == Perfis.PERFIL_POED;

    public bool EhProfessorPosl()
        => Perfil == Perfis.PERFIL_POSL;

    public bool EhProfessor()
    {
        return Perfil == Perfis.PERFIL_PROFESSOR
            || Perfil == Perfis.PERFIL_PROFESSOR_INFANTIL;
    }

    public bool EhProfessorCj()
    {
        return Perfil == Perfis.PERFIL_CJ
            || Perfil == Perfis.PERFIL_CJ_INFANTIL;
    }


    public bool EhProfessorPoa()
        => Perfis.EhPerfilPOA(Perfil);

    public bool EhProfessorInfantil()
    {
        return Perfil == Perfis.PERFIL_PROFESSOR_INFANTIL;
    }

    public bool EhProfessorCjInfantil()
    {
        return Perfil == Perfis.PERFIL_CJ_INFANTIL;
    }
}
