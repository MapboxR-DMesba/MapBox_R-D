using GoogleMap.Data;
using GoogleMap.DBModels;
using GoogleMap.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
