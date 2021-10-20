using System;
namespace BalloonSuite.CustomerExport.Models
{
  public class ProductExportCSV2
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Category1 { get; set; }
    public string Category2 { get; set; }
    public string Category3 { get; set; }
    public double Weight { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool Enabled { get; set; }
  }
}
