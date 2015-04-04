namespace Novaroma.Interface.Model {

    public interface IEntity {
        object Id { get; }
        bool IsModified { get; set; }
    }
}
