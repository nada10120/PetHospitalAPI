public class PostResponse
{
    public int PostId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }   // جديد
    public string Content { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }    // جديد
    public int CommentsCount { get; set; } // جديد
}
