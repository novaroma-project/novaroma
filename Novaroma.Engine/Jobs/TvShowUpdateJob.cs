using Novaroma.Interface;
using Quartz;

namespace Novaroma.Engine.Jobs {

    [DisallowConcurrentExecution]
    public class TvShowUpdateJob : IJob {
        private readonly INovaromaEngine _engine;

        public TvShowUpdateJob(INovaromaEngine engine) {
            _engine = engine;
        }

        public void Execute(IJobExecutionContext context) {
            _engine.ExecuteTvShowUpdates().Wait();
        }
    }
}
