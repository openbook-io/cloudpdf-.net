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

namespace CloudPDF.NET
{
  public class CloudPDFUploadResponse
  {
    public bool Success { get; set; }
    public string Error { get; set; }
    
    public CloudPDFUpload Upload { get; set; }
    
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
    
    public async Task<CloudPDFUploadResponse> UploadDocument(Stream file, string fileName, DateTime expiry, bool @public, bool search)
    {
      var claims = new[]
      {
        new Claim("type", "upload"),
        new Claim("public", @public.ToString(),ClaimValueTypes.Boolean)
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
        var upload = Newtonsoft.Json.JsonConvert.DeserializeObject<CloudPDFUpload>(content);
        return new CloudPDFUploadResponse() {Success = true, Upload = upload};
      }

      var error = Newtonsoft.Json.JsonConvert.DeserializeObject<CloudPDFUploadError>(content);
      
      return new CloudPDFUploadResponse
      {
        Error = error?.Error ?? "Unknown Error",
        Success = false
      };
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
    public string GetViewingToken(string documentId, DateTime expiry, bool download = false, bool search = false)
    {
      var claims = new[]
      {
        new Claim("type", "get-document"),
        new Claim("documentId", documentId),
        new Claim("download", download.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean),
        new Claim("search", search.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean)
      };

      return GetToken(claims, expiry);
    }

  }

  public class CloudPDFUploadError
  {
    public string Error { get; set; }
  }
}
