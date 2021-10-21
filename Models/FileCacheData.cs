using System;
namespace BalloonSuite.CustomerExport.Models
{
  public class FileCacheData
  {
    public Guid Guid { get; set; }
    public string Path { get; set; }
    public bool Ready { get; set; }
  }
}
