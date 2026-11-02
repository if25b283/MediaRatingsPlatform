namespace MRP.Domain
{
    public enum MediaType
    {
        Movie,
        Series,
        Game
    }

    internal class Media
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public MediaType Type { get; set; }
        public int ReleaseYear { get; set; }
        public List<string> Genres { get; set; } = new();
        public int AgeRestriction { get; set; }
        public int CreatorId { get; set; }
    }
}
