using PPNetLib.Prototypes;
using System.Collections.Generic;
using System.Linq;

namespace PPServer
{
    public class Guard
    {
        private readonly List<User> _authUsers;
        private readonly List<PossibleBannedIp> _bannedIps;
        private readonly Server _server;
        private readonly int _maxTries;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="server">Instance of the server object</param>
        /// <param name="maxTries">The maximum number of invalid requests before the client gets banned</param>
        public Guard(Server server, int maxTries = 10)
        {
            _server = server;
            _authUsers = new List<User>();
            _bannedIps = new List<PossibleBannedIp>();
            _maxTries = maxTries;
        }

        /// <summary>
        /// Add an user to the auth users colleciton
        /// </summary>
        /// <param name="u"></param>
        public void AddAuthUser(User u)
        {
            _authUsers.Add(u);
        }

        /// <summary>
        /// Remove an user from the auth users collection
        /// <param name="u">The user to be removed</param>
        /// </summary>
        public void RemoveAuthUser(User u)
        {
            _authUsers.Remove(u);
        }

        /// <summary>
        /// Find an user based on its IP
        /// </summary>
        /// <param name="ipPort"></param>
        public User GetAuthUser(string ipPort)
        {
            return _authUsers.FirstOrDefault(u => u.IpPort == ipPort);
        }

        /// <summary>
        /// Find an user based on its userId
        /// </summary>
        /// <param name="ipPort"></param>
        public User GetAuthUser(int userId)
        {
            return _authUsers.FirstOrDefault(u => u.Id == userId);
        }

        /// <summary>
        /// Return auth users collection
        /// </summary>
        /// <returns></returns>
        public List<User> GetAuthUsers()
        {
            return _authUsers;
        }

        /// <summary>
        /// Check if the user has a given GroupRole level
        /// </summary>
        /// <param name="u"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public bool CheckPrivileges(User u, GroupRole min)
        {
            if (!_authUsers.Contains(u))
                return false;

            return u.GroupRole >= min;
        }

        /// <summary>
        /// Finds user in the auth users collection based on its IP
        /// </summary>
        /// <param name="ipPort"></param>
        /// <returns></returns>
        public User IP2User(string ipPort)
        {
            return _authUsers.FirstOrDefault(user => user.IpPort == ipPort);
        }

        /// <summary>
        /// Check if an user has been banned
        /// </summary>
        /// <param name="u">The user to check</param>
        /// <returns>True if the user has been banned</returns>
        public bool IsBanned(string ipAddress)
        {
            var ip = _bannedIps.FirstOrDefault(x => x.IpPort.Split(':')[0] == ipAddress);
            if (ip == null)
                return false; 

            return ip.Tries > _maxTries;
        }

        /// <summary>
        /// Increase tries on a possible banned IP
        /// </summary>
        /// <param name="ipAddress"></param>
        public void TryCheck(string ipAddress)
        {
            var ip = _bannedIps.FirstOrDefault(x => x.IpPort.Split(':')[0] == ipAddress);
            if(ip != null)
            {
                ip.Try();
            }
            else
            {
                _bannedIps.Add(new PossibleBannedIp(ipAddress));
            }
        }

        /// <summary>
        /// Ban an IP address manually
        /// </summary>
        /// <param name="ipAddress"></param>
        public void BanIp(string ipAddress)
        {
            var ip = _bannedIps.FirstOrDefault(x => x.IpPort.Split(':')[0] == ipAddress);
            if(ip == null)
                _bannedIps.Add(new PossibleBannedIp(ipAddress));
        }

        /// <summary>
        /// Check if a banned user's IP ban has expired and unban if necessary
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool CheckExpired(string ipAddress)
        {
            var ip = _bannedIps.FirstOrDefault(x => x.IpPort.Split(':')[0] == ipAddress);
            if (ip == null)
                return true;

            if (ip.HasExpired())
            {
                _bannedIps.Remove(ip);
                return true;
            }
            else
                return false;
        }

        public List<PossibleBannedIp> GetBannedIps()
        {
            return _bannedIps.Where(ip => ip.Tries > _maxTries).ToList();
        }
    }
}
