using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace gardener.Tree
{
    class TreeManager
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public async Task SaveAsync()
        {
            if (Garden.TreeState != null)
            {
                await _semaphore.WaitAsync(Program.StopToken);
                if (Program.StopToken.IsCancellationRequested) return;
                var serialized = JsonConvert.SerializeObject(Garden.TreeState);
                await File.WriteAllTextAsync("data/tree.garden", serialized);
                _semaphore.Release();
            }
        }

        public async Task LoadAsync()
        {
            Garden.TreeState = File.Exists("data/tree.garden") ?
                JsonConvert.DeserializeObject<TreeState>(await File.ReadAllTextAsync("data/tree.garden")) : new TreeState();

            if (Garden.TreeState.Users.Count == 0)
            {
                var usr = CreateUser(236596516423204865);
                usr.TreeId = 0;
                usr.InvitedBy = 0;
                Garden.TreeState.Users.Add(usr);
                Garden.TreeState.InviteMap.Add(usr.InviteTreeCode, 0);
                Garden.TreeState.UserMap[236596516423204865] = 0;
            }

            foreach (var user in await Garden.TheFriendTree.GetUsersAsync())
            {
                var id = user.Id;

                if (Garden.Tree.GetUser(id) == null && !Garden.TreeState.UsersConnecting.Contains(id) && !user.IsBot && !user.IsWebhook)
                {
                    try
                    {
                        await user.SendMessageAsync("**Sorry for this late message, it seems like you have joined right when I restarted for an upgrade. You may now connect to the server!\n**");
                        await Garden.Tree.OnUserJoin(user as SocketGuildUser);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public async Task OnUserLeave(SocketGuildUser user)
        {
            var usr = GetUser(user.Id);
            if (usr != null)
            {
                var channel = await Garden.TheFriendTree.GetTextChannelAsync(Garden.JoinChannel);
                await channel.SendMessageAsync($"Farewell {user.Mention}!");
            }
        }

        public async Task OnUserJoin(SocketGuildUser user)
        {
            if (user.IsBot) return;
            var usr = GetUser(user.Id);
            if (usr != null)
            {
                var channel = await Garden.TheFriendTree.GetTextChannelAsync(Garden.JoinChannel);
                try
                {
                    await channel.SendMessageAsync($"Welcome {user.Mention} back to The Friend Tree!");
                    await GiveRoles(user.Id);
                }
                catch
                {

                }
            }
            else
            {
                await user.AddRoleAsync(user.Guild.GetRole(Garden.NotConnectedRole));
                try
                {
                    await user.SendMessageAsync("**Welcome to The Friend Tree!**\n" +
                                                "\n" +
                                                "Please send me a **Tree Code** to join the server.\n" +
                                                "A **Tree Code** looks like `T-123-123-123`.\n" +
                                                "Just message me in this DM to connect your account to the Tree!");
                    Garden.TreeState.UsersConnecting.Add(user.Id);
                }
                catch
                {

                }
            }

        }

        
        public async Task OnUserMessageAsync(SocketMessage message)
        {
            var result = Garden.TreeCodeMatcher.Match(message.Content);
            try
            {
                await RegisterUser(message.Author, result.Value);
            }
            catch
            {

            }
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
                    if (Garden.TreeState.InviteMap.ContainsKey(inviteCode))
                    {
                        var inviter = Garden.TreeState.Users[Garden.TreeState.InviteMap[inviteCode]];
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
                        userObj.InvitedBy = inviter.TreeId;
                        userObj.Friends.Add(inviter.TreeId);
                        int newUserIndex = Garden.TreeState.Users.Count;
                        userObj.TreeId = newUserIndex;

                        Garden.TreeState.Users.Add(userObj);

                        Garden.TreeState.UserMap[user.Id] = userObj.TreeId;

                        inviter.FriendsInvited.Add(newUserIndex);
                        inviter.Friends.Add(newUserIndex);

                        Garden.TreeState.UsersConnecting.Remove(user.Id);

                        var channel = await Garden.TheFriendTree.GetTextChannelAsync(Garden.JoinChannel);

                        await channel.SendMessageAsync($"Welcome {user.Mention} to The Friend Tree! Please read #about for more info!");

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
            if (!Garden.TreeState.UserMap.ContainsKey(uid)) return null;
            return Garden.TreeState.Users[Garden.TreeState.UserMap[uid]];
        }

        public string GetTreeCodeFormatted(int code)
        {
            return
                $"T-{(code / 100000000) % 10}{(code / 10000000) % 10}{(code / 1000000) % 10}" +
                $"-{(code / 100000) % 10}{(code / 10000) % 10}{(code / 1000) % 10}" +
                $"-{(code / 100) % 10}{(code / 10) % 10}{(code) % 10}";
        }

        public UserObject CreateUser(ulong uid)
        {
            var obj = new UserObject {UserId = uid, Friends = new HashSet<int>(), FriendsInvited = new HashSet<int>()};
            var rng = new Random();
            int code = rng.Next(0, 999999999);
            while (Garden.TreeState.InviteMap.ContainsKey(code))
            {
                code = rng.Next(0, 999999999);
            }

            obj.InviteTreeCode = code;
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
