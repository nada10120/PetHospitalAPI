using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;
using System.Security.Claims;
using Utility;

namespace PetHospitalApi.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;

        public CommunityController(IPostRepository postRepository, ICommentRepository commentRepository, IUserRepository userRepository, UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _userManager = userManager;
        }
        [HttpGet("GetAllPosts")]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _postRepository.GetAsync();
            var response = new List<PostResponse>();
            foreach (var post in posts)
            {
                var user = _userManager.FindByIdAsync(post.UserId);
                response.Add(new PostResponse
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    UserName = user.Result.UserName,
                    Content = post.Content,
                    MediaUrl = post.MediaUrl,
                    CreatedAt = post.CreatedAt,

                });

            }
            return Ok(response);
        }
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromForm] PostRequest postRequest)
        {
            if (postRequest == null)
            {
                return BadRequest("Post cannot be null");
            }
            if (string.IsNullOrEmpty(postRequest.Content))
            {
                return BadRequest("Content is required");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var post = new Post
            {
                Content = postRequest.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };
            if (postRequest.MediaUrl != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "posts");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(postRequest.MediaUrl.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await postRequest.MediaUrl.CopyToAsync(stream);

                post.MediaUrl = $"/images/posts/{fileName}";
            }
            var createdPost = await _postRepository.CreateAsync(post);
            if (createdPost == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating post");
            }
            return Ok(createdPost.Adapt<PostResponse>());
        }
        [HttpPut("EditPost/{postId}")]
        public async Task<IActionResult> EditPost([FromRoute] int postId, [FromForm] PostRequest postRequest)
        {
            if (postRequest == null)
            {
                return BadRequest("Post cannot be null");
            }
            if (string.IsNullOrEmpty(postRequest.Content))
            {
                return BadRequest("Content is required");
            }
            var existingPost = await _postRepository.GetOneAsync(p => p.PostId == postId);
            if (existingPost == null)
            {
                return NotFound("Post not found");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || existingPost.UserId != userId)
            {
                return Unauthorized("You are not authorized to edit this post");
            }
            existingPost.Content = postRequest.Content;
            if (postRequest.MediaUrl != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "posts");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(postRequest.MediaUrl.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await postRequest.MediaUrl.CopyToAsync(stream);
                existingPost.MediaUrl = $"/images/posts/{fileName}";
            }
            var updatedPost = await _postRepository.EditAsync(existingPost);
            if (updatedPost == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating post");
            }
            return Ok(updatedPost.Adapt<PostResponse>());
        }
        [HttpDelete("DeletePost/{postId}")]
        public async Task<IActionResult> DeletePost([FromRoute] int postId)
        {
            var existingPost = await _postRepository.GetOneAsync(p => p.PostId == postId);
            if (existingPost == null)
            {
                return NotFound("Post not found");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || existingPost.UserId != userId)
            {
                return Unauthorized("You are not authorized to delete this post");
            }
            var deleted = await _postRepository.DeleteAsync(existingPost);

            return Ok("Post deleted successfully");

        }
        [HttpPost("LikePost/{postId}")]
        public async Task<IActionResult> LikePost([FromRoute] int postId)
        {
            var post = await _postRepository.GetOneAsync(p => p.PostId == postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var hasLiked = await _postRepository.HasUserLikedPostAsync(postId, userId);
            if (hasLiked)
            {
                return BadRequest("You have already liked this post");
            }
            var like = new Like
            {
                PostId = postId,
                UserId = userId,
            };
            var addedLike = await _postRepository.AddLikeAsync(like);
            
            if (addedLike == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error liking post");
            }
            return Ok("Post liked successfully");
        }
        [HttpPost("AddComment/{postId}")]
        public async Task<IActionResult> AddComment([FromRoute] int postId, [FromBody] CommunityRequest communityRequest)
        {
            if (communityRequest == null || string.IsNullOrEmpty(communityRequest.Content))
            {
                return BadRequest("Comment content is required");
            }
            var post = await _postRepository.GetOneAsync(p => p.PostId == postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = communityRequest.Content,
                CreatedAt = DateTime.UtcNow
            };
            var addedComment = await _commentRepository.CreateAsync(comment);
            if (addedComment == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding comment");
            }
            return Ok(addedComment.Adapt<CommentResponse>());
        }
        [HttpGet("GetComments/{postId}")]
        public async Task<IActionResult> GetComments([FromRoute] int postId)
        {
            var post = await _postRepository.GetOneAsync(p => p.PostId == postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }
            var comments = await _commentRepository.GetAsync(c => c.PostId == postId);
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

    }
}