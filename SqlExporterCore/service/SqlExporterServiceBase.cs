using System.Timers;
using System;
using SqlExporterCore.utils;
using SqlExporterCore.exporter;
using SqlExporterCore.helpers;
using System.Configuration;

namespace SqlExporterCore.service
{
    public class SqlExporterServiceBase
    {
        private static NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private static readonly string APP_SETTING_CONNECTION_HORARIO_EXPORTACAO = "HorarioExecucaoExportacaoSQL";
        private static readonly int PING_TIME = 60 * 1000; // 60s = 1 minuto
        
        private static bool processando = false;
        private static bool paradaSolicitada = false;

        private Timer temporizador;
        private DefaultSqlExporter exporter;

        public bool ExportacaoRealizada { get; private set; }

        private TimeSpan HorarioProximaExecucao
        {
            get
            {
                var agora = DateTime.Now.AddMinutes(5);
                var hora = agora.Hour;
                var minuto = agora.Minute;

                var horarioExecucao = ConfigurationManager.AppSettings[APP_SETTING_CONNECTION_HORARIO_EXPORTACAO];
                if (!string.IsNullOrEmpty(horarioExecucao))
                {
                    if (horarioExecucao.Contains(":"))
                    {
                        var partes = horarioExecucao.Split(':');
                        if (partes.Length > 1)
                            int.TryParse(partes[1], out minuto);

                        int.TryParse(partes[0], out hora);
                    }
                    else
                    {
                        int.TryParse(horarioExecucao, out hora);
                    }
                }
                return new TimeSpan(hora, minuto, 0);
            }
        }

        public void OnStart(string[] args)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(OnStart), nameof(args), args);

            ExportacaoRealizada = false;

            temporizador = new Timer();
            temporizador.Elapsed += new ElapsedEventHandler(Processar);
            temporizador.AutoReset = true;
            temporizador.Interval = PING_TIME;
            temporizador.Start();

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(OnStart));
        }

        public void OnStop()
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(OnStop));

            paradaSolicitada = true;

            if (temporizador != null && temporizador.Enabled)
                temporizador.Stop();

            if (exporter != null)
                exporter.InterromperProcesso = true;

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(OnStop));
        }

        private void Processar(object source, ElapsedEventArgs eventArgs)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(Processar),
                    nameof(source), source,
                    nameof(eventArgs), eventArgs);

            if (processando)
            {
                if (LOG.IsDebugEnabled)
                    LOG.Debug("Processo já está em execução. Aguardando término do processo iniciado...");

                return;
            }

            if (LOG.IsDebugEnabled)
                LOG.Debug("PING");

            try
            {
                temporizador.Stop();
                processando = true;

                var agora = DateTime.Now;
                var horario = new TimeSpan(agora.Hour, agora.Minute, 0); // ignora os segundos
                if (horario.Equals(HorarioProximaExecucao))
                {
                    if (LOG.IsDebugEnabled)
                        LOG.Debug("Iniciando serviço de exportação de objetos de banco de dados!");

                    exporter = new DefaultSqlExporter();
                    exporter.GerarArquivos();

                    if (!paradaSolicitada)
                    {
                        var git = new GitHelper();
                        git.AdicionarCommitarSincronizarRepositorio();
                    }

                    ExportacaoRealizada = true;
                }
            }
            catch (Exception e)
            {
                LOG.Error(e, "Não foi possível executar o processo de exportação de objetos de banco de dados");
            }
            finally
            {
                processando = false;
                if (!paradaSolicitada)
                    temporizador.Start();
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(Processar));
        }
    }
}
