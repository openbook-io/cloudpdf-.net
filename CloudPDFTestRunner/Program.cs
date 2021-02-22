using System;
using System.IO;
using System.Threading.Tasks;
using CloudPDF.NET;

namespace CloudPDFTestRunner
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var cloudPDF = new CloudPdf("You-Cloud-Name", "Your-Access-Secret");

      var token = cloudPDF.GetViewingToken(
        "Your-Document-Id",
        new DateTime(2021, 12, 09),
        true, true);
      
      Console.WriteLine(token.Payload);

      var fileName = @"Your-File-Location.pdf"; 

      await using FileStream fs =  File.OpenRead(fileName); // the user would get the stream from somewhere, maybe the cloud or his computer
      
      var response = await cloudPDF.UploadDocument(fs, "Your-File-Name.pdf", DateTime.UtcNow.AddYears(1), true, true);
      
      Console.WriteLine(response.Payload.Id);
    }
  }
}
