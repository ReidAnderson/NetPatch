# NetPatch

An implementation to provide RFC6902 valid JSON patch documents from .NET 5. Will return the patch either as a Microsoft.AspNetCore.JsonPatch JsonPatchDocument, or it's JSON serialized string equivalent.

A very early, alpha-stage package. Please report any bugs or performance issues.

## JSON Packages

Unfortunately System.Text.Json doesn't support ExpandoObject/dynamic at the moment. One of the primary benefits of JSON patch is that it provides audit and diff capabilities without requiring an udnerstanding of the underlying type. As a result that means that we can't rely solely on the core System.Text provided JSON utilities. That package is used where possible to try and keep up to date with the direction .NET is headed for JSON tooling, but there is still a dependency on Newtonsoft.Json through the Microsoft.AspNetCore.Mvc.NewtonsoftJson package until a time where System.Text.Json can operated without specified typing.
