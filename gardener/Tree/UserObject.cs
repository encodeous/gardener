using System;
using System.Collections.Generic;
using System.Text;

namespace gardener.Tree
{
    public class UserObject
    {
        public ulong UserId = 0;
        public int TreeId = 0;
        public int InvitedBy = 0;
        public HashSet<int> Friends = new HashSet<int>();
        public HashSet<int> FriendsInvited = new HashSet<int>();
        public int InviteTreeCode = 0;
        public long TotalPeopleInvited = 0;
    }
}
