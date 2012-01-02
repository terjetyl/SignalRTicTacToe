namespace SignalRTicTacToe.Web.Code
{
    public enum ClientRole
    {
        None,
        PlayerX,
        PlayerO,
        Spectator
    }

    public class ClientRoleAssignment
    {
        public string ClientId { get; set; }
        public ClientRole Role { get; set; }
    }

    public delegate void ClientRoleAssignedDelegate(object sender, string clientId);
    public delegate void ClientRoleAssignedWithRoleDelegate(object sender, ClientRoleAssignment assignment);

    /// <summary>
    /// Determines and keeps track of tic-tac-toe client roles (Player X, O, or Spectator)
    /// </summary>
    public interface IClientManager
    {
        int SpectatorCount { get; }

        event ClientRoleAssignedWithRoleDelegate ClientRoleAssigned;

        void AssignToNextAvailableRole(string clientId);
        ClientRole GetClientRole(string clientId);
        void RemoveClient(string clientId);

        // TODO: Rename or refactor to something more meaningful.
        void RotateRolesKeepingAsPlayer(ClientRole keepAsPlayer);
    }
}