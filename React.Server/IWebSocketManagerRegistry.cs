namespace React.Server
{
    public interface IWebSocketManagerRegistry
    {
        void AddWebSocketManager(string managerId, WebSocketManager manager);
        WebSocketManager GetWebSocketManager(string managerId);
    }
}
