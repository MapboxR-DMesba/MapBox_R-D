using GoogleMap.Data;
using GoogleMap.DBModels;
using GoogleMap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace GoogleMap.Controllers
{
    public class GoogleMapController : Controller
    {
        private readonly ILogger<GoogleMapController> _logger;
        private readonly AppDbContext _dBcontext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        public GoogleMapController(ILogger<GoogleMapController> logger, AppDbContext dBcontext, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _dBcontext = dBcontext;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SearchBox()
        {
            return View();
        }
        public IActionResult MarkBuilding()
        {
            return View();
        }
        public IActionResult CreateArea()
        {
            return View("MapMarking");
        }
        public IActionResult MarkArea()
        {
            return View("LocationDetection");
        }
        [HttpPost]
        public async Task<IActionResult> SavePolygon(PolygonModel polygon)
        {
            string shortCode = MyLast(polygon.ShortCode,2);
            shortCode+= MyLast(polygon.UserInput , 2 );
            shortCode += GenerateRandomPassword();
            PolygonCenter polygonCenter = new PolygonCenter();
            polygonCenter.Lat = polygon.Center.Lat;
            polygonCenter.Lng = polygon.Center.Lng;
            polygonCenter.FormatedAddress = polygon.FormattedAddress;
            polygonCenter.UserAddress = polygon.UserInput;
            polygonCenter.NUmberOfPoints = polygon.Points.Count;
            polygonCenter.CenterCode = shortCode;
            polygonCenter.CountryCode = MyLast(polygon.CountryCode,2);

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
            return Ok(polygonCenter.CenterCode);
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

            PolygonCenter? polygon = await _dBcontext.PolygonCenters.AsNoTracking().Include(p => p.PolygonPoints).FirstOrDefaultAsync(x => x.UserAddress == address || x.FormatedAddress == address || x.CenterCode == address);
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

        public async Task<IActionResult> CheckPOintInPOlygonOrNot(double latitude, double longitude)
        {
            var point = new Point(longitude, latitude) { SRID = 4326 };

            // Retrieve all polygons from the database, including their points
            List<PolygonCenter> allPolygons = await _dBcontext.PolygonCenters.AsNoTracking().Include(p => p.PolygonPoints).ToListAsync();

            foreach (var polygon in allPolygons)
            {
                // Compute convex hull of the polygon points
                if(polygon.PolygonPoints.Any() && polygon.PolygonPoints.First().Lat == polygon.PolygonPoints.Last().Lat && polygon.PolygonPoints.First().Lng == polygon.PolygonPoints.Last().Lng){
                    var lastPoints = polygon.PolygonPoints.Last();
                    polygon.PolygonPoints.Remove(lastPoints);   
                }
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
                        PolygonPoints = polygon.PolygonPoints.Select(pp => new LocationModel { Lat = pp.Lat, Lng = pp.Lng }).ToList()
                    };

                    return new JsonResult(new { isFound = true, result = polygonDto });
                }
            }

            return Json(new { isFound = false });
        }
        public async Task<IActionResult> GetPolygonCode(double latitude, double longitude)
        {
            var point = new Point(longitude, latitude) { SRID = 4326 };

            // Retrieve all polygons from the database, including their points
            List<PolygonCenter> allPolygons = await _dBcontext.PolygonCenters.AsNoTracking().Include(p => p.PolygonPoints).ToListAsync();

            foreach (var polygon in allPolygons)
            {
                // Compute convex hull of the polygon points
                if (polygon.PolygonPoints.Any() && polygon.PolygonPoints.First().Lat == polygon.PolygonPoints.Last().Lat && polygon.PolygonPoints.First().Lng == polygon.PolygonPoints.Last().Lng)
                {
                    var lastPoints = polygon.PolygonPoints.Last();
                    polygon.PolygonPoints.Remove(lastPoints);
                }
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

                   

                    return new JsonResult(new { code = $"{polygon.CenterCode}-{polygon.UserAddress}-{polygon.CountryCode}-{latitude},{longitude}" });
                }
            }

            return new JsonResult(new { code = $"{latitude},{longitude}" });
        }
        public async Task<IActionResult> GetSugeestedItem(string searchText)
        {
          

            // Retrieve all polygons from the database, including their points
            List<PolygonCenter> suggestedItem = await _dBcontext.PolygonCenters.AsNoTracking().Where(x => x.CenterCode.StartsWith(searchText) || x.UserAddress.StartsWith(searchText)).Take(10).ToListAsync();


            return Json(new { items = suggestedItem });
        }
        public  string MyLast(string str, int length)
        {
            if (str == null)
                return "00";
            else if (str.Length >= length)
                return str.Substring(0, length);
            else
                return str;
        }
        public static string GenerateRandomPassword()
        {
            Random random = new Random();
            string password = "";

            // Generate one random number (digit)
            password += random.Next(0, 10);

            // Generate one random uppercase letter (ASCII 65-90)
            password += (char)random.Next(65, 91);

            // Generate one random lowercase letter (ASCII 97-122)
            password += (char)random.Next(97, 123);

            // Shuffle the characters randomly to mix them up
            password = ShuffleString(password);

            return password;
        }

        public static string ShuffleString(string str)
        {
            char[] array = str.ToCharArray();
            Random random = new Random();
            int n = array.Length;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                char value = array[k];
                array[k] = array[n];
                array[n] = value;
            }

            return new string(array);
        }
    }
}
