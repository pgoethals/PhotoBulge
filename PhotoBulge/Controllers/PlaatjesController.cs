using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;
using Ionic.Zip;
using PhotoBulge.Models;

namespace PhotoBulge.Controllers
{
    public class PlaatjesController : ApiController
    {
        [HttpGet]
        [Route("list/{folder}")]
        public IHttpActionResult List(string folder)
        {
            folder = "ardjan2015";

            string path = Path.Combine(ConfigurationManager.AppSettings["ImagesFolder"], folder);

            return Ok(new ListModel()
            {
                Name = "ardjan2015",
                Description = "La Vaux - januari 2015",
                Images = Directory.GetFiles(path, "img*.*")
                            .Select(x => new ImageModel
                            {
                                Name = Path.GetFileName(x),                                
                            }).ToArray()
            });
        }

        [HttpGet]
        [Route("image/{folder}/{name}/{size}")]
        public HttpResponseMessage Get(string folder, string name, string size)
        {
            string imgPath = Path.Combine(ConfigurationManager.AppSettings["ImagesFolder"], folder, name);
            using(var imageStream = new MemoryStream())
            using (var image = Image.FromFile(imgPath))
            {
                if (size == "full")
                {
                    image.Save(imageStream, ImageFormat.Jpeg);
                }
                else if (size == "small")
                {
                    double factor = 200/(double)image.Width;
                    int height = (int) ((double)image.Height*factor);
                    using (var thumb = image.GetThumbnailImage(200, height, () => true, new IntPtr()))
                    {
                        thumb.Save(imageStream, ImageFormat.Jpeg);
                    }
                }
                
                if (imageStream.Length > 0)
                {                    
                    var response= new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(imageStream.ToArray()),                        
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    return response;
                }
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }


        [HttpPost]
        [Route("download")]
        public HttpResponseMessage Download(DownloadModel downloadModel)
        {
            string imgPath = Path.Combine(ConfigurationManager.AppSettings["ImagesFolder"], downloadModel.Folder);
            var images = downloadModel.Images.Split(new[] {','});
            
            using (var zipStream = new MemoryStream())
            using (var zipFile = new ZipFile())
            {
                
                foreach (var image in images)
                {
                    zipFile.AddFile(Path.Combine(imgPath, image), "");
                    
                }
                zipFile.Save(zipStream);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(zipStream.ToArray()),
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("photos")
                {
                    FileName = downloadModel.Folder + ".zip"
                };
                return response;
            }
        }


        [HttpGet]
        [Route("folders")]
        public IHttpActionResult GetFolders()
        {
            var path = ConfigurationManager.AppSettings["ImagesFolder"];
            var folders = from dir in Directory.GetDirectories(path)
                from file in Directory.GetFiles(dir, "description.txt")
                select new
                {
                    Name = Path.GetFileName(dir),
                    Description = File.ReadAllText(file)
                };
            return Ok(folders);
        }
        
    }
}
