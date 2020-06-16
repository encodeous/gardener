using System;
using System.Collections.Generic;
using System.Text;

namespace gardener.Tree
{
    class TreeState
    {
        public List<UserObject> Graph = new List<UserObject>();
        /// <summary>
        /// Maps Invite Link to User
        /// </summary>
        public Dictionary<int, int> InviteMap = new Dictionary<int, int>();
    }
}
