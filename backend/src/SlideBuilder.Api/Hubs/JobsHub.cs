using Microsoft.AspNetCore.SignalR;

namespace SlideBuilder.Api.Hubs;

public class JobsHub : Hub
{
    public async Task JoinProjectGroup(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task LeaveProjectGroup(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
    }
}

public interface IJobsClient
{
    Task JobStatusUpdated(object status);
    Task JobProgressUpdated(object progress);
}
