using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
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
