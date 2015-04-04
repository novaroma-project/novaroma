namespace Novaroma.Interface {

    public interface IContextFactory<out TContext>: IContextFactory where TContext: INovaromaContext {

        new TContext CreateContext();
    }

    public interface IContextFactory {
        INovaromaContext CreateContext();
    }
}
