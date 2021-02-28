using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.Json;
using netjson = System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace NetPatch
{
    public class PatchHelper
    {

        public static JsonPatchDocument GetPatchForObject(string originalJson, string currentJson)
        {
            JsonPatchDocument patch = new JsonPatchDocument();

            using (JsonDocument original = JsonDocument.Parse(originalJson))
            using (JsonDocument current = JsonDocument.Parse(currentJson))
            {
                FindPatchDiffs(original.RootElement.Clone(), current.RootElement.Clone(), patch, "/");
            }

            return patch;
        }

        // call one level down: path + currentProperty.Name + "/"
        // for this property: path + prop.Name
        private static void FindPatchDiffs(JsonElement original, JsonElement current, JsonPatchDocument patchDocument, string path)
        {
            if (original.ValueKind != JsonValueKind.Object && original.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("Value types should not require a recursive call");
            }

            // Start by checking for properties that have just been added or removed
            if (original.ValueKind == JsonValueKind.Object)
            {
                HashSet<string> currentPropSet = new HashSet<string>(current.EnumerateObject().Select(eo => eo.Name));
                HashSet<string> originalPropSet = new HashSet<string>(original.EnumerateObject().Select(eo => eo.Name));

                foreach (JsonProperty property in original.EnumerateObject())
                {
                    if (!currentPropSet.Contains(property.Name))
                    {
                        patchDocument.Remove($"{path}{property.Name}");
                    }
                }

                foreach (JsonProperty property in current.EnumerateObject())
                {
                    if (!originalPropSet.Contains(property.Name))
                    {
                        patchDocument.Add($"{path}{property.Name}", AddAsType(property.Value, patchDocument, current, property));
                    }
                    else
                    {
                        JsonElement originalPropertyValue = original.GetProperty(property.Name);

                        if (property.Value.ValueKind != originalPropertyValue.ValueKind)
                        {
                            // Different types => replace
                            patchDocument.Replace($"{path}{property.Name}", AddAsType(property.Value, patchDocument, current, property));
                        }

                        if (property.Value.GetRawText() != originalPropertyValue.GetRawText())
                        {
                            if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                            {
                                FindPatchDiffs(originalPropertyValue, property.Value, patchDocument, $"{path}{property.Name}/");
                            }
                            else
                            {
                                patchDocument.Replace($"{path}{property.Name}", AddAsType(property.Value, patchDocument, current, property));
                            }
                        }
                    }
                }
            }

            if (original.ValueKind == JsonValueKind.Array)
            {
                List<JsonElement> originalElements = original.EnumerateArray().Select(o => o).ToList();
                List<JsonElement> currentElements = current.EnumerateArray().Select(c => c).ToList();

                if (currentElements.Count() > originalElements.Count())
                {
                    for (int i = originalElements.Count(); i < currentElements.Count(); i++)
                    {
                        string pathIdentifier = i.ToString();
                        if (i == currentElements.Count() - 1){
                            pathIdentifier = "-";
                        }
                        patchDocument.Add($"{path}{pathIdentifier}", AddAsType(currentElements[i], patchDocument, null, null));
                    }
                }

                for (int i = 0; i < originalElements.Count(); i++)
                {
                    if (i > currentElements.Count())
                    {
                        patchDocument.Remove($"{path}{i}");
                    }

                    if (originalElements[i].GetRawText() != currentElements[i].GetRawText())
                    {
                        patchDocument.Replace($"{path}{i}", AddAsType(currentElements[i], patchDocument, null, null));
                    }
                }
            }

        }

        private static object AddAsType(JsonElement element, JsonPatchDocument patchDocument, JsonElement? parentObject, JsonProperty? property)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    return parentObject == null ? JObject.Parse(element.GetRawText()) : JObject.Parse(parentObject.Value.GetRawText()).Property(property.Value.Name).Value;
                case JsonValueKind.Array:
                    return parentObject == null ? JArray.Parse(element.GetRawText()) : JObject.Parse(parentObject.Value.GetRawText()).Property(property.Value.Name).Value;
                case JsonValueKind.Number:
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Undefined:
                    return null;
                default:
                    throw new Exception("Unsupported kind");
            }
        }
    }
}
