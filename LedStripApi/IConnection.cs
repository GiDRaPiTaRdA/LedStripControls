namespace LedStripApi
{
    public interface IConnection : IDisposable
    {
        string SendCommand(string command);
    }
}
