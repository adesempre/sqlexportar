using SqlExporterCore.utils;
using System;
using System.Configuration;

namespace SqlExporterCore.helpers
{
    public class GitHelper
    {
        private static NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private static readonly string[][] COMANDOS_GIT = new string[][]
            {
                new string[] { "add", "add -A" },
                new string[] { "commit", "commit -m \"Processo automático: Acompanhamento de mudanças na base de dados\"" },
                new string[] { "push", "push" }
            };

        private static readonly string APP_SETTING_COMANDO_GIT = "comandoGIT";
        private static readonly string APP_SETTING_REPOSITORIO_GIT = "repositorioGIT";


        private string repositorio;

        private string caminhoGit;



        public GitHelper()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(GitHelper));

            caminhoGit = ConfigurationManager.AppSettings[APP_SETTING_COMANDO_GIT];
            if (!string.IsNullOrEmpty(caminhoGit))
                caminhoGit = $"\"{caminhoGit}\"";

            if (LOG.IsDebugEnabled)
                LOG.Debug($"{nameof(caminhoGit)}={caminhoGit}");

            repositorio = ConfigurationManager.AppSettings[APP_SETTING_REPOSITORIO_GIT];
            if (!string.IsNullOrEmpty(repositorio))
                repositorio = $"-C \"{repositorio}\"";

            if (LOG.IsDebugEnabled)
                LOG.Debug($"{nameof(repositorio)}={repositorio}");

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(GitHelper));
        }

        public void AdicionarCommitarSincronizarRepositorio()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(AdicionarCommitarSincronizarRepositorio));

            if (string.IsNullOrEmpty(caminhoGit) || string.IsNullOrEmpty(repositorio))
                LOG.Info("GIT não parametrizado.");

            LOG.Info("Sincronizando arquivos...");
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                foreach (var c in COMANDOS_GIT)
                {
                    LOG.Info($"Executando comando GIT {c[0]}...");
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = caminhoGit;
                    startInfo.Arguments = $"{repositorio} {c[1]}";
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    LOG.Info($"...{c[0]} executado com sucesso!");
                }

                LOG.Info("...arquivos sincronizados!");
            }
            catch (Exception e)
            {
                throw new Exception("Não foi possível sincronizar com o GIT", e);
            }
            finally
            {
                if (LOG.IsTraceEnabled)
                    LOG.EndTrace(nameof(AdicionarCommitarSincronizarRepositorio));
            }
        }
    }
}
