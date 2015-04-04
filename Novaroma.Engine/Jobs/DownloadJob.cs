using Novaroma.Interface;
using Quartz;

namespace Novaroma.Engine.Jobs {

    [DisallowConcurrentExecution]
    public class DownloadJob : IJob {
        private readonly INovaromaEngine _engine;

        public DownloadJob(INovaromaEngine engine) {
            _engine = engine;
        }

        public void Execute(IJobExecutionContext context) {
            _engine.ExecuteDownloads().Wait();
        }
    }
}
