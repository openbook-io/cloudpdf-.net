using System;
using CloudPDF.NET;

namespace CloudPDFTestRunner
{
  class Program
  {
    static void Main(string[] args)
    {
      var token = CloudPdf.GetViewingToken(
        "",
        "",
        "",
        new DateTime(2021, 12, 09),
        true, true);

      Console.WriteLine(token);
    }
  }
}
