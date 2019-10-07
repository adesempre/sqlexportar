namespace SqlExporterCore.regras
{
    public class SqlTiposFabrica
    {
        public ITipoObjetoSql[] ListarTipos()
        {
            return new ITipoObjetoSql[]
            {
                new ProcedimentoTipoSql(),
                new VisaoTipoSql(),
                new TabelaTipoSql()
            };
        }
    }
}
