namespace SignalRTicTacToe.Web.Code
{
    public enum ClientRole
    {
        None,
        PlayerX,
        PlayerO,
        Spectator
    }

    /// <summary>
    /// Determines and keeps track of tic-tac-toe client roles (Player X, O, or Spectator)
    /// </summary>
    public interface IClientManager
    {
        int SpectatorCount { get; }

        event ClientRoleAssignedDelegate PlayerXAssigned;
        event ClientRoleAssignedDelegate PlayerOAssigned;
        event ClientRoleAssignedDelegate SpectatorAssigned;

        void AssignRole(string clientId);
        ClientRole GetClientRole(string clientId);
        void RotateRoleOutWithSpectator(ClientRole role);
        void Unassign(string clientId);
    }
}