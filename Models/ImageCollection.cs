using System;
using BalloonSuite.Api.Common.Objects;

namespace BalloonSuite.CustomerExport.Models
{
  public class ImageCollection
  {
    public ImageDto Image { get; set; }
    public WebsiteImageDto WebsiteImage { get; set; }
    public ImageCollectionType Type { get; set; }
  }

  public enum ImageCollectionType
  {
    unknown = 0,
    image = 1,
    websiteImage = 2
  }
}
