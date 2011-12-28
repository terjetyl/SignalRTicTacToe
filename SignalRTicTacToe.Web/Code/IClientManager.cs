namespace SignalRTicTacToe.Web.Code
{
    public enum ClientRole
    {
        Unknown,
        PlayerX,
        PlayerO,
        Spectator
    }

    public interface IClientManager
    {
        event ClientRoleAssignedDelegate PlayerXAssigned;
        event ClientRoleAssignedDelegate PlayerOAssigned;
        event ClientRoleAssignedDelegate SpectatorAssigned;
        
        void AssignRole(string clientId);
        ClientRole GetClientRole(string clientId);
    }
}