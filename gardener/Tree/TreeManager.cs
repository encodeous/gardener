using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace gardener.Tree
{
    class TreeManager
    {
        public TreeState TreeState;
        public HashSet<ulong> UsersConnecting = new HashSet<ulong>();
        public async Task SaveAsync()
        {

        }

        public async Task LoadAsync()
        {
            TreeState = new TreeState();
            if (TreeState.Users.Count == 0)
            {
                var usr = CreateUser(236596516423204865);
                usr.TreeIndex = 0;
                usr.InvitedBy = 0;
                TreeState.Users.Add(usr);
                TreeState.InviteMap.Add(usr.TreeCode, 0);
                TreeState.UserMap[236596516423204865] = 0;
            }
        }

        public void OnUserJoin(SocketGuildUser user)
        {
            user.AddRoleAsync(user.Guild.GetRole(Garden.NotConnectedRole));
            user.SendMessageAsync("**Welcome to The Friend Tree!**\n" +
                                  "\n" +
                                  "Please send me a **Tree Code** to join the server.\n" +
                                  "A **Tree Code** looks like `T-123-123-123`.\n" +
                                  "Just message me in this DM to connect your account to the Tree!");
            UsersConnecting.Add(user.Id);
        }

        private readonly Regex _matcher = new Regex("T-[0-9]{3}-[0-9]{3}-[0-9]{3}");
        public async Task OnUserMessageAsync(SocketMessage message)
        {
            var result = _matcher.Match(message.Content);
            await RegisterUser(message.Author, result.Value);
        }

        public async Task RegisterUser(SocketUser user, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                await user.SendMessageAsync("**Please double check your Tree Code!**").ConfigureAwait(false);
            }
            else
            {
                try
                {
                    int inviteCode = ParseCode(code);
                    if (TreeState.InviteMap.ContainsKey(inviteCode))
                    {
                        var inviter = TreeState.Users[TreeState.InviteMap[inviteCode]];
                        var inviteUser = await Garden.TheFriendTree.GetUserAsync(inviter.UserId).ConfigureAwait(false);
                        if (inviteUser != null)
                        {
                            await user.SendMessageAsync("Your account has been successfully " +
                                                        "linked to the server using " + inviteUser.Username + ":" + inviteUser.Discriminator+"'s code.").ConfigureAwait(false);
                        }
                        else
                        {
                            await user.SendMessageAsync("Your account has been successfully linked to the server.").ConfigureAwait(false);
                        }

                        var userObj = CreateUser(user.Id);
                        userObj.InvitedBy = inviter.TreeIndex;
                        userObj.Friends.Add(inviter.TreeIndex);
                        int newUserIndex = TreeState.Users.Count;
                        userObj.TreeIndex = newUserIndex;

                        TreeState.Users.Add(userObj);

                        TreeState.UserMap[user.Id] = userObj.TreeIndex;

                        inviter.FriendsInvited.Add(newUserIndex);
                        inviter.Friends.Add(newUserIndex);

                        UsersConnecting.Remove(user.Id);
                        await GiveRoles(user.Id);
                    }
                    else
                    {
                        await user.SendMessageAsync("**The Tree Code you have entered is not valid.**").ConfigureAwait(false);
                    }
                }
                catch
                {
                    await user.SendMessageAsync("**An error occurred when processing your Tree Code, please try again.**").ConfigureAwait(false);
                }
            }
        }

        public UserObject GetUser(ulong uid)
        {
            return TreeState.Users[TreeState.UserMap[uid]];
        }

        public string GetTreeCodeFormatted(int code)
        {
            return
                $"T-{code / 100000000}{code / 10000000}{code / 1000000}" +
                $"-{code / 100000}{code / 10000}{code / 1000}" +
                $"-{code / 100}{code / 10}{code}";
        }

        public UserObject CreateUser(ulong uid)
        {
            var obj = new UserObject {UserId = uid, Friends = new List<int>(), FriendsInvited = new List<int>()};
            var rng = new Random();
            int code = rng.Next(0, 999999999);
            while (TreeState.InviteMap.ContainsKey(code))
            {
                code = rng.Next(0, 999999999);
            }

            obj.TreeCode = code;
            return obj;
        }

        int ParseCode(string code)
        {
            int value = 0;
            value = (value + ParseChar(code[2])) * 10;
            value = (value + ParseChar(code[3])) * 10;
            value = (value + ParseChar(code[4])) * 10;

            value = (value + ParseChar(code[6])) * 10;
            value = (value + ParseChar(code[7])) * 10;
            value = (value + ParseChar(code[8])) * 10;

            value = (value + ParseChar(code[10])) * 10;
            value = (value + ParseChar(code[11])) * 10;
            value = (value + ParseChar(code[12]));
            return value;
        }

        int ParseChar(char c)
        {
            return c - '0';
        }

        public async Task GiveRoles(ulong uid)
        {
            var user = await Garden.TheFriendTree.GetUserAsync(uid).ConfigureAwait(false);
            await user.AddRoleAsync(Garden.TheFriendTree.GetRole(Garden.MemberRole)).ConfigureAwait(false);
            await user.RemoveRoleAsync(Garden.TheFriendTree.GetRole(Garden.NotConnectedRole)).ConfigureAwait(false);
        }
    }
}
