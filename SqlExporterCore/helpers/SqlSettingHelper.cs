using SqlExporterCore.utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace SqlExporterCore.helpers
{
    public class SqlSettingHelper
    {
        private static NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string SEPARADOR_LISTAS = ";";
        public static readonly string SEPARADOR_SUBSTITUTO = "_";
        public static readonly string SEPARADOR_APP_SETTING = "-";

        public static readonly string APP_SETTING_CONNECTION_STRING_BASE = "connectionString";
        public static readonly string APP_SETTING_LOCAL_GERACAO_SCRIPT = "LocalScript";
        public static readonly string APP_SETTING_NOME_SERVIDORES = "ServidorSQL";
        public static readonly string APP_SETTING_NOME_BANCOS_DE_DADOS = "BancoSQL";
        public static readonly string APP_SETTING_DIAS_ATUALIZACAO = "NDiasModificacaoSQL";

        public static readonly int DEFAULT_DIAS_ATUALIZACAO = -2;

        public static readonly string CHAVE_SERVIDOR = "[servidor]";
        public static readonly string CHAVE_BANCO = "[banco]";
        public static readonly string CHAVE_TIPO = "[tipo]";
        public static readonly string CHAVE_PROPRIETARIO = "[proprietario]";
        public static readonly string CHAVE_NOME = "[nome]";

        private static SqlSettingHelper _instancia;

        public string connectionString { get; private set; }
        public string localScriptBase { get; private set; }
        public string[] servidores { get; private set; }
        public IDictionary<string, string> servidoresToFile { get; private set; }
        public string[] bancos { get; private set; }
        public IDictionary<string, IDictionary<string, string>> localScript { get; private set; }
        public int diasAtualizacao { get; private set; }
        private SqlSettingHelper()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(SqlExporterCore));

            this.connectionString = ConfigurationManager.AppSettings[APP_SETTING_CONNECTION_STRING_BASE];
            this.localScriptBase = ConfigurationManager.AppSettings[APP_SETTING_LOCAL_GERACAO_SCRIPT];

            string servidores = ConfigurationManager.AppSettings[APP_SETTING_NOME_SERVIDORES];
            this.servidores = this.StringToList(servidores);
            ConverterNomeServidores();

            string bancos = ConfigurationManager.AppSettings[APP_SETTING_NOME_BANCOS_DE_DADOS];
            this.bancos = this.StringToList(bancos);
            DescobrirLocaisScripts();

            try
            {
                var settingDias = int.Parse(ConfigurationManager.AppSettings[APP_SETTING_DIAS_ATUALIZACAO]);
                if (settingDias <= 0)
                    diasAtualizacao = DEFAULT_DIAS_ATUALIZACAO;
                else
                    diasAtualizacao = -1 * settingDias;
            }
            catch (Exception)
            {
                if (LOG.IsDebugEnabled)
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[APP_SETTING_DIAS_ATUALIZACAO]))
                        LOG.Debug($"Configuração de número de dias ({APP_SETTING_DIAS_ATUALIZACAO}) com valor inválido: {ConfigurationManager.AppSettings[APP_SETTING_DIAS_ATUALIZACAO]}");

                diasAtualizacao = DEFAULT_DIAS_ATUALIZACAO;
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(SqlExporterCore));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static SqlSettingHelper GetInstance()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(GetInstance));

            if (SqlSettingHelper._instancia == null)
            {
                if (LOG.IsDebugEnabled)
                    LOG.Debug("Iniciando instância de configuração...");

                SqlSettingHelper._instancia = new SqlSettingHelper();

                if (LOG.IsDebugEnabled)
                    LOG.Debug("..instância de configuração iniciada!");
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(GetInstance));

            return SqlSettingHelper._instancia;
        }

        public string GetFileName(string servidor, string banco, string tipo, string proprietario, string nome)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(GetFileName),
                    nameof(servidor), servidor,
                    nameof(banco), banco,
                    nameof(tipo), tipo,
                    nameof(proprietario), proprietario,
                    nameof(nome), nome);

            string local = localScript[servidor][banco];
            local = local.Replace(CHAVE_SERVIDOR, servidoresToFile[servidor]);
            local = local.Replace(CHAVE_BANCO, banco);
            local = local.Replace(CHAVE_TIPO, tipo);
            local = local.Replace(CHAVE_PROPRIETARIO, proprietario);
            local = local.Replace(CHAVE_NOME, nome);

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(GetFileName), local);

            return local;
        }

        public string[] StringToList(string param)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(StringToList), nameof(param), param);

            string[] values;
            if (param.Contains(SEPARADOR_LISTAS))
                values = param.Split(SEPARADOR_LISTAS.ToCharArray());
            else
                values = new string[] { param };

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(StringToList), values);

            return values;
        }

        private void ConverterNomeServidores()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(ConverterNomeServidores));

            servidoresToFile = new Dictionary<string, string>();
            foreach (string servidor in servidores)
            {
                servidoresToFile.Add(servidor, servidor.Replace(FileUtils.SEPARADOR_DIRETORIO, SEPARADOR_SUBSTITUTO));
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(ConverterNomeServidores));

        }

        private void DescobrirLocaisScripts()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(DescobrirLocaisScripts));

            localScript = new Dictionary<string, IDictionary<string, string>>();
            foreach (KeyValuePair<string, string> map in servidoresToFile)
            {
                string servidor = map.Value;
                string chaveServidor = string.Format("{0}{1}{2}", APP_SETTING_LOCAL_GERACAO_SCRIPT, SEPARADOR_APP_SETTING, servidor);
                string localServidor = ConfigurationManager.AppSettings[chaveServidor];
                IDictionary<string, string> local = new Dictionary<string, string>();
                foreach (string banco in bancos)
                {
                    string chaveBanco = string.Format("{0}{1}{2}", APP_SETTING_LOCAL_GERACAO_SCRIPT, SEPARADOR_APP_SETTING, banco);
                    string localBanco = ConfigurationManager.AppSettings[chaveBanco];
                    string chaveServidorBanco = string.Format("{0}{1}{2}{3}{4}", APP_SETTING_LOCAL_GERACAO_SCRIPT, SEPARADOR_APP_SETTING, servidor, SEPARADOR_APP_SETTING, banco);
                    string localServidorBanco = ConfigurationManager.AppSettings[chaveServidorBanco];
                    string localScript = localScriptBase;
                    if (localServidorBanco != null)
                        localScript = localServidorBanco;
                    else if (localServidor != null)
                        localScript = localServidor;
                    else if (localBanco != null)
                        localScript = localBanco;

                    localScript = localScript.Replace(CHAVE_SERVIDOR, servidor);
                    localScript = localScript.Replace(CHAVE_BANCO, banco);
                    local.Add(banco, localScript);
                }
                localScript.Add(map.Key, local);
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(DescobrirLocaisScripts));
        }
    }
}
