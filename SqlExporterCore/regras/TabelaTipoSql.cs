using System;

namespace SqlExporterCore.regras
{
    public class TabelaTipoSql : ITipoObjetoSql
    {
        public static readonly string TABELA_SQL = "table";
        private static readonly string SELECT = @"select s.[name], o.[name], o.[modify_date]
from [sys].[objects] o
join [sys].[schemas] s
on o.[schema_id] = s.[schema_id]
where o.[type] = 'U'
order by s.[name], o.[name]";

        public string Nome()
        {
            return TABELA_SQL;
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
                return string.Format(script.AbstractTabelaCreateUtil.SCRIPT, proprietario, nomeObjeto);

            throw new NotImplementedException($"Não existe implementação para o banco de dados informado: {banco}");
        }

        public override string ToString()
        {
            return $"Tipo de objeto: {TABELA_SQL}";
        }
    }
}
