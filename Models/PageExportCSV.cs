using System;
namespace BalloonSuite.CustomerExport.Models
{
  public class PageExportCSV
  {
    public string Title { get; set; }
    public string Status { get; set; }
    public string Url { get; set; }
    public DateTime DateModified { get; set; }
    public string SeoTitle { get; set; }
    public string SeoDescription { get; set; }
  }
}
