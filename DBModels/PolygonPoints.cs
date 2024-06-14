using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoogleMap.DBModels
{
    public class PolygonPoint
    {
        [Key]
        public int Id { get; set; }
        public int CenterId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        [ForeignKey("CenterId")]
        public PolygonCenter PolygonCenter { get; set; }
    }
}
