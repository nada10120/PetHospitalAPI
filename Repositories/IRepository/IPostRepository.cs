using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Repositories.IRepository
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<int> GetLikeCountAsync(int postId);
        Task<Like> GetLikeAsync(int postId, string userId);
        Task<bool> HasUserLikedPostAsync(int postId, string userId);
        Task<Like?> AddLikeAsync(Like like);
    }
}
