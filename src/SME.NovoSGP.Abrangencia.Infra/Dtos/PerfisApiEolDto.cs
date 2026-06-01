namespace SME.NovoSGP.Abrangencia.Infra.Dtos
{
    public class PerfisApiEolDto
    {
        public string CodigoRf { get; set; }
        public IEnumerable<Guid> Perfis { get; set; }
        public bool PossuiCargoCJ { get; set; }
        public bool PossuiPerfilCJ { get; set; }
        public bool ContratoExterno { get; set; }
    }
}
