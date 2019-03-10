namespace Lem.Networking.Implementation.Connections
{
    // This is discriminator for actions in PumpMessages.
    internal enum ConnectionAction
    {
        Receive,
        Send,
        Update
    }
}
