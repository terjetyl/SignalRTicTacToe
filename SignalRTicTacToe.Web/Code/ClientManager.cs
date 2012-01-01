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

        public ClientManager()
        {
            // Default anonymous delegates, so that we do not have to check for null when invoking these events.
            PlayerXAssigned += (sender, id) => { };
            PlayerOAssigned += (sender, id) => { };
            SpectatorAssigned += (sender, id) => { };
        }

        public int SpectatorCount
        {
            get { return _spectators.Count; }
        }

        public event ClientRoleAssignedDelegate PlayerXAssigned;
        public event ClientRoleAssignedDelegate PlayerOAssigned;
        public event ClientRoleAssignedDelegate SpectatorAssigned;

        public void AssignRole(string clientId)
        {
            if (_playerX == null)
            {
                _playerX = clientId;
                PlayerXAssigned.Invoke(this, clientId);
            }
            else if (_playerO == null)
            {
                _playerO = clientId;
                PlayerOAssigned.Invoke(this, clientId);
            }
            else
            {
                _spectators.Add(clientId);
                SpectatorAssigned.Invoke(this, clientId);
            }
        }

        public void Unassign(string clientId)
        {
            if (_playerX == clientId)
            {
                _playerX = null;

                var firstSpectator = _spectators.FirstOrDefault();
                if (firstSpectator != null)
                {
                    _spectators.Remove(firstSpectator);
                    _playerX = firstSpectator;
                    PlayerXAssigned.Invoke(this, _playerX);
                }
            }
            else if (_playerO == clientId)
            {
                _playerO = null;

                var firstSpectator = _spectators.FirstOrDefault();
                if (firstSpectator != null)
                {
                    _spectators.Remove(firstSpectator);
                    _playerO = firstSpectator;
                    PlayerOAssigned.Invoke(this, _playerO);
                }
            }
            else if (_spectators.Contains(clientId))
            {
                _spectators.Remove(clientId);
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