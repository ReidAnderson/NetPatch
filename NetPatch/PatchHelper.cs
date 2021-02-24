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

        public static JsonPatchDocument GeneratePatch(string originalJson, string currentJson)
        {
            JObject original = JObject.Parse(originalJson);
            JObject current = JObject.Parse(currentJson);

            JsonPatchDocument patch = new JsonPatchDocument();
            FillPatchForObject(original, current, patch, "/");

            return patch;
        }

        public static JsonPatchDocument GeneratePatch(object originalObject, object currentObject)
        {
            JObject original = JObject.FromObject(originalObject);
            JObject current = JObject.FromObject(currentObject);

            JsonPatchDocument patch = new JsonPatchDocument();
            FillPatchForObject(original, current, patch, "/");

            return patch;
        }

        public static JsonPatchDocument GetPatchForObject(string originalJson, string currentJson)
        {
            JsonPatchDocument patch = new JsonPatchDocument();

            JsonDocument original = JsonDocument.Parse(originalJson);
            JsonDocument current = JsonDocument.Parse(currentJson);
            
            // using (JsonDocument original = JsonDocument.Parse(originalJson))
            // using (JsonDocument current = JsonDocument.Parse(currentJson))
            // {
                FindPatchDiffs(original.RootElement, current.RootElement, patch, "/");
            // }

            return patch;
        }

        // call one level down: path + currentProperty.Name + "/"
        // for this property: path + prop.Name
        private static void FindPatchDiffs(JsonElement original, JsonElement current, JsonPatchDocument patchDocument, string path)
        {
            HashSet<string> currentPropSet = new HashSet<string>(current.EnumerateObject().Select(eo => eo.Name));
            HashSet<string> originalPropSet = new HashSet<string>(original.EnumerateObject().Select(eo => eo.Name));


            // Start by checking for properties that have just been added or removed
            if (original.ValueKind == JsonValueKind.Object)
            {
                foreach(JsonProperty property in original.EnumerateObject()) {
                    if (!currentPropSet.Contains(property.Name)) {
                        patchDocument.Remove($"{path}{property.Name}");
                    }
                }

                foreach(JsonProperty property in current.EnumerateObject()) {
                    if (!originalPropSet.Contains(property.Name)) {
                        AddAsType(property.Value, patchDocument, $"{path}{property.Name}", current, property);
                    }
                }
            }

        }

        private static void AddAsType(JsonElement element, JsonPatchDocument patchDocument, string path, JsonElement parentObject, JsonProperty property) {
            switch (element.ValueKind) {
                case JsonValueKind.Null:
                    patchDocument.Add(path, null);
                    break;
                case JsonValueKind.Object:
                    patchDocument.Add(path, JObject.Parse(parentObject.GetRawText()).Property(property.Name).Value);
                    break;
                case JsonValueKind.Array:
                    patchDocument.Add(path, JObject.Parse(parentObject.GetRawText()).Property(property.Name).Value);
                    break;
                case JsonValueKind.Number:
                    patchDocument.Add(path, element.GetDouble());
                    break;
                case JsonValueKind.True:
                    patchDocument.Add(path, true);
                    break;
                case JsonValueKind.False:
                    patchDocument.Add(path, false);
                    break;
                case JsonValueKind.String:
                    patchDocument.Add(path, element.GetString());
                    break;
                case JsonValueKind.Undefined:
                    patchDocument.Add(path, null);
                    break;
                default:
                    throw new Exception("Unsupported kind");
            }
        }

        static void FillPatchForObject(JObject original, JObject current, JsonPatchDocument patch, string path)
        {
            string[] originalNames = original.Properties().Select(x => x.Name).ToArray();
            string[] currentNames = current.Properties().Select(x => x.Name).ToArray();

            // Properties that have been removed
            foreach (string k in originalNames.Except(currentNames))
            {
                JProperty prop = original.Property(k);
                patch.Remove(path + prop.Name);
            }

            // Properties that have been added
            foreach (string k in currentNames.Except(originalNames))
            {
                JProperty prop = current.Property(k);
                patch.Add(path + prop.Name, prop.Value);
            }

            // Present in both
            foreach (string k in originalNames.Intersect(currentNames))
            {
                JProperty originalProperty = original.Property(k);
                JProperty currentProperty = current.Property(k);

                if (originalProperty.Value.Type != currentProperty.Value.Type)
                {
                    // If the types don't match, this must be a replace
                    patch.Replace(path + currentProperty.Name, currentProperty.Value);
                }
                else if (!string.Equals(
                                originalProperty.Value.ToString(Newtonsoft.Json.Formatting.None),
                                currentProperty.Value.ToString(Newtonsoft.Json.Formatting.None)))
                {
                    // If the JSON string representation is different, some kind of action is needed
                    if (originalProperty.Value.Type == JTokenType.Object)
                    {
                        // Recurse into objects
                        FillPatchForObject(originalProperty.Value as JObject, currentProperty.Value as JObject, patch, path + currentProperty.Name + "/");
                    }
                    else
                    {
                        // Replace values directly
                        patch.Replace(path + currentProperty.Name, currentProperty.Value);
                    }
                }
            }
        }
    }
}
