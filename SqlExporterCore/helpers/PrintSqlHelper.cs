using System;

namespace SqlExporterCore.helpers
{
    public class PrintSqlHelper
    {
        public void PrintToConsole()
        {
            var helper = SqlSettingHelper.GetInstance();
            Console.WriteLine($"ConnectionString: {helper.connectionString}");

            Console.WriteLine("Bancos:");
            foreach (var b in helper.bancos)
                Console.WriteLine(b);

            Console.WriteLine("Servidores:");
            foreach (var s in helper.servidores)
                Console.WriteLine(s);

            foreach (var k in helper.servidoresToFile.Keys)
                Console.WriteLine($"{k} mapeado para: {helper.servidoresToFile[k]}");

            foreach (var k in helper.localScript.Keys)
            {
                Console.WriteLine($"Mapa de arquivos para: {k}");
                foreach (var l in helper.localScript[k].Keys)
                    Console.WriteLine($"{k}, l: {l}, arquivo: {helper.localScript[k][l]}");
            }
        }
    }
}
