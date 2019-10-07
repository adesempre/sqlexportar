using SqlExporterCore.service;

namespace SqlExporterTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new SqlExporterServiceBase();
            service.OnStart(null);

            var esperarPor = 60 * 60; // 1h
            for (var i = 0; i < esperarPor; i++)
            {
                if (service.ExportacaoRealizada)
                    break;

                System.Threading.Thread.Sleep(1000);
            }

            service.OnStop();
        }
    }
}
