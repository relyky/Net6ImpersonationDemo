using Microsoft.Extensions.Configuration;
using Net6ImpersonationDemo.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net6ImpersonationDemo;

internal class App
{
  readonly IConfiguration _config;
  readonly RandomService _randSvc;

  public App(IConfiguration config, RandomService randSvc)
  {
    _config = config;
    _randSvc = randSvc;
  }

  public void Run(string[] args)
  {
    try
    {
      // Check the identity.  
      Console.WriteLine("Before impersonation: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);

      Impersonator.RunImpersonated("userName", "domain", "password", id =>
      {
        // User action  
        Console.WriteLine("During impersonation: " + id.Name);

        ///file:///C:/Temp/SRS%20Sample.html
        FileInfo src = new FileInfo(@"C:/Temp/Sample.html");
        FileInfo tgt = new FileInfo(@"\\192.168.0.199\d$\SHARE\Sample.html");

        src.CopyTo(tgt.FullName, true);

        //\\192.168.0.67\d$
      });

      // Check the identity again.  
      Console.WriteLine("After impersonation: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
    }
    catch (Exception ex)
    {
      // Check the identity again.  
      Console.WriteLine("Exception: " + ex.Message);
    }
  }
}
