﻿namespace GoogleMap.Models
{
    public class PolygonModel
    {
        public LocationModel Center { get; set; }
        public List<LocationModel> Points { get; set; }
        public string UserInput { get; set; }
        public string FormattedAddress { get; set; }
        public string ShortCode { get; set; }   
        public string CountryCode { get; set; }
    }
}
