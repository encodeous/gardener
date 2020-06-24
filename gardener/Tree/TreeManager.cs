﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using gardener.Utilities;
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
                        user.SendMessageAsync("**Sorry for this late message, it seems like you have joined right when I restarted for an upgrade. You may now connect to the server!\n**").Forget();
                    }
                    catch
                    {

                    }
                    await Garden.Tree.OnUserJoin(user as SocketGuildUser);
                }

            }
        }

        public async Task OnUserLeave(SocketGuildUser user)
        {
            var usr = GetUser(user.Id);
            if (usr != null)
            {
                var channel = await Garden.TheFriendTree.GetTextChannelAsync(Garden.JoinChannel);
                channel.SendMessageAsync($"Farewell {user.Mention}, hope you return!").Forget();
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
                    channel.SendMessageAsync($"Welcome {user.Mention} back to The Friend Tree!").Forget();
                    
                }
                catch (Exception e)
                {

                }
                await GiveRoles(user.Id);
            }
            else
            {
                foreach (var k in user.Roles)
                {
                    try
                    {
                        await user.RemoveRoleAsync(k);
                    }
                    catch
                    {

                    }

                }

                await user.AddRoleAsync(user.Guild.GetRole(Garden.NotConnectedRole));
                try
                {
                    user.SendMessageAsync("**Welcome to The Friend Tree!**\n" +
                                                "\n" +
                                                "Please send me a **Tree Code** to join the server.\n" +
                                                "A **Tree Code** looks like `T-123-123-123`.\n" +
                                                "Just message me in this DM to connect your account to the Tree!").Forget();
                }
                catch
                {

                }
                Garden.TreeState.UsersConnecting.Add(user.Id);
            }
        }

        
        public async Task OnUserMessageAsync(SocketMessage message, bool privateChannel)
        {
            try
            {
                var result = Garden.TreeCodeMatcher.Match(message.Content);
                if (privateChannel)
                {
                    if (!result.Success || string.IsNullOrEmpty(result.Value))
                    {
                        message.Author.SendMessageAsync("**Please double check your Tree Code!**").Forget();
                    }
                    else
                    {
                        await RegisterUser(message.Author, result.Value);
                    }
                }
                else
                {
                    if (result.Success && !string.IsNullOrEmpty(result.Value))
                    {
                        await message.DeleteAsync();
                        await RegisterUser(message.Author, result.Value);
                    }
                }
            }
            catch
            {

            }
        }

        public async Task RegisterUser(SocketUser user, string code)
        {
            try
            {
                int inviteCode = ParseCode(code);
                if (Garden.TreeState.InviteMap.ContainsKey(inviteCode))
                {
                    var inviter = Garden.TreeState.Users[Garden.TreeState.InviteMap[inviteCode]];
                    var inviteUser = await Garden.TheFriendTree.GetUserAsync(inviter.UserId).ConfigureAwait(false);

                    var userObj = CreateUser(user.Id);
                    userObj.InvitedBy = inviter.TreeId;
                    userObj.Friends.Add(inviter.TreeId);
                    int newUserIndex = Garden.TreeState.Users.Count;
                    userObj.TreeId = newUserIndex;

                    Garden.TreeState.Users.Add(userObj);

                    Garden.TreeState.UserMap[user.Id] = userObj.TreeId;

                    Garden.TreeState.InviteMap[userObj.InviteTreeCode] = userObj.TreeId;

                    inviter.FriendsInvited.Add(newUserIndex);

                    inviter.Friends.Add(newUserIndex);

                    Garden.TreeState.UsersConnecting.Remove(user.Id);

                    var channel = await Garden.TheFriendTree.GetTextChannelAsync(Garden.JoinChannel);

                    channel.SendMessageAsync($"Welcome {user.Mention} to The Friend Tree! Please read <#721095701882470491> for more info!").Forget();

                    await GiveRoles(user.Id);

                    Task.Run(() =>
                    {
                        int id = newUserIndex;
                        while (id != Garden.TreeState.Users[id].InvitedBy)
                        {
                            id = Garden.TreeState.Users[id].InvitedBy;
                            Garden.TreeState.Users[id].TotalPeopleInvited++;
                            int cid = id;
                            Task.Run(() => { PeopleInvitedChanged(cid); });
                        }
                    }).Forget();

                    if (inviteUser != null)
                    {
                        user.SendMessageAsync("Your account has been successfully " +
                                              $"linked to the server using {DsUtils.GetDiscordUsername(inviteUser.Id)}'s code.").Forget();
                    }
                    else
                    {
                        user.SendMessageAsync("Your account has been successfully linked to the server.").Forget();
                    }
                }
                else
                {
                    user.SendMessageAsync("**The Tree Code you have entered is not valid.**").Forget();
                }
            }
            catch(Exception e)
            {
                e.Log("Error while processing tree code");
                user.SendMessageAsync("**An error occurred when processing your Tree Code, please try again.**").Forget();
            }
        }

        public static void PeopleInvitedChanged(int id)
        {
            // to do (invite roles)
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
            int value = (ParseChar(code[2])) * 10;
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
