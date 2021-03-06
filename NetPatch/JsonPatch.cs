﻿using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.Json;
using netjson = System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Dynamic;

namespace NetPatch
{
    public enum Operation
    {
        Add,
        Remove
    }

    public class JsonPatch
    {

        public static JsonPatchDocument GetPatch(string originalJson, string currentJson)
        {
            JsonPatchDocument patch = new JsonPatchDocument();

            using (JsonDocument original = JsonDocument.Parse(originalJson))
            using (JsonDocument current = JsonDocument.Parse(currentJson))
            {
                FindPatchDiffs(original.RootElement.Clone(), current.RootElement.Clone(), patch, "/");
            }

            return patch;
        }

        public static string GetPatchString(string originalJson, string currentJson)
        {
            JsonPatchDocument patchDocument = GetPatch(originalJson, currentJson);

            return Newtonsoft.Json.JsonConvert.SerializeObject(patchDocument);
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
                        patchDocument.Add($"{path}{property.Name}", AddAsType(property.Value, patchDocument));
                    }
                    else
                    {
                        JsonElement originalPropertyValue = original.GetProperty(property.Name);

                        if (property.Value.ValueKind != originalPropertyValue.ValueKind)
                        {
                            // Different types => replace
                            patchDocument.Replace($"{path}{property.Name}", AddAsType(property.Value, patchDocument));
                        }

                        if (property.Value.GetRawText() != originalPropertyValue.GetRawText())
                        {
                            if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                            {
                                FindPatchDiffs(originalPropertyValue, property.Value, patchDocument, $"{path}{property.Name}/");
                            }
                            else
                            {
                                patchDocument.Replace($"{path}{property.Name}", AddAsType(property.Value, patchDocument));
                            }
                        }
                    }
                }
            }

            if (original.ValueKind == JsonValueKind.Array)
            {
                List<JsonElement> originalElements = original.EnumerateArray().Select(o => o).ToList();
                List<JsonElement> currentElements = current.EnumerateArray().Select(c => c).ToList();

                Tuple<int?, Operation?> isAddOrRemove = IsAddOrRemove(originalElements, currentElements);
                if (isAddOrRemove.Item2 == Operation.Add && isAddOrRemove.Item1.HasValue)
                {
                    patchDocument.Add($"{path}{isAddOrRemove.Item1}", AddAsType(currentElements[isAddOrRemove.Item1.Value], patchDocument));
                    return;
                }
                else if (isAddOrRemove.Item2 == Operation.Remove && isAddOrRemove.Item1.HasValue)
                {
                    patchDocument.Remove($"{path}{isAddOrRemove.Item1}");
                    return;
                }

                if (currentElements.Count() > originalElements.Count())
                {
                    for (int i = originalElements.Count(); i < currentElements.Count(); i++)
                    {
                        string pathIdentifier = i.ToString();
                        if (i == currentElements.Count() - 1)
                        {
                            pathIdentifier = "-";
                        }
                        patchDocument.Add($"{path}{pathIdentifier}", AddAsType(currentElements[i], patchDocument));
                    }
                }

                for (int i = 0; i < originalElements.Count(); i++)
                {
                    if (i >= currentElements.Count())
                    {
                        patchDocument.Remove($"{path}{i}");
                        continue;
                    }

                    if (originalElements[i].GetRawText() != currentElements[i].GetRawText())
                    {
                        if (originalElements[i].ValueKind != JsonValueKind.Object && originalElements[i].ValueKind != JsonValueKind.Array) {
                            patchDocument.Replace($"{path}{i}", AddAsType(currentElements[i], patchDocument));
                        }
                        else {
                            FindPatchDiffs(originalElements[i], currentElements[i], patchDocument, $"{path}{i}/");
                        }
                    }
                }
            }
        }

        private static object AddAsType(JsonElement element, JsonPatchDocument patchDocument)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    // Unfortunately System.Text.Json doesn't support ExpandoObject at the moment
                    // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0#types-without-built-in-support
                    return JsonConvert.DeserializeObject<ExpandoObject>(element.GetRawText());
                case JsonValueKind.Array:
                    return JArray.Parse(element.GetRawText());
                case JsonValueKind.Number:
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new ObjectToInferredTypesConverter());
                    return netjson.JsonSerializer.Deserialize<object>(element.GetRawText(), options);
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

        private static List<int> GetShiftArray(List<JsonElement> originalElements, List<JsonElement> currentElements)
        {
            List<int> shiftFromOriginal = new List<int>();
            for (int i = 0; i < originalElements.Count(); i++)
            {
                for (int j = 0; j < currentElements.Count(); j++)
                {
                    if (originalElements[i].GetRawText() == currentElements[j].GetRawText())
                    {
                        shiftFromOriginal.Add(j - i);
                    }
                }
            }

            return shiftFromOriginal;
        }

        private static Tuple<int?, Operation?> IsAddOrRemove(List<JsonElement> originalElements, List<JsonElement> currentElements)
        {
            bool hasChanged = false;
            int? changepoint = null;
            Operation? op = null;

            List<int> shiftArray = GetShiftArray(originalElements, currentElements);

            for (int i = 1; i < shiftArray.Count(); i++)
            {
                if (shiftArray[i] > 0)
                {
                    op = Operation.Add;
                }
                else if (shiftArray[i] < 0)
                {
                    op = Operation.Remove;
                }

                if (shiftArray[i - 1] != shiftArray[i])
                {
                    if (hasChanged)
                    {
                        return new Tuple<int?, Operation?>(null, null);
                    }
                    else
                    {
                        changepoint = i;
                        hasChanged = true;
                    }
                }
            }

            if (hasChanged)
            {
                return new Tuple<int?, Operation?>(changepoint, op);
            }
            else
            {
                return new Tuple<int?, Operation?>(null, null);
            }
        }
    }

    public class ObjectToInferredTypesConverter : netjson.Serialization.JsonConverter<object>
    {
        public override object Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
                JsonTokenType.String => reader.GetString(),
                _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
            };

        public override void Write(
            Utf8JsonWriter writer,
            object objectToWrite,
            JsonSerializerOptions options) =>
            netjson.JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
    }
}
