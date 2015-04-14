using Novaroma.Interface;
using Quartz;

namespace Novaroma.Engine.Jobs {

    [DisallowConcurrentExecution]
    public class SubtitleDownloadJob : IJob {
        private readonly INovaromaEngine _engine;

        public SubtitleDownloadJob(INovaromaEngine engine) {
            _engine = engine;
        }

        public void Execute(IJobExecutionContext context) {
            _engine.ExecuteSubtitleDownloads().Wait();
        }
    }
}
