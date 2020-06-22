using System;
using System.Collections.Generic;
using System.Text;

namespace gardener.Tree
{
    class TreeState
    {
        public List<UserObject> Users = new List<UserObject>();
        /// <summary>
        /// Maps Invite Link to User
        /// </summary>
        public Dictionary<int, int> InviteMap = new Dictionary<int, int>();
        public Dictionary<ulong, int> UserMap = new Dictionary<ulong, int>();
        public HashSet<ulong> UsersConnecting = new HashSet<ulong>();
        public HashSet<ulong> UnfilteredChannels = new HashSet<ulong>();
    }
}
