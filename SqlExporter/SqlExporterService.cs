using System.ServiceProcess;
using SqlExporterCore.service;

namespace SqlExporter
{
    public partial class SqlExporterService : ServiceBase
    {
        private SqlExporterServiceBase serviceBase;

        public SqlExporterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            serviceBase = new SqlExporterServiceBase();
            serviceBase.OnStart(args);
        }

        protected override void OnStop()
        {
            serviceBase.OnStop();
        }
    }
}
