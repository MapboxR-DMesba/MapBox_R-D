using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace GoogleMap.Models
{
    public class Location
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Lat { get; set; }
        [Required]
        public string Lng { get; set; }
        public string? PlaceName { get; set; }
        public string? Addresses { get; set; }
        public string? Street { get; set; }
        public string? Neighborhood { get; set; }
        public string? Locality { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? LocationCode { get; set; }
        [Required]
        public string PlaceId { get; set; }
    }

    public class LocationDto
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string PlaceName { get; set; }
        public string Addresses { get; set; }
        public string Street { get; set; }
        public string Neighborhood { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string PlaceId { get; set; }
    }
     
}      



