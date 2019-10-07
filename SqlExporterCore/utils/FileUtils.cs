using System.IO;

namespace SqlExporterCore.utils
{
    public class FileUtils
    {
        private static NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string SEPARADOR_DIRETORIO = "\\";
        public static readonly string INDICADOR_REDE = "\\\\";

        public static readonly string INDICADOR_BLOQUEIO_VALIDACAO = "[";

        public void CriarDiretorioQuandoNaoExistir(string caminho)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(CriarDiretorioQuandoNaoExistir), nameof(caminho), caminho);

            bool ehCaminhoRede = caminho.StartsWith(INDICADOR_REDE);
            if (ehCaminhoRede)
                caminho = caminho.Substring(INDICADOR_REDE.Length);

            var partes = caminho.Split(SEPARADOR_DIRETORIO.ToCharArray());
            if (partes.Length == 0)
                return; // sem separador

            if (partes.Length == 1)
                return; //diretório atual ou raiz

            if (partes.Length == 2)
                return; //diretório raiz e arquivo apenas

            string parte = (ehCaminhoRede ? $"{INDICADOR_REDE}{partes[0]}" : partes[0]);
            int limite = partes.Length - 1;
            for (int i = 1; i < limite; i++)
            {
                if (partes[i].Contains(INDICADOR_BLOQUEIO_VALIDACAO))
                    break;

                parte = $"{parte}{SEPARADOR_DIRETORIO}{partes[i]}";
                if (!Directory.Exists(parte))
                {
                    Directory.CreateDirectory(parte);

                    if (LOG.IsDebugEnabled)
                        LOG.Debug($"Diretório criado: {parte}");
                }
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(CriarDiretorioQuandoNaoExistir));
        }

        public bool VerificarSeDiretorioExiste(string caminho)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(VerificarSeDiretorioExiste), nameof(caminho), caminho);

            bool existe = true;
            bool ehCaminhoRede = caminho.StartsWith(INDICADOR_REDE);
            if (ehCaminhoRede)
                caminho = caminho.Substring(INDICADOR_REDE.Length);

            var partes = caminho.Split(SEPARADOR_DIRETORIO.ToCharArray());
            if (partes.Length > 2)
            {
                string parte = (ehCaminhoRede ? $"{INDICADOR_REDE}{partes[0]}" : partes[0]);
                int limite = partes.Length - 1;
                for (int i = 1; i < limite; i++)
                {
                    if (partes[i].Contains(INDICADOR_BLOQUEIO_VALIDACAO))
                        break;

                    parte = $"{parte}{SEPARADOR_DIRETORIO}{partes[i]}";
                    if (!Directory.Exists(parte))
                    {
                        existe = false;
                        break;
                    }
                }
            }

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(VerificarSeDiretorioExiste), existe);

            return existe;
        }

        public void RecriarArquivoSeExistir(string caminhoCompleto, string conteudo)
        {
            if (LOG.IsTraceEnabled)
                LOG.InitTrace(nameof(RecriarArquivoSeExistir),
                    nameof(caminhoCompleto), caminhoCompleto,
                    nameof(conteudo), conteudo);

            CriarDiretorioQuandoNaoExistir(caminhoCompleto);
            if (File.Exists(caminhoCompleto))
            {
                if (LOG.IsDebugEnabled)
                    LOG.Debug($"Arquivo já existe e será excluído: {caminhoCompleto}");

                File.Delete(caminhoCompleto);
            }

            using (var fs = File.OpenWrite(caminhoCompleto))
            using (var fb = new StreamWriter(fs))
                fb.WriteLine(conteudo);

            if (LOG.IsDebugEnabled)
                LOG.Debug($"Arquivo criado com sucesso: {caminhoCompleto}");

            if (LOG.IsTraceEnabled)
                LOG.EndTrace(nameof(RecriarArquivoSeExistir));
        }
    }
}
