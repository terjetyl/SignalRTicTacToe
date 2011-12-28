namespace SignalRTicTacToe.Web.Code
{
    public enum ClientRole
    {
        Unknown,
        PlayerX,
        PlayerO,
        Spectator
    }

    /// <summary>
    /// Determines and keeps track of tic-tac-toe client roles (Player X, O, or Spectator)
    /// </summary>
    public interface IClientManager
    {
        event ClientRoleAssignedDelegate PlayerXAssigned;
        event ClientRoleAssignedDelegate PlayerOAssigned;
        event ClientRoleAssignedDelegate SpectatorAssigned;
        
        void AssignRole(string clientId);
        ClientRole GetClientRole(string clientId);
    }
}