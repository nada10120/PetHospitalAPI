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

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm]PostRequest post)
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
                CreatedAt = DateTime.UtcNow
            };
            if (post.MediaUrl != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "posts");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(post.MediaUrl.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await post.MediaUrl.CopyToAsync(stream);

                postEntity.MediaUrl = $"/images/posts/{fileName}";
            }
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
            if (post.MediaUrl != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "posts");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(post.MediaUrl.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await post.MediaUrl.CopyToAsync(stream);

                existingPost.MediaUrl = $"/images/posts/{fileName}";
            }
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
