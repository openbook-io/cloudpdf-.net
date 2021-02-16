using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CloudPDF.NET
{
  public static class CloudPdf
  {
    public static string GetViewingToken(string cloudName, string accessSecret, string documentId, DateTime expiry, bool download = false, bool search = false)
    {
      var handler = new JsonWebTokenHandler();
       
      var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessSecret)) {KeyId = cloudName};

      var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

      // create JWT
      var token = handler.CreateToken(new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[]
        {
          new Claim("type", "get-document"),
          new Claim("documentId",documentId),
          new Claim("download",download.ToString().ToLowerInvariant(),ClaimValueTypes.Boolean),
          new Claim("search",search.ToString().ToLowerInvariant(),ClaimValueTypes.Boolean)
        }),
        SigningCredentials = signingCredentials,
        Expires = expiry
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

  }
}
