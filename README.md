# cloudpdf-.net

A .netstandard 2.0 utility nuget package for uploading documents to your cloudpdf.io account, and for retrieving a token for viewing a document.

### Installation

Install the nuget package with the CLI by running the command ```Install-Package CloudPDF.io -Version 1.0.0```

### Dependencies

NETStandard.Library 2.0.3
Microsoft.IdentityModel.JsonWebTokens 6.8.0
Newtonsoft.Json 12.0.3

### Usage

See the example console application "CloudPDFTestRunner" for usage details. Please note, the example project is written using .net 5.0 syntax, with ```await var using``` and other modern .net syntax.

The nuget package is backwards compatible with .net framework and .netstandard 2.0 projects, but the syntax you will need to write to run the upload/get token code will be slightly different depending on the version of .net you are targetting.
