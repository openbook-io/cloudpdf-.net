using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CloudPDF.NET
{
  public class CloudPDFResponse<TPayload>
  {
    public bool Success { get; set; }

    public Exception Exception { get; set; }

    public string ErrorMessage { get; set; }
    
    public TPayload Payload { get; set; }
  }


  public class CloudPDFUpload
  {
    public string Id {get; set; }
    public string Name { get; set; }
    
  }
  
  public class CloudPdf
  {
    private string cloudName;
    private string accessSecret;

    public CloudPdf(string cloudName, string accessSecret)
    {
      this.cloudName = cloudName;
      this.accessSecret = accessSecret;
    }
    
    /// <summary>
    /// Upload a pdf to your cloudpdf.io account
    /// </summary>
    /// <param name="file">A file stream or memory stream containing a byte array of the pdf you wish to upload.</param>
    /// <param name="fileName">Your desired filename. Should be suffixed with .pdf</param>
    /// <param name="expiry">The expiry date of the document</param>
    /// <param name="public">Whether the document should be uploaded as public or not</param>
    /// <param name="search">Whether the document should be searchable</param>
    /// <returns>A cloudPDFResponse with a payload containing the document id, if the upload was successful. If there was an error, payload will be null and success will be false.</returns>
    public async Task<CloudPDFResponse<CloudPDFUpload>> UploadDocument(Stream file, string fileName, DateTime expiry, bool @public, bool search)
    {
      try
      {
        var claims = new[]
        {
          new Claim("type", "upload"),
          new Claim("public", @public.ToString().ToLowerInvariant(),ClaimValueTypes.Boolean),
          new Claim("search", @search.ToString().ToLowerInvariant(),ClaimValueTypes.Boolean)
        };

        var uploadToken = GetToken(claims, expiry);

        using var form = new MultipartFormDataContent();

        using var memoryStream = new MemoryStream();
      
        file.CopyTo(memoryStream);
        var bytes =  memoryStream.ToArray();
        using var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "file", fileName);

        var client = new HttpClient{};
      
        //client.DefaultRequestHeaders.Add("Content-Type","application/json");
        client.DefaultRequestHeaders.Add("v-authorization",uploadToken);
        var response = await client.PostAsync("https://api.cloudpdf.io/api/upload", form);
      
        var content = await response.Content.ReadAsStringAsync();
      
        if (response.IsSuccessStatusCode)
        {
          var upload = JsonConvert.DeserializeObject<CloudPDFUpload>(content);
          if (upload != null)
          {
            return new  CloudPDFResponse<CloudPDFUpload>() {Success = true, Payload = upload};
          }
          return new  CloudPDFResponse<CloudPDFUpload>() {Success = false, ErrorMessage = "The document uploaded successfully but we could not deserialize the response. Please contact cloudpdf with an error report."};
        }

        var error = JsonConvert.DeserializeObject<CloudPDFUploadError>(content);
      
        return new CloudPDFResponse<CloudPDFUpload>
        {
          ErrorMessage = error?.Error ?? "Unknown Error",
          Success = false
        };
      }
      catch (Exception e)
      {
        return new CloudPDFResponse<CloudPDFUpload>()
        {
          Exception = e,
          ErrorMessage = e.Message,
          Success = false
        };
      }
    }

    private string GetToken(Claim[] claims, DateTime expiresUTC)
    {
      var handler = new JsonWebTokenHandler();
       
      var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessSecret)) {KeyId = cloudName};

      var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

      // create JWT
      var token = handler.CreateToken(new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        SigningCredentials = signingCredentials,
        Expires = expiresUTC
      });

      //// validate JWT
      var result = handler.ValidateToken(token, new TokenValidationParameters
      {
        IssuerSigningKey = symmetricKey,
        ValidateAudience = false,
        ValidateIssuer = false
      });

      if (!result.IsValid)
      {
        // go boom? //
      }

      return token;
    }
    public CloudPDFResponse<string> GetViewingToken(string documentId, DateTime expiry, bool download = false, bool search = false)
    {
      try
      {
        var claims = new[]
        {
          new Claim("type", "get-document"),
          new Claim("documentId", documentId),
          new Claim("download", download.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean),
          new Claim("search", search.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean)
        };

        var token = GetToken(claims, expiry);

        if (!string.IsNullOrEmpty(token))
        {
          return new CloudPDFResponse<string>
          {
            Payload = token,
            Success = true
          };
        }

        return new CloudPDFResponse<string>()
        {
          ErrorMessage = "Could not create token",
          Success = false
        };
      }
      catch (Exception e)
      {
        return new CloudPDFResponse<string>()
        {
          ErrorMessage = e.Message,
          Exception = e,
          Success = false
        };
      }
    }

  }

  public class CloudPDFUploadError
  {
    public string Error { get; set; }
  }
}
