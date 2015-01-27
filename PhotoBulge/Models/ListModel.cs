using System;
using System.Net.Http;

namespace PhotoBulge.Models
{
    public class ListModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ImageModel[] Images { get; set; }
    }

    public class ImageModel
    {
        public string Name { get; set; }        
    }
}