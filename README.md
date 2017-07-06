The **DeleteUnusedPolicies** tool works with the [Azure Media Services Client SDK for .Net](https://docs.microsoft.com/en-us/dotnet/api/microsoft.windowsazure.mediaservices.client?view=azuremediaservices-3.8.0.5) that you can download from [nuget](https://www.nuget.org/packages/windowsazure.mediaservices).

This tool removes unused access policies from your Media Services account. There is a limit of 1 million access policies in an account. This application deletes all access policies that are not associated with an existing locator.

Usage:
`DeleteUnusedPolicies.exe <AAD tenant domain> <REST API endpoint>`

Example:
`DeleteUnusedPolicies.exe microsoft.onmicrosoft.com https://accountname.restv2.westcentralus.media.azure.net/API`

The application first builds a list of policies that are associated with locators.  Then it enumerates through all policies and compares them against the first list. Policies that do not have associated locators get added to a delete list.  Finally it deletes the policies in the second list.

Authentication for the application is done with Azure Active Directory.  For info on how to use that with Azure Media Services see [Access the Azure Media Services API with Azure AD authentication](https://docs.microsoft.com/en-us/azure/media-services/media-services-use-aad-auth-to-access-ams-api).
