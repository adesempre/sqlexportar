using SqlExporterCore.helpers;
using SqlExporterCore.regras;
using SqlExporterCore.utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlExporterCore.exporter
{
    public class DefaultSqlExporter
    {
        private static NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private static readonly string REGEX_NOME_OBJETO_VALIDO = "^[a-zA-Z][a-zA-Z0-9_]{1,}";

        private SqlSettingHelper helper;
        private FileUtils fileUtils;
        private ITipoObjetoSql[] tiposObjetos;

        public bool InterromperProcesso { get; set; }

        public DefaultSqlExporter()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(DefaultSqlExporter));

            InterromperProcesso = false;

            helper = SqlSettingHelper.GetInstance();
            fileUtils = new FileUtils();
            tiposObjetos = new SqlTiposFabrica().ListarTipos();

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(DefaultSqlExporter));
        }


        private string GerarStringConexao(string servidor, string banco)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(GerarStringConexao),
                    nameof(servidor), servidor,
                    nameof(banco), banco);

            var connectionString = helper.connectionString.Replace(SqlSettingHelper.CHAVE_SERVIDOR, servidor);
            connectionString = connectionString.Replace(SqlSettingHelper.CHAVE_BANCO, banco);

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(GerarStringConexao), connectionString);

            return connectionString;
        }
        public void GerarArquivos()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(GerarArquivos));

            var datIni = DateTime.Now;
            LOG.Info($"Iniciando processo de geração de arquivos: {datIni.ToLongTimeString()}");

            foreach (var servidor in helper.localScript.Keys)
                foreach (var banco in helper.localScript[servidor].Keys)
                {
                    LOG.Info($"Gerando objetos para o servidor {servidor} e banco {banco}");
                    try
                    {
                        ProcessarRegras(servidor, banco);
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e, $"Não foi possível gerar os objetos para o servidor {servidor} e banco {banco}");
                    }

                    if (InterromperProcesso)
                        break;
                }

            var datFin = DateTime.Now;
            LOG.Info($"Processo de geração de arquivos encerrado: {datFin.ToLongTimeString()}");

            LOG.Info($"Tempo total de processamento: {new DateTime(datFin.Subtract(datIni).Ticks).ToLongTimeString()}");

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(GerarArquivos));
        }

        private void ProcessarRegras(string servidor, string banco)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(ProcessarRegras),
                    nameof(servidor), servidor,
                    nameof(banco), banco);

            var connectionString = GerarStringConexao(servidor, banco);
            if (LOG.IsDebugEnabled)
                LOG.Debug($"String de conexao gerada: {connectionString}");

            using (var objConn = new SqlConnection(connectionString))
            {

                DateTime datRef;
                if (fileUtils.VerificarSeDiretorioExiste(helper.GetFileName(servidor, banco, "[]", "[]", "[]")))
                    datRef = DateTime.Today.AddDays(helper.diasAtualizacao);
                else
                    datRef = DateTime.MinValue;

                objConn.Open();
                foreach (var tipo in tiposObjetos)
                {
                    var objetos = IdentificarObjetos(objConn.CreateCommand(), tipo);
                    if (!InterromperProcesso)
                        foreach (var o in objetos)
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(o.Nome, REGEX_NOME_OBJETO_VALIDO))
                            {
                                string arquivo = helper.GetFileName(servidor, banco, o.Tipo, o.Proprietario, o.Nome);

                                if (datRef.CompareTo(o.DataModificacao) < 0) // sofreu modificação na data de corte
                                    GerarScript(objConn.CreateCommand(), arquivo, tipo, o);
                                else if (!System.IO.File.Exists(arquivo)) // não sofreu alteração, mas ainda não foi versionado
                                    GerarScript(objConn.CreateCommand(), arquivo, tipo, o);
                            }

                            if (InterromperProcesso)
                                break;
                        }
                }

                objConn.Close();
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(ProcessarRegras));
        }

        private void GerarScript(SqlCommand objCommand, string arquivo, ITipoObjetoSql tipo, SqlObject o)
        {
            try
            {
                objCommand.CommandText = tipo.SqlRecuperarScriptCriacao(BancoDadosEnum.SQL_SERVER, o.Proprietario, o.Nome);
                using (var objReader = objCommand.ExecuteReader())
                {
                    string script = string.Empty;
                    while (objReader.Read())
                    {
                        script = script + objReader.GetString(0);
                    }
                    objReader.Close();

                    fileUtils.RecriarArquivoSeExistir(arquivo, script);
                }
            }
            catch (Exception e)
            {
                LOG.Info($"Não foi possível gerar o objeto {o.Proprietario}.{o.Nome}:\n\t{e.Message}");
                if (LOG.IsDebugEnabled)
                    LOG.Debug(e);
            }
        }

        private List<SqlObject> IdentificarObjetos(SqlCommand objCommand, ITipoObjetoSql tipo)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(IdentificarObjetos),
                    nameof(objCommand), objCommand,
                    nameof(tipo), tipo);

            LOG.Info($"Listando objetos do tipo {tipo.Nome()}");

            var objetos = new List<SqlObject>();
            try
            {
                objCommand.CommandText = tipo.SqlListarObjetos(BancoDadosEnum.SQL_SERVER);
                using (var objReader = objCommand.ExecuteReader())
                {
                    while (objReader.Read())
                    {
                        objetos.Add(new SqlObject()
                        {
                            Tipo = tipo.Nome(),
                            Proprietario = objReader.GetString(0),
                            Nome = objReader.GetString(1),
                            DataModificacao = objReader.GetDateTime(2)
                        });

                        if (InterromperProcesso)
                            break;
                    }
                    objReader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Não foi possível recuperar os objetos do tipo {tipo.Nome()}", e);
            }

            if (LOG.IsDebugEnabled)
                LOG.Debug($"Total de objetos do tipo {tipo.Nome()}: {objetos.Count}");

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(IdentificarObjetos), objetos);

            return objetos;
        }
    }
}
