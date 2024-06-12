using GoogleMap.Data;
using GoogleMap.DBModels;
using GoogleMap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using static System.Net.WebRequestMethods;

namespace GoogleMap.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _dBcontext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, AppDbContext dBcontext, IHttpClientFactory httpClientFactory)
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

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Map()
        {
            return View();  
        }
        public IActionResult MapInsert()
        {
            return View();
        }
        [HttpPost]
        public async Task< IActionResult> HandleMapClick(LocationDto data)
        {
            // Extract the coordinates from the JSON data

            bool isLocationSaved = await _dBcontext.Locations.AnyAsync(x => x.PlaceId == data.PlaceId);
            // Perform your logic with the received coordinates
            // For example, save to database or process further
            if(!isLocationSaved)
            {
                Location loc = new Location()
                {
                    Lat = data.Latitude.ToString(),
                    Lng = data.Longitude.ToString(),
                    PlaceName = data.PlaceName,
                    Addresses = data.Addresses,
                    Street = data.Street,
                    Neighborhood = data.Neighborhood,
                    Locality = data.Locality,
                    Region = data.Region,
                    Country = data.Country,
                    LocationCode = $"{data.Latitude}+{data.Longitude}",
                    PlaceId = data.PlaceId
                };
                await _dBcontext.Locations.AddAsync(loc);
                await _dBcontext.SaveChangesAsync();
            }
           
            //loc.

            // get boundary info
            //var httpClient = _httpClientFactory.CreateClient();
            //var accessToken = "pk.eyJ1IjoibWVzYmF1bGhhc2FuIiwiYSI6ImNsd3pjMGc5cjA1bHIyanFwOGlib3JtNmMifQ.BYcuMdFSAA3bjnTVN9LGEw";
            ////var url = $"https://api.mapbox.com/boundaries/v1/retrieve/{data.PlaceId}.json?access_token={accessToken}";
            //var url = $"https://api.mapbox.com/v4/mapbox.enterprise-boundaries-a0-v2/tilequery/{data.Longitude},{data.Latitude}.json?access_token= {accessToken}";

            //try
            //{
            //    var response = await httpClient.GetAsync(url);
            //    response.EnsureSuccessStatusCode();
            //    var content = await response.Content.ReadAsStringAsync();
            //    return Content(content, "application/json");
            //}
            //catch (HttpRequestException ex)
            //{
            //    return BadRequest(ex.Message);
            //}

            return Json(new { message = $"Coordinates saved long: {data.Longitude} lat: {data.Latitude} and code : {data.PlaceId}",  });
        }
        [HttpGet]
        public async Task<ActionResult> GetLocationByPlaceId(string placeId)
        {
           
            var location = await _dBcontext.Locations.FirstOrDefaultAsync(x => x.PlaceId == placeId);
            if(location == null) return Json(new { message = "not found", });
            return Json(new { message = "data found", locationDetails = location });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("reverse/{longitude},{latitude}")]
        public async Task<IActionResult> GetReverseGeocode(double longitude, double latitude)
        {
            var url = $"https://api.mapbox.com/search/v1/reverse/{longitude},{latitude}?access_token=pk.eyJ1IjoibWVzYmF1bGhhc2FuIiwiYSI6ImNsd3pjMGc5cjA1bHIyanFwOGlib3JtNmMifQ.BYcuMdFSAA3bjnTVN9LGEw&language=en&limit=1&types=address";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Origin", "http://your-custom-origin.com"); // Set your custom origin here

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return Content(content, "application/json");
        }


    }
}
