using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BalloonSuite.CustomerExport.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using BalloonSuite.Api.Common.Requests;
using BalloonSuite.Api.Common.Objects;
using BalloonSuite.NetworkCommunication;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace BalloonSuite.CustomerExport.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly List<SlimWebsite> _websites;

    public HomeController(ILogger<HomeController> logger)
    {
      _logger = logger;
      _websites = this.GetSlimWebsites();
    }

    public IActionResult Index()
    {
      var model = new IndexViewModel
      {
         Customers = _websites
      };

      return View(model);
    }

    public IActionResult Images(int id)
    {
      byte[] response = null;
      var website = this.GetWebsite(id).Value;
      var csv = new List<ImageExportCSV>();

      if (website == default)
      {
        return Content($"Website {id} did not return any data");
      }

      if (website.Images != null)
      {
        foreach (var image in website.Images)
        {
          csv.Add(new ImageExportCSV
          {
            Name = image.Name,
            Url = image.Url
          });
        }
      }

      if (website.WebsiteImages != null)
      {
        foreach (var image in website.WebsiteImages)
        {
          csv.Add(new ImageExportCSV
          {
            Name = image.FileName,
            Url = image.Url
          });
        }
      }

      using (var memoryStream = new MemoryStream())
      using (var streamWriter = new StreamWriter(memoryStream))
      using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
      {
        csvWriter.WriteRecords(csv);
        streamWriter.Flush();

        response = memoryStream.ToArray();
      }

      var domainName = website.DomainName
        .Replace(" ", "-")
        .Replace("https://", "")
        .Replace("http://", "")
        .Replace("https", "")
        .Replace("http:", "")
        .Replace("www.", "")
        .Replace("www", "").ToLower();

      var fileName = $"{domainName}_Image_Export.csv";

      return File(response, "text/csv", fileName);
    }

    public IActionResult DownloadPageImages(int websiteId, int pageId)
    {
      byte[] response = null;
      var website = this.GetWebsite(websiteId).Value;
      var page = this.GetPageBuilderById(new GetPageByIdRequest { PageId = pageId, WebsiteId = websiteId }).Value;
      var csv = new List<ImageExportCSV>();

      foreach (var section in page.PageSections.Where(x => x.Type == Api.Common.PageSectionType.Features))
      {
        if(section.FeaturesSection.Features != default)
        {
          foreach (var feature in section.FeaturesSection.Features)
          {
            if(feature.Image != default)
            {
              csv.Add(new ImageExportCSV
              {
                Name = feature.Image.FileName,
                Url = feature.Image.Url
              });
            }
          }
        }
      }

      foreach (var section in page.PageSections.Where(x => x.Type == Api.Common.PageSectionType.Image))
      {
        if(section.ImageSection != default && section.ImageSection.Image != default)
        {
          csv.Add(new ImageExportCSV
          {
            Name = section.ImageSection?.Image.FileName ?? "",
            Url = section.ImageSection?.Image.Url ?? ""
          });

        }
      }

      foreach (var section in page.PageSections.Where(x => x.Type == Api.Common.PageSectionType.Gallery))
      {
        if(section.GallerySection.Gallery != default)
        {
          foreach (var image in section.GallerySection.Gallery.Images)
          {
            if(image.Image != default)
            {
              csv.Add(new ImageExportCSV
              {
                Name = image.Image?.Name ?? "",
                Url = image.Image?.Url ?? ""
              });
            }
          }
        }
      }

      using (var memoryStream = new MemoryStream())
      using (var streamWriter = new StreamWriter(memoryStream))
      using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
      {
        csvWriter.WriteRecords(csv);
        streamWriter.Flush();

        response = memoryStream.ToArray();


        var domainName = website.DomainName
          .Replace(" ", "-")
          .Replace("https://", "")
          .Replace("http://", "")
          .Replace("https", "")
          .Replace("http:", "")
          .Replace("www.", "")
          .Replace("www", "").ToLower();

        var fileName = $"{domainName}_Page_{page.Title}_Images_Export.csv";

        return File(response, "text/csv", fileName);
      }
    }

    public IActionResult DownloadPages(int id)
    {
      byte[] response = null;
      var website = this.GetWebsite(id);
      var pages = this.GetPageBuilders(id);
      var csv = new List<PageExportCSV>();

      foreach (var page in pages.Value)
      {
        csv.Add(new PageExportCSV
        {
          Title = page.Title,
          Url = page.Url,
          Status = page.Status.ToString(),
          DateModified = page.DateModified,
          SeoDescription = page.SeoDescription,
          SeoTitle = page.SeoTitle
        });
      }

      using (var memoryStream = new MemoryStream())
      using (var streamWriter = new StreamWriter(memoryStream))
      using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
      {
        csvWriter.WriteRecords(csv);
        streamWriter.Flush();

        response = memoryStream.ToArray();
      }

      var domainName = website.Value.DomainName
        .Replace(" ", "-")
        .Replace("https://", "")
        .Replace("http://", "")
        .Replace("https", "")
        .Replace("http:", "")
        .Replace("www.", "")
        .Replace("www", "").ToLower();

      var fileName = $"{domainName}_Page_Export.csv";

      return File(response, "text/csv", fileName);
    }

    public IActionResult Pages(int id)
    {
      var website = this.GetWebsite(id);
      var pages = this.GetPageBuilders(id);
      var csv = new List<PageExportCSV>();
      ViewData["websiteId"] = id;

      if (pages == default || !pages.IsSuccess)
      {
        return Content($"Website {id} did not return any data");
      }

      return View(pages.Value);
    }

    public IActionResult Page(int id)
    {
      var website = this.GetWebsite(id);
      var pages = this.GetPageBuilders(id).Value;
      var page = pages.FirstOrDefault(x => x.Id == id);
      var csv = new List<PageExportCSV>();

      if (page == default)
      {
        return Content($"Website {id} did not return any data");
      }

      return View(page);
    }

    public IActionResult Posts(int id)
    {
      byte[] response = null;
      var website = this.GetWebsite(id);
      var csv = new List<ProductExportCSV2>();

      if (website == default || !website.IsSuccess)
      {
        return Content($"Website {id} did not return any data");
      }

      foreach (var product in website.Value.Bouquets)
      {
        csv.Add(new ProductExportCSV2
        {
          Name = product.Title,
          Price = product.Price,
          Description = product.Description,
          ImageUrl = product?.Images.FirstOrDefault()?.Image?.Url ?? "",
          Enabled = !product.Disabled,
          Weight = product.Weight,
          Quantity = product.ProductDetails?.Quantity ?? 0,
          Category1 = product?.Categories.FirstOrDefault()?.Name ?? "",
          Category2 = product?.Categories.ElementAtOrDefault(1)?.Name ?? "",
          Category3 = product?.Categories.ElementAtOrDefault(2)?.Name ?? "",
        });
      }

      using (var memoryStream = new MemoryStream())
      using (var streamWriter = new StreamWriter(memoryStream))
      using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
      {
        csvWriter.WriteRecords(csv);
        streamWriter.Flush();

        response = memoryStream.ToArray();
      }

      var domainName = website.Value.DomainName
        .Replace(" ", "-")
        .Replace("https://", "")
        .Replace("http://", "")
        .Replace("https", "")
        .Replace("http:", "")
        .Replace("www.", "")
        .Replace("www", "").ToLower();

      var fileName = $"{domainName}_Product_Export.csv";

      return File(response, "text/csv", fileName);
    }

    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
