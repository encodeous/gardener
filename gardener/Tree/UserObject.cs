using System;
using System.Collections.Generic;
using System.Text;

namespace gardener.Tree
{
    class UserObject
    {
        public ulong UserId;
        public int TreeIndex;
        public int InvitedBy;
        public List<int> Friends;
        public List<int> FriendsInvited;
        public int InviteLink;
    }
}
