using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalRTicTacToe.Web.Code
{
    public class ClientManager : IClientManager
    {
        private string _playerX;
        private string _playerO;
        private readonly IList<string> _spectators = new List<string>();

        public int SpectatorCount
        {
            get { return _spectators.Count; }
        }

        public event EventHandler<ClientRoleAssignedArgs> ClientRoleAssigned = (sender, args) => { }; 

        public void AssignToNextAvailableRole(string clientId)
        {
            ClientRole nextAvailableRole = GetNextAvailableRole();
            AssignClientToRole(clientId, nextAvailableRole);
            ClientRoleAssigned(this, new ClientRoleAssignedArgs(clientId, nextAvailableRole));
        }

        private ClientRole GetNextAvailableRole()
        {
            if (_playerX == null)
                return ClientRole.PlayerX;
            if (_playerO == null)
                return ClientRole.PlayerO;

            return ClientRole.Spectator;
        }

        private void AssignClientToRole(string clientId, ClientRole nextAvailableRole)
        {
            switch (nextAvailableRole)
            {
                case ClientRole.PlayerX:
                    _playerX = clientId;
                    break;
                case ClientRole.PlayerO:
                    _playerO = clientId;
                    break;
                case ClientRole.Spectator:
                    _spectators.Add(clientId);
                    break;
            }
        }

        // TODO: Needs refactoring badly.
        public void RotateRolesKeepingAsPlayer(ClientRole keepAsPlayer)
        {
            var playerFormallyKnownAsX = _playerX;
            var playerFormallyKnownAsO = _playerO;
            var firstSpectator = _spectators.FirstOrDefault();

            _playerX = null;
            _playerO = null;

            if (firstSpectator != null)
            {
                _spectators.Remove(firstSpectator);
                if (keepAsPlayer == ClientRole.PlayerX)
                {
                    AssignToNextAvailableRole(firstSpectator);
                    AssignToNextAvailableRole(playerFormallyKnownAsX);
                    AssignToNextAvailableRole(playerFormallyKnownAsO);
                }
                else
                {
                    AssignToNextAvailableRole(playerFormallyKnownAsO);
                    AssignToNextAvailableRole(firstSpectator);
                    AssignToNextAvailableRole(playerFormallyKnownAsX);
                }
            }
            else
            {
                AssignToNextAvailableRole(playerFormallyKnownAsO);
                AssignToNextAvailableRole(playerFormallyKnownAsX);
            }
        }

        public void RemoveClient(string clientId)
        {
            if (_playerX == clientId)
            {
                _playerX = null;
                MoveFirstSpectatorIntoAvailablePlayerRole();
            }
            else if (_playerO == clientId)
            {
                _playerO = null;
                MoveFirstSpectatorIntoAvailablePlayerRole();
            }
            else if (_spectators.Contains(clientId))
            {
                _spectators.Remove(clientId);
            }
        }

        private void MoveFirstSpectatorIntoAvailablePlayerRole()
        {
            var firstSpectator = _spectators.FirstOrDefault();
            if (firstSpectator != null)
            {
                RemoveClient(firstSpectator);
                AssignToNextAvailableRole(firstSpectator);
            }
        }

        public ClientRole GetClientRole(string clientId)
        {
            if (clientId == _playerX)
            {
                return ClientRole.PlayerX;
            }
            if (clientId == _playerO)
            {
                return ClientRole.PlayerO;
            }
            if (_spectators.Contains(clientId))
            {
                return ClientRole.Spectator;
            }
            return ClientRole.None;
        }
    }
}