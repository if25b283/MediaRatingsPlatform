using MRP.Data;
using MRP.Domain;

namespace MRP.Business
{
    internal class MediaService
    {
        private readonly MediaRepository repository = new();

        public List<Media> GetAll()
        {
            return repository.GetAll();
        }

        public Media? GetById(int id)
        {
            return repository.GetById(id);
        }

        public int AddMedia(Media media)
        {
            ValidateMedia(media);
            return repository.AddMedia(media);
        }

        public bool UpdateMedia(Media media)
        {
            ValidateMedia(media);
            return repository.UpdateMedia(media);
        }

        public bool DeleteMedia(int id, int creatorId)
        {
            return repository.DeleteMedia(id, creatorId);
        }

        private static void ValidateMedia(Media media)
        {
            if (string.IsNullOrWhiteSpace(media.Title))
                throw new ArgumentException("Title is required");

            if (!Enum.IsDefined(media.Type))
                throw new ArgumentException("Invalid media type");

            if (media.ReleaseYear < 1888 ||
                media.ReleaseYear > DateTime.UtcNow.Year + 5)
                throw new ArgumentException("Invalid release year");

            if (media.AgeRestriction < 0 || media.AgeRestriction > 18)
                throw new ArgumentException("Invalid age restriction");
        }
    }
}
