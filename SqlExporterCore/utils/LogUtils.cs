using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlExporterCore.utils
{
    public static class LogUtils
    {
        public static void InitTrace(this NLog.Logger log, string metodo, params object[] parametros)
        {
            var texto = new StringBuilder($"Entrada do método: {metodo}");
            if (parametros != null)
                for (int i = 0; i < parametros.Length; i += 2)
                    texto.Append($" [{parametros[i]}={parametros[i + 1]}]");

            log.Trace(texto.ToString());
        }

        public static void EndTrace(this NLog.Logger log, string metodo, object valor = null)
        {
            if (valor == null)
                log.Trace($"Saída do método: {metodo}");
            else
                log.Trace($"Saída do método: {metodo}=[{valor}]");
        }
    }
}
