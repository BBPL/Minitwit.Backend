﻿using Infrastructure.Interfaces;
using Minitwit.DataAccessLayer;
using Minitwit.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class TimelineRepository : ITimelineRepository
    {
        public async Task<IEnumerable<Message>> GetPublicTimeline()
        {
            using(var ctx = new CustomDbContext())
            {
                return await ctx.Messages.Where(m => !m.Flagged).ToListAsync();
            }
        }

        public async Task<IEnumerable<Message>> GetTimelineForLoggedInUser(Guid userId)
        {
            using (var ctx = new CustomDbContext())
            {
                return await ctx.Messages.
                Join(ctx.Followers,
                m => m.User.UserId,
                f => f.Self.UserId,
                (m, f) => new { Message = m, Follower = f }).
                Where(elem => elem.Message.UserId == userId && elem.Follower.Self.UserId == userId)
                .Select(elem => elem.Message).ToListAsync();
            }
        }

        public async Task<IEnumerable<Message>> GetUserTimeline(string username)
        {
            using (var ctx = new CustomDbContext())
            {
                return await ctx.Messages.Include(m => m.User).Where(m => !m.Flagged && m.User.Username == username).ToListAsync();
            }
        }

        public async Task PostMessage(Guid userId, string message)
        {
            using (var ctx = new CustomDbContext())
            {
                var user = await ctx.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    throw new ArgumentException();
                var msg = new Message
                {
                    Content = message,
                    Flagged = false,
                    User = user,
                    UserId = userId,
                    PublishedTime = DateTime.Now,
                };
                ctx.Messages.Add(msg);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
