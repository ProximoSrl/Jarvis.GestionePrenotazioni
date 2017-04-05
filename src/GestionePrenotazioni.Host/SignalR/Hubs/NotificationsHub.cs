using Microsoft.AspNet.SignalR;
using System.Linq;

namespace GestionePrenotazioni.Host.SignalR.Hubs
{
    public class NotificationsHub : Hub
    {
        public void Subscribe(string readModelName, string[] filter)
        {
            if (filter.Any())
            {
                foreach (var id in filter)
                {
                    Groups.Add(Context.ConnectionId, readModelName + "@" + id);
                }
            }
            else
            {
                Groups.Add(Context.ConnectionId, readModelName);
            }
        }

        public void Unsubscribe(string readModelName, string[] filter)
        {
            if (filter.Any())
            {
                foreach (var id in filter)
                {
                    Groups.Remove(Context.ConnectionId, readModelName + "@" + id);
                }
            }
            else
            {
                Groups.Remove(Context.ConnectionId, readModelName);
            }
        }

    }
}
