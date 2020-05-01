using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace QualityImage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private static string getOk = "OK";
        private readonly ImageService _service;
        string _path = "";

        public ImageController(ImageService service, IWebHostEnvironment environment)
        {
            _service = service;
            _path = Path.Combine(environment.ContentRootPath, "Images");
            Directory.CreateDirectory(_path);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(getOk);
        }

        // Example: https://localhost:44303/image?quality=70&type=0
        [HttpPost]
        public IActionResult Post(IFormFile file, [FromQuery] long quality, [FromQuery] int type)
        {
            try
            {
                _service.quality = quality;

                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        ms.Position = 0L;

                        switch (type)
                        {
                            case 0:
                                _service.CoreCompact(ms, _path);
                                break;
                            case 1:
                                _service.MagicCompact(ms, _path);
                                break;
                            case 2:
                                _service.SharpCompactJPG(ms, _path);
                                break;
                            case 3:
                                _service.SharpCompactPNG(ms, _path);
                                break;
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("IOException source: {0}", ex.Message);
                return StatusCode(500);
            }
        }
    }
}
