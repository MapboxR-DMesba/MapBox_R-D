using GoogleMap.Data;
using GoogleMap.DBModels;
using GoogleMap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using Newtonsoft.Json.Linq;

namespace GoogleMap.Controllers
{
    public class OpenStreetMapController : Controller
    {
        private readonly ILogger<GoogleMapController> _logger;
        private readonly AppDbContext _dBcontext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        public OpenStreetMapController(ILogger<GoogleMapController> logger, AppDbContext dBcontext, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _dBcontext = dBcontext;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient();
        }
        public IActionResult Index()
        {
            return View("Buildings");
        }
        public IActionResult CreateArea()
        {
            return View("MapMarking");
        }
        [HttpPost]
        public async Task<IActionResult> SavePolygon(PolygonModel polygon)
        {
            PolygonCenter polygonCenter = new PolygonCenter();
            polygonCenter.Lat = polygon.Center.Lat;
            polygonCenter.Lng = polygon.Center.Lng;
            polygonCenter.FormatedAddress = polygon.FormattedAddress;
            polygonCenter.UserAddress = polygon.UserInput;
            polygonCenter.NUmberOfPoints = polygon.Points.Count;
            polygonCenter.CenterCode = Guid.NewGuid().ToString();

            await _dBcontext.PolygonCenters.AddAsync(polygonCenter);
            await _dBcontext.SaveChangesAsync();

            // save points
            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();
            foreach(var point in polygon.Points) {
                PolygonPoint polygonPoint = new PolygonPoint()
                {
                    CenterId = polygonCenter.Id,
                    Lat = point.Lat,
                    Lng = point.Lng,

                };
                polygonPoints.Add(polygonPoint);    
            }
            await _dBcontext.AddRangeAsync(polygonPoints);
            await _dBcontext.SaveChangesAsync();

            // Save the polygon data to the database
            // You'll need to implement this logic
            // You can access polygon.center, polygon.points, polygon.userInput, polygon.formattedAddress here
            // Return appropriate response
            return Ok();
        }

        public async Task<IActionResult> GetPolygonByCordinate(double latitude, double longitude)
        {
            var point = new Point(longitude, latitude) { SRID = 4326 };

            // Retrieve all polygons from the database, including their points
            List<PolygonCenter> allPolygons = await _dBcontext.PolygonCenters.AsNoTracking().Include(p => p.PolygonPoints).ToListAsync();

            foreach (var polygon in allPolygons)
            {
                // Compute convex hull of the polygon points
                List<PolygonPoint> convexHullPoints = ComputeConvexHull(polygon.PolygonPoints.ToList());

                // Sort the convex hull points by longitude and latitude to ensure correct order
                //convexHullPoints.Sort((p1, p2) => p1.Lng.CompareTo(p2.Lng) != 0 ? p1.Lng.CompareTo(p2.Lng) : p1.Lat.CompareTo(p2.Lat));

                // Add the first point again at the end to close the polygon
                convexHullPoints.Add(convexHullPoints.First());

                // Convert PolygonPoints to NetTopologySuite Coordinate objects
                var polygonCoords = convexHullPoints.Select(p => new Coordinate(p.Lng, p.Lat)).ToArray();
                var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                var ntsPolygon = geometryFactory.CreatePolygon(polygonCoords);

                if (ntsPolygon.Contains(point))
                {

                    MatchingPolygon polygonDto = new MatchingPolygon
                    {
                        Id = polygon.Id,
                        Address = $"{polygon.UserAddress},{polygon.FormatedAddress}",
                        CenterCode = polygon.CenterCode,
                        PolygonPoints = polygon.PolygonPoints.Select(pp => new LocationModel{ Lat = pp.Lat, Lng = pp.Lng }).ToList()
                    };

                    return new JsonResult(new { success = true, result = polygonDto });
                }
            }

            return Json(new { success = false });
        }

        public async Task<IActionResult> GEtPolygonByAddress(string address)
        {
            if(string.IsNullOrEmpty(address))
                return Json(new { success = false });

            PolygonCenter? polygon = await _dBcontext.PolygonCenters.AsNoTracking().Include(p => p.PolygonPoints).FirstOrDefaultAsync(x => x.UserAddress == address);
            if(polygon == null)
                return Json(new { success = false });

            MatchingPolygon polygonDto = new MatchingPolygon
            {
                Id = polygon.Id,
                Address = $"{polygon.UserAddress},{polygon.FormatedAddress}",
                CenterCode = polygon.CenterCode,
                Lat = polygon.Lat,
                Lng = polygon.Lng,
                PolygonPoints = polygon.PolygonPoints.Select(pp => new LocationModel { Lat = pp.Lat, Lng = pp.Lng }).ToList()
            };

            return new JsonResult(new { success = true, result = polygonDto });
        }
        public  List<PolygonPoint> SortPolygonCoordinates(List<PolygonPoint> coordinates)
        {
            if (coordinates == null || coordinates.Count < 3)
                throw new ArgumentException("At least three coordinates are required to form a polygon.");

            var sortedCoordinates = coordinates.OrderBy(c => c.Lat).ThenBy(c => c.Lng).ToList();

            // Optional: Implement more sophisticated sorting if necessary, e.g., Graham scan for convex hull

            return sortedCoordinates;
        }
        private List<PolygonPoint> ComputeConvexHull(List<PolygonPoint> points)
        {
            points.Sort((p1, p2) => p1.Lng.CompareTo(p2.Lng) != 0 ? p1.Lng.CompareTo(p2.Lng) : p1.Lat.CompareTo(p2.Lat));

            List<PolygonPoint> lower = new List<PolygonPoint>();
            foreach (var point in points)
            {
                while (lower.Count >= 2 && CrossProduct(lower[lower.Count - 2], lower[lower.Count - 1], point) <= 0)
                {
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(point);
            }

            List<PolygonPoint> upper = new List<PolygonPoint>();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                var point = points[i];
                while (upper.Count >= 2 && CrossProduct(upper[upper.Count - 2], upper[upper.Count - 1], point) <= 0)
                {
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(point);
            }

            upper.RemoveAt(upper.Count - 1);
            lower.RemoveAt(lower.Count - 1);
            lower.AddRange(upper);

            return lower;
        }

        // Helper method to calculate cross product of vectors
        private double CrossProduct(PolygonPoint o, PolygonPoint a, PolygonPoint b)
        {
            return (a.Lat - o.Lat) * (b.Lng - o.Lng) - (a.Lng - o.Lng) * (b.Lat - o.Lat);
        }

       
        public async Task<IActionResult> GetBuildings()
        {
            try
            {
                var query = @"
                   [out:json];
                   area[name='New York City']->.searchArea;
                   (
                     way['building'](area.searchArea);
                   );
                   out body;
                ";

                var response = await _httpClient.GetStringAsync($"http://overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}");
                var parsedResponse = JObject.Parse(response);
                var elements = parsedResponse["elements"];
                var buildings = parsedResponse["elements"]
                    .Where(e => e["type"].ToString() == "way" && e["nodes"].HasValues)
                    .Select(e => new
                    {
                        Id = e["id"].ToString(),
                        BoundingBox = new
                        {
                            MinLat = e["bounds"]["minlat"].ToString(),
                            MaxLat = e["bounds"]["maxlat"].ToString(),
                            MinLon = e["bounds"]["minlon"].ToString(),
                            MaxLon = e["bounds"]["maxlon"].ToString()
                        }
                    });

                return Ok(buildings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
