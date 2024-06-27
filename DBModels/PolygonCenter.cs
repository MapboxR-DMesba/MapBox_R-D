using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GoogleMap.DBModels
{
    public class PolygonCenter
    {
        [Key]
        public int Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string FormatedAddress { get; set; }
        public string UserAddress { get; set; }
        public int NUmberOfPoints { get; set; }
        public  string CenterCode { get; set; }
        public string CountryCode { get; set; } 
        public ICollection<PolygonPoint> PolygonPoints { get; set; }
    }
}
