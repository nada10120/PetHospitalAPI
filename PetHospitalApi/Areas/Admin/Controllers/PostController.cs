using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepository;
using Models.DTOs.Response;
using Models;
using Models.DTOs.Request;
using Microsoft.AspNetCore.Identity;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<User> _userManager;

        public PostController(IPostRepository postRepository, UserManager<User> userManager)
        {
            _postRepository = postRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postRepository.GetAsync();
            return Ok(posts.Adapt<IEnumerable<PostResponse>>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var post = await _postRepository.GetOneAsync(x => x.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post.Adapt<PostResponse>());
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostRequest post)
        {
            if (post == null)
            {
                return BadRequest("Post cannot be null");
            }
            if (string.IsNullOrEmpty(post.Content))
            {
                return BadRequest("Content is required");
            }
            if (string.IsNullOrEmpty(post.UserId))
            {
                return BadRequest("UserId is required");
            }

            var user = await _userManager.FindByIdAsync(post.UserId);
            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            var postEntity = new Post
            {
                UserId = post.UserId,
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepository.CreateAsync(postEntity);
            return CreatedAtAction(nameof(GetById), new { id = postEntity.PostId }, postEntity.Adapt<PostResponse>());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PostRequest post)
        {
            if (post == null)
            {
                return BadRequest("Post cannot be null");
            }
            if (string.IsNullOrEmpty(post.Content))
            {
                return BadRequest("Content is required");
            }
            if (string.IsNullOrEmpty(post.UserId))
            {
                return BadRequest("UserId is required");
            }
            var existingPost = await _postRepository.GetOneAsync(x => x.PostId == id);
            if (existingPost == null)
            {
                return NotFound();
            }
            existingPost.Content = post.Content;
            existingPost.MediaUrl = post.MediaUrl;
            await _postRepository.EditAsync(existingPost);
            return Ok(existingPost.Adapt<PostResponse>());
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var post = await _postRepository.GetOneAsync(x => x.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            await _postRepository.DeleteAsync(post);
            return NoContent();
        }
    }
}
