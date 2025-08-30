using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.IRepository;

namespace Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        private readonly ApplicationDbContext context;
        public PostRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Like?> AddLikeAsync(Like like)
        {
            // check if user already liked the post
            var existingLike = await context.Likes
                .FirstOrDefaultAsync(l => l.PostId == like.PostId && l.UserId == like.UserId);

            if (existingLike != null)
            {
                return null; // user already liked this post
            }

            await context.Likes.AddAsync(like);
            await context.SaveChangesAsync();
            return like;
        }

        public async Task<Like?> GetLikeAsync(int postId, string userId)
        {
            return await context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await context.Likes
                .CountAsync(l => l.PostId == postId);
        }

        public async Task<bool> HasUserLikedPostAsync(int postId, string userId)
        {
            return await context.Likes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }


    }
}
