using Novaroma.Engine;
using Novaroma.Interface;

namespace Novaroma.Win.Infrastructure {

    public class Db4OContextFactory: IContextFactory<NovaromaDb4OContext> {

        public NovaromaDb4OContext CreateContext() {
            return IoCContainer.Resolve<NovaromaDb4OContext>();
        }

        INovaromaContext IContextFactory.CreateContext() {
            return CreateContext();
        }
    }
}
