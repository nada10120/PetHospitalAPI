namespace PetHospitalApi.DTOs.Response
{
    internal class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
        public bool Status { get; set; }
    }
}