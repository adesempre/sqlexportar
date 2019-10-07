using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SqlExporter
{
    static class SqlExporterApp
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SqlExporterService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
