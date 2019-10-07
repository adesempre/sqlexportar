using System;

namespace SqlExporterCore.regras
{
    public class ProcedimentoTipoSql : ITipoObjetoSql
    {
        public static readonly string PROCEDURE_SQL = "procedure";
        private static readonly string SELECT = @"select s.[name], o.[name], o.[modify_date]
from [sys].[objects] o
join [sys].[schemas] s
on o.[schema_id] = s.[schema_id]
where o.[type] = 'P'
order by s.[name], o.[name]";
        private static readonly string SCRIPT = "sp_helptext '{0}.{1}'";

        public string Nome()
        {
            return PROCEDURE_SQL;
        }

        public string SqlListarObjetos(BancoDadosEnum banco)
        {
            if (banco == BancoDadosEnum.SQL_SERVER)
                return SELECT;

            throw new NotImplementedException($"Não existe implementação para o banco de dados informado: {banco}");
        }

        public string SqlRecuperarScriptCriacao(BancoDadosEnum banco, string proprietario, string nomeObjeto)
        {
            if (banco == BancoDadosEnum.SQL_SERVER)
                return string.Format(SCRIPT, proprietario, nomeObjeto);

            throw new NotImplementedException($"Não existe implementação para o banco de dados informado: {banco}");
        }

        public override string ToString()
        {
            return $"Tipo de objeto: {PROCEDURE_SQL}";
        }
    }
}
