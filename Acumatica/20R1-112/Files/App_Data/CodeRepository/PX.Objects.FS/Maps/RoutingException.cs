using System;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.FS
{
  /// <summary>
  /// Exception thrown if a request to generate a route between locations fails.
  /// </summary>
  public class RoutingException : Exception
  {
    internal RoutingException(string message) : base(message)
    {
    }
  }
}
