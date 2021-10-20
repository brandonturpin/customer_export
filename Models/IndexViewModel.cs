using System;
using System.Collections.Generic;
using BalloonSuite.Api.Common.Objects;

namespace BalloonSuite.CustomerExport.Models
{
  public class IndexViewModel
  {
    public List<SlimWebsite> Customers { get; set; } = new List<SlimWebsite>();
  }
}
