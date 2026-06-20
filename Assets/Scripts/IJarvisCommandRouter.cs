namespace Jarvis3DCommandCenter
{
    public interface IJarvisCommandRouter
    {
        MockRouteDecision RouteCommand(string command);
    }
}

