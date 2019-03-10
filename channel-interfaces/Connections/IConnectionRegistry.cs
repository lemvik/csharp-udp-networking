namespace Lem.Networking.Connections
{
    public interface IConnectionRegistry
    {
        IConnection AllocateConnection(uint connectionId);
    }
}
