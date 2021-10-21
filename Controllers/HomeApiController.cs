using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BalloonSuite.Api.Common.Objects;
using BalloonSuite.Api.Common.Requests;
using BalloonSuite.CustomerExport.Models;
using BalloonSuite.NetworkCommunication;
using LazyCache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BalloonSuite.CustomerExport.Controllers
{
  [Route("api/file")]
  public class HomeApiController : Controller
  {
    private readonly IAppCache _lazyCache;
    private readonly IWebHostEnvironment _environment;

    public HomeApiController(IAppCache cache, IWebHostEnvironment environment)
    {
      _lazyCache = cache;
      _environment = environment;
    }

    // GET: api/values
    [HttpGet]
    [Route("PrepareImagesForDownload/{websiteId}")]
    public IActionResult PrepareImagesForDownload(int websiteId)
    {
      var imageData = _lazyCache.Get<FileCacheData>(websiteId.ToString());

      if(imageData != default)
      {
        return new JsonResult(imageData);
      }

      var guid = Guid.NewGuid();
      var newData = new FileCacheData
      {
        Guid = guid,
        Ready = false
      };

      this.ProcessImages(guid, websiteId);

      this._lazyCache.Add(websiteId.ToString(), newData);
      return new JsonResult(newData);
    }

    private async Task ProcessImages(Guid guid, int websiteId)
    {
      WebClient client = new WebClient();
      var website = this.GetWebsite(websiteId).Value;
      var path = Path.Combine(this._environment.WebRootPath, "image-downloads");
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      var filePath = Path.Combine(path, guid.ToString() + ".zip");
      using (var compressedFileStream = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.Write))
      {
        using (ZipArchive zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, leaveOpen: true))
        {
          if (website.Images != default)
          {
            foreach (var image in website.Images)
            {
              if (image != null && !string.IsNullOrEmpty(image.Url))
              {
                Uri uri = new Uri(image.Url);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                byte[] fileBytes = client.DownloadData(image.Url);
                var zipEntry = zipArchive.CreateEntry(filename);
                using (var entryStream = new MemoryStream(fileBytes))
                using (var zipEntryStream = zipEntry.Open())
                {
                  entryStream.CopyTo(zipEntryStream);
                }
              }
            }
          }

          if (website.WebsiteImages != default)
          {
            foreach (var image in website.WebsiteImages)
            {
              if (image != null && !string.IsNullOrEmpty(image.Url))
              {
                Uri uri = new Uri(image.Url);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                byte[] fileBytes = client.DownloadData(image.Url);
                var zipEntry = zipArchive.CreateEntry(filename);
                using (var entryStream = new MemoryStream(fileBytes))
                using (var zipEntryStream = zipEntry.Open())
                {
                  entryStream.CopyTo(zipEntryStream);
                }
              }
            }
          }
        }
      }

      var cacheFile = this._lazyCache.Get<FileCacheData>(websiteId.ToString());
      cacheFile.Path = filePath;
      cacheFile.Ready = true;
      this._lazyCache.Add(websiteId.ToString(), cacheFile);
    }


  private ApiResponse<WebsiteDto> GetWebsite(int id)
  {
    using (var client = this.GetClient())
    {
      var url = $"https://api.balloonsuite.com/api/portal/GetWebsite";
      var request = new GetWebsiteRequest { WebsiteId = id };
      var response = client.Post<ApiResponse<WebsiteDto>, GetWebsiteRequest>(url, request).Result;
      return response;
    }
  }

  private ApiResponse<List<PageBuilderDto>> GetPageBuilders(int id)
  {
    using (var client = this.GetClient())
    {
      var url = $"https://api.balloonsuite.com/api/pages/get/{id}";
      var response = client.Get<ApiResponse<List<PageBuilderDto>>>(url).Result;
      return response;
    }
  }

  public List<SlimWebsite> GetSlimWebsites()
  {
    using (var client = this.GetClient())
    {
      var url = $"https://api.balloonsuite.com/api/portal/GetSlimWebsites";
      var response = client.Get<ApiResponse<List<SlimWebsite>>>(url).Result;
      return response.Value;
    }
  }

  public ApiResponse<PageBuilderDto> GetPageBuilderById(GetPageByIdRequest request)
  {
    using (var client = this.GetClient())
    {
      var url = $"https://api.balloonsuite.com/api/pages/getPageById";
      var response = client.Post<ApiResponse<PageBuilderDto>, GetPageByIdRequest>(url, request).Result;
      return response;
    }
  }

  private HttpClient GetClient()
  {
    var client = new HttpClient();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Add("X-Token", "48B88B74-63F5-4D85-B3D9-A99FFED0DC32");
      return client;
  }
}

}