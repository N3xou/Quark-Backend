﻿using Quark_Backend.DAL;
using Quark_Backend.Entities;
using Quark_Backend.Utilities;
using Quark_Backend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Query;

namespace Quark_Backend.Hubs
{
    public class QuarkHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            User user;
            using (var db = new QuarkDbContext())
            {
                user = await db.Users
                    .Include(u => u.Connections)
                    .FirstOrDefaultAsync(u => u.Username == Context.User.Identity.Name);//does token generation (ClaimType.Name) set Identity.Name value properly?
                if (user == null)
                    return;
                // var httpContext = Context.GetHttpContext();
                // var headers = httpContext.Request.Headers;
                user.Connections.Add(
                    new Connection
                    {
                        Id = int.Parse(Context.ConnectionId),//TODO: add migration to change Id to ConnectionId that is string
                        State = true,//unnecessary field
                        // UserAgent = headers["User-Agent"],//or ["user-agent"]
                        User = user
                    });
                try
                {
                    await db.SaveChangesAsync();
                }
                catch(DbUpdateException exception)
                {
                    return;
                }
            }
            foreach (var conversation in user.Conversations)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, conversation.Name);
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            User user;
            int connectionId = int.Parse(Context.ConnectionId);
            using(var db = new QuarkDbContext())
            {
                var connection = await db.Connections.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == connectionId);
                user =  connection.User;
                user.Connections.Remove(connection);//because connectionId is different everytime user connects to application
                try
                {
                    await db.SaveChangesAsync();
                }
                catch(DbUpdateException updateException)//should base.OnDisconnectedAsync be called before?
                {
                    return;
                }
            }
            foreach (var conversation in user.Conversations)
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, conversation.Name);
            }
            await base.OnDisconnectedAsync(exception);
        }
        /*
            potential methods:
            - StartPrivateConversation (check if not started). Probably AddToConversation will be enough.

        */


        // public async Task<Conversation> OpenPrivateConversation(string requestingUser, string otherUser)
        // {
        //     using(var db = new QuarkDbContext())
        //     {
        //         db.Conversations.Include(c => c.Messages).Include(c => c.Users)
        //             .Where(c => c.Users.Count == 2)
        //             .Where(c => c.Users.)
        //     }
        // }

        public async Task OpenConversation(string conversationName) //alternative name: GetConversation
        {
            ConversationMessagesModel conversationModel;
            using(var db = new QuarkDbContext())
            {
                Conversation conversation = await db.Conversations
                    .Include(c => c.Messages).ThenInclude(m => m.User)
                    .FirstOrDefaultAsync(c => c.Name == conversationName);
                if(conversation is null)
                    return;
                conversationModel = new ConversationMessagesModel();
                foreach(var message in conversation.Messages)
                {
                    conversationModel.Messages.Add
                    (
                        new ConversationMessagesModel.Message
                        {
                            Username = message.User.Username,
                            Text = message.Text,
                            Date = message.SentDate
                        }
                    );
                }
            }
            await Clients.Caller.SendAsync("ShowConversation", conversationModel);
            //invoke ShowConversation frontend method
        }

        public async Task InitiatePrivateConversation(string username) // alternative name: AddToPrivateConversation
        {
            string newConversationName;
            using(var db = new QuarkDbContext())
            {
                // var users = await db.Users.ToListAsync();
                // var userA = users.FirstOrDefault(u => u.Username == username);
                // var userB = users.FirstOrDefault(u => u.Username == Context.User.Identity.Name);
                var mutualConversations = await db.Conversations.Include(c => c.Users).Where(c => c.Users.Any(u => u.Username == username) && c.Users.Any(u => u.Username == Context.User.Identity.Name)).ToListAsync();
                var bothUsers = db.Users.Include(u => u.Connections).Where(u => u.Username == username || u.Username == Context.User.Identity.Name);
                bool isPrivate = false;
                foreach(var conversation in mutualConversations)
                {
                    if(conversation.Users.Count == 2)
                    {
                        isPrivate = true;// could be: return
                        break;
                    }
                }
                if(isPrivate) //has private conversation -> dont make room/conversation
                    return; // maybe send conversation name?
                newConversationName = NameGenerator.GenerateRandomConversationName();
                var newConversation = new Conversation
                {
                    Name = newConversationName
                };
                db.Conversations.Add(newConversation);
                foreach(var user in bothUsers)
                {
                    foreach(var connection in user.Connections)
                    {
                        var connectionId = connection.Id.ToString();
                        await Groups.AddToGroupAsync(connectionId, newConversationName);
                    }
                    newConversation.Users.Add(user);
                }
                await db.SaveChangesAsync();
            }
            Clients.Caller.SendAsync("InitiatePrivateConversationHandler", newConversationName);//change name of method
        }

        //should initiating new conversation be implemented in seperated method?
        public async Task AddToConversation(string conversationName, string username)
        {
            string currentConversationName = conversationName;
            using(var db = new QuarkDbContext())
            {
                var conversation = await db.Conversations.Include(c => c.Users).FirstOrDefaultAsync(c => c.Name == conversationName);
                if(conversation is null)
                    return;
                if(conversation.Users.FirstOrDefault(u => u.Username == username) != null)
                    return;//user is already in conversation

                var userToAdd = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
                var usersCount = conversation.Users.Count;
                if(usersCount <= 2) //create new room to prevent adding new member to existing private conversation
                {
                    //generate roomHash/roomName that will be possible to change into custom name
                    string randomName;
                    while(true)
                    {
                        randomName = NameGenerator.GenerateRandomConversationName();
                        if(db.Conversations.FirstOrDefault(c => c.Name == randomName) == null)
                        {
                            break;
                        }
                    }
                    List<User> newConversationMembers = new List<User>(conversation.Users);
                    newConversationMembers.Add(userToAdd);
                    var newConversation = new Conversation
                    {
                        Name = randomName,
                        Users = newConversationMembers
                    };
                    db.Conversations.Add(newConversation);
                    await db.SaveChangesAsync();
                    foreach(var user in newConversation.Users)
                    {
                        foreach(var connection in user.Connections)
                        {
                            var connectionId = connection.Id.ToString();
                            await Groups.AddToGroupAsync(connectionId, conversation.Name);
                        }
                    }
                }
                else //it means it's already a group conversation
                {
                    foreach(var connection in userToAdd.Connections)
                    {
                        var connectionId = connection.Id.ToString();
                        await Groups.AddToGroupAsync(connectionId, conversation.Name);
                    }
                    conversation.Users.Add(userToAdd);
                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch(DbUpdateException)
                    {
                        return;
                    }
                }
            }
            await Clients.Caller.SendAsync("UpdateConversations", currentConversationName);
        }
        public async Task BroadcastUser(User user)
        {
            await Clients.All.SendAsync("ReceiveUser", user);
        }

        // should work for both "private" and "group" messages
        public async Task SendMessage(string text, string username, string conversationName)
        {
            using(var db = new QuarkDbContext())
            {
                //TODD: add TimeOnly property to Message entity and do MIGRATION
                Conversation conversation = await db.Conversations.Include(c => c.Messages).FirstOrDefaultAsync();
                if(conversation is null) 
                    return;
                User user = db.Users.FirstOrDefault(u => u.Username == username);
                if(user is null)
                    return;
                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                Message message = new Message
                {
                    SentDate = currentDate,
                    User = user,
                    Conversation = conversation,
                    Text = text
                };
                conversation.Messages.Add(message);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {

                }
                finally
                {
                    await Clients.Group(conversationName).SendAsync("ReceiveMessage", username, text);
                }
            }  
            // await Clients.All.SendAsync("ReceiveMessage", message, username);
        }
    }
}
