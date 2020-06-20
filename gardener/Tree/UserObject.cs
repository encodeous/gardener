using System;
using System.Collections.Generic;
using System.Text;

namespace gardener.Tree
{
    public class UserObject
    {
        public ulong UserId;
        public int TreeIndex;
        public int InvitedBy;
        public HashSet<int> Friends;
        public HashSet<int> FriendsInvited;
        public int TreeCode;
    }
}
