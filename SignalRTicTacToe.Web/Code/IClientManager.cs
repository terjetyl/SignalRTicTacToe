using System;

namespace SignalRTicTacToe.Web.Code
{
    public enum ClientRole
    {
        None,
        PlayerX,
        PlayerO,
        Spectator
    }

    public class ClientRoleAssignedArgs : EventArgs
    {
        public ClientRoleAssignedArgs(string clientId, ClientRole role)
        {
            ClientId = clientId;
            Role = role;
        }
        
        public string ClientId { get; set; }
        public ClientRole Role { get; set; }
    }

    /// <summary>
    /// Determines and keeps track of tic-tac-toe client roles (Player X, O, or Spectator)
    /// </summary>
    public interface IClientManager
    {
        int SpectatorCount { get; }

        event EventHandler<ClientRoleAssignedArgs> ClientRoleAssigned; 

        void AssignToNextAvailableRole(string clientId);
        ClientRole GetClientRole(string clientId);
        void RemoveClient(string clientId);

        // TODO: Rename or refactor to something more meaningful.
        void RotateRolesKeepingAsPlayer(ClientRole keepAsPlayer);
    }
}