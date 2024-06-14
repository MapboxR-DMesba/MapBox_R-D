using GoogleMap.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GoogleMap.DBModels
{
    public class MatchingPolygon
    {
        [Key]
        public int Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Address { get; set; }
        public  string CenterCode { get; set; }
        public ICollection<LocationModel> PolygonPoints { get; set; }
    }
}
