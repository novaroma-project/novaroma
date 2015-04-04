namespace Novaroma.Interface {

    public interface IRuntimeService<out TService>: INovaromaService where TService: INovaromaService  {
        string Code { get; }
        TService Instance { get; }
        string DefaultCode { get; }
    }
}
