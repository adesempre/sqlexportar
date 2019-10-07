namespace SqlExporterCore.regras
{
    public interface ITipoObjetoSql
    {
        string Nome();
        string SqlListarObjetos(BancoDadosEnum banco);
        string SqlRecuperarScriptCriacao(BancoDadosEnum banco, string proprietario, string nomeObjeto);
    }
}
