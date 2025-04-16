using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.Core.ResponseBodies
{
    public struct PaginatedCelestialBodiesResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<CelestialBodies> Items { get; set; }
    }
}
