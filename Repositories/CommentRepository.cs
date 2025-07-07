using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager;
using Models;
using Repositories.IRepository;

namespace Repositories
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        private readonly ApplicationDbContext context;
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        // Additional methods specific to CommentRepository can be added here
    
    }
}
