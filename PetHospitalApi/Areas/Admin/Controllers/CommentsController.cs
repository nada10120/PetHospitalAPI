using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPostRepository _postRepository;

        public CommentsController(ICommentRepository commentRepository, UserManager<User> userManager, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _postRepository = postRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var comments = await _commentRepository.GetAsync();

            var response = new List<CommentResponse>();

            foreach (var comment in comments)
            {
                var user = await _userManager.FindByIdAsync(comment.UserId);
                response.Add(new CommentResponse
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    UserName = user?.UserName, // رجع اليوزر نيم
                    PostId = comment.PostId
                });
            }

            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var comment = await _commentRepository.GetOneAsync(x => x.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CommentRequest commentRequest)
        {
            // Validate the request model
            if (!ModelState.IsValid)
            {

                return BadRequest(ModelState);
            }

            // Check if the post exists
            var post = await _postRepository.GetOneAsync(e => e.PostId == commentRequest.PostId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            // Check if the user exists
            var user = await _userManager.FindByIdAsync(commentRequest.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Create the comment entity
            var comment = new Comment
            {
                UserId = commentRequest.UserId,
                Content = commentRequest.Content,
                PostId = (int)commentRequest.PostId,
                CreatedAt = DateTime.UtcNow
            };

            // Save the comment
            var comment1 =await _commentRepository.CreateAsync(comment);
            return Ok(comment1.Adapt<CommentResponse>());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CommentRequest commentRequest)
        {
            // Validate the request model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if the comment exists
            var comment = await _commentRepository.GetOneAsync(x => x.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }
            // Update the comment properties
            comment.Content = commentRequest.Content;
            comment.PostId = (int)commentRequest.PostId;
            // Save the changes
            await _commentRepository.EditAsync(comment);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            // Check if the comment exists
            var comment = await _commentRepository.GetOneAsync(x => x.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }
            // Delete the comment
            await _commentRepository.DeleteAsync(comment);
            return NoContent();
        }
    }
}
