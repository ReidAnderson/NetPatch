using System;
using Xunit;
using NetPatch;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace NetPatchTests
{
    public class NetPatchTests
    {
        private bool PatchRoundTripMatches(string originalJson, string currentJson, string expectedPatch = null)
        {
            JsonPatchDocument patch = PatchHelper.GetPatchForObject(
                        originalJson,
                        currentJson);

            var originalObj = JsonConvert.DeserializeObject(originalJson);

            var currentObj = JsonConvert.DeserializeObject(originalJson);
            patch.ApplyTo(currentObj);

            if (expectedPatch != null)
            {
                bool matchesExpectedPatch = JToken.DeepEquals(JToken.Parse(expectedPatch), JToken.Parse(JsonConvert.SerializeObject(patch)));

                if (!matchesExpectedPatch)
                {
                    throw new Exception($"Expected Patch Does Not Match. Expected {expectedPatch}. Received {JsonConvert.SerializeObject(patch)}");
                }
            }

            JToken expected = JObject.Parse(JsonConvert.SerializeObject(JObject.Parse(currentJson)));
            JToken observed = JObject.Parse(JsonConvert.SerializeObject(currentObj));
            bool areEqual = JToken.DeepEquals(expected, observed);

            if (!areEqual) {
                JsonPatchDocument failPatch = PatchHelper.GetPatchForObject(
                        JsonConvert.SerializeObject(expected),
                        JsonConvert.SerializeObject(observed));
                throw new Exception($"Round trip failed. Expected {expected}. Received {observed}. Failure patch is {JsonConvert.SerializeObject(failPatch)}");
            }

            return areEqual;
        }

        [Fact]
        public void BasicSystemTextJsonExample()
        {
            var patch = PatchHelper.GetPatchForObject("{}", "{}");
            Assert.Equal(1, 1);
        }

        [Fact]
        public void ApplyingPatchToEmptyObject()
        {
            string originalJson = "{}";
            string currentJson = "{\"activityDate\":\"2021-02-11T00:00:00.0000000\",\"books\":[{\"bookedBy\":\"ANDEREI\",\"bookedDateTime\":\"2021-02-11T16:38:00.0000000\",\"bookId\":\"0\",\"bookType\":\"TL\",\"carrier\":{\"code\":\"T2383\",\"logipoints\":0.9857,\"name\":\"Lizak Transportation, Ltd. test 22\",\"scac\":\"IOKI\"},\"emptyDateTime\":\"2021-02-11T16:38:00.0000000\",\"emptyLocation\":{\"city\":\"Denver\",\"countryCode\":\"US\",\"postalCode\":\"802\",\"stateCode\":\"CO\"},\"expectedDeliveryDateTime\":\"2021-02-12T23:59:00.0000000\",\"expectedPickupDateTime\":\"2021-02-11T23:59:00.0000000\",\"roles\":{\"checkInSalesRep\":{\"code\":\"ANDEREI\"},\"carrierRep\":{\"branchCode\":\"7650\",\"code\":\"ANDEREI\"}},\"surfaceDetails\":{\"distanceToPickup\":{\"kilometers\":106.2169,\"miles\":66},\"equipment\":{\"tractorNumber\":\"123\",\"trailerNumber\":\"456\",\"trailerYear\":\"1994\"},\"isDriverEmpty\":true,\"trackingMethodCode\":\"GPS\",\"isVendor\":false,\"loadInfoTo\":\"DISPATCHER\"}}],\"bookingRules\":{\"bookingRule\":\"Rep\",\"carrierTier\":\"Base\",\"isOkToAdvertise\":false,\"isSpotBidRequested\":false,\"loadPriority\":2,\"maxBuy\":800.0000},\"checkCallRuleCode\":\"10AM      \",\"condition\":\"A\",\"customerOrders\":[{\"billTos\":[{\"billToId\":\"0\",\"branchCode\":\"7650\",\"code\":\"C800342\",\"name\":\"STRUTHO's Donuts\",\"operationsRep\":{},\"salesRep\":{\"branchCode\":\"7650\",\"code\":\"ANDEREI\"},\"secondSalesRep\":{\"code\":\"RODNTAN\"},\"orderedBy\":\"ra\",\"freightTerms\":\"\"}],\"customers\":[{\"code\":\"C800342\",\"customerId\":\"0\",\"name\":\"STRUTHO's Donuts\",\"salesRep\":{\"code\":\"ANDEREI\"},\"orderedBy\":\"ra\"}],\"executionOrderReferenceNumber\":3933385,\"loadNumber\":102512673,\"orderNumber\":0,\"orderTransactionType\":\"T\"}],\"deliverByDate\":\"2021-02-13T00:00:00.0000000\",\"deliveryReadyDate\":\"2021-02-12T00:00:00.0000000\",\"distance\":{\"kilometers\":1974.6688,\"miles\":1227},\"enteredDateTimeUtc\":\"2021-02-11T22:23:00.0000000Z\",\"enteredBySourceSystem\":\"\",\"financialCondition\":\"Error\",\"flags\":{\"hasOpenClaims\":false,\"hasOpenEvents\":false,\"isControl\":false,\"isForwardingHouse\":false,\"isHazMat\":false,\"isTeamOperated\":false,\"isRegulatedByStf\":false},\"items\":[{\"actualPallets\":5.00,\"actualQuantity\":2,\"actualWeight\":{\"metric\":18597.2720,\"standard\":41000.00},\"code\":\"1FUJ\",\"executionOrderReferenceNumber\":3933385,\"expectedMaximumWeight\":{\"metric\":20411.6400,\"standard\":45000.00},\"expectedMinimumWeight\":{\"metric\":18143.6800,\"standard\":40000.00},\"expectedPackagingCode\":\"CTN\",\"expectedTrailerSpace\":{\"metric\":14.6304,\"standard\":48},\"hazMatDetails\":{},\"itemEquipment\":{\"flatBedEquipmentDescription\":\"\",\"loadingTypeDescription\":\"\",\"tarpDescription\":\"\"},\"itemId\":\"3933385-0\",\"itemSourceUnitOfMeasure\":\"Standard\",\"name\":\"1 APPLES, FUJI, 100CT\",\"orderNumber\":0,\"packagingDimensions\":[],\"referenceNumbers\":[],\"value\":15000.0000}],\"loadNumber\":102512673,\"mode\":\"V\",\"pickUpByDate\":\"2021-02-11T00:00:00.0000000\",\"pickUpReadyByDate\":\"2021-02-11T00:00:00.0000000\",\"rating\":\"S\",\"referenceNumbers\":[],\"regionCode\":\"NA\",\"requiredShipmentEquipment\":{\"length\":{\"metric\":14.6304,\"standard\":48},\"width\":{\"metric\":254.00,\"standard\":100},\"description\":\"\",\"isCustomerSpecific\":false,\"isExactEquipment\":false},\"roles\":{\"accountExecutive\":{\"branchCode\":\"7650\",\"code\":\"STRUTHO\"},\"assignedRep\":{\"branchCode\":\"7650\",\"code\":\"ANDEREI\"}},\"securityFlags\":0,\"shipmentId\":\"102512673\",\"sourceUnitOfMeasure\":\"Standard\",\"status\":\"Delivered\",\"stops\":[{\"activities\":[{\"appointment\":{\"closeDateTime\":\"2021-02-11T23:59:00.0000000\",\"isCloseTimeSet\":true,\"isOpenTimeSet\":true,\"openDateTime\":\"2021-02-11T00:01:00.0000000\",\"rep\":{\"code\":\"MOSHDAN\"},\"scheduledBy\":\"ANDEREI\",\"scheduledCompany\":\"STRUTHO's Safeway\",\"schedulingOptionCode\":\"O\",\"scheduledEnteredDateTimeUtc\":\"2021-02-11T22:26:00.0000000Z\",\"scheduledWithFirstName\":\"Receiving\",\"statusCode\":\"C\"},\"arrivedDateTime\":\"2021-02-11T11:40:00.0000000\",\"bookIds\":[\"0\"],\"departureDateTime\":\"2021-02-11T13:40:00.0000000\",\"isDropTrailer\":false,\"isRequestedTimeSet\":false,\"itemIds\":[\"3933385-0\"],\"stopActivityId\":\"102512673_0\",\"stopId\":\"0\"}],\"billToIds\":[\"0\"],\"customerIds\":[\"0\"],\"isBlind\":false,\"loadingType\":\"NotSet\",\"referenceNumbers\":[],\"requiredDriverWorkCode\":\"NT\",\"stopId\":\"0\",\"stopLocation\":{\"address1\":\"0920\",\"address2\":\"451 E Wonderview Ave\",\"location\":{\"city\":\"Estes Park\",\"coordinate\":{\"lat\":40.37612150,\"lon\":-105.52436000},\"countryCode\":\"US\",\"county\":\"Larimer\",\"postalCode\":\"80517\",\"stateCode\":\"CO\"},\"warehouseCloseTime\":\"PT23H59M\",\"warehouseCode\":\"W2353048\",\"warehouseName\":\"STRUTHO's Safeway\",\"warehouseOpenTime\":\"PT1M\",\"warehousePhoneNumber\":\"9705864447\"},\"stopNumber\":0,\"stopStyle\":\"N\",\"stopType\":\"Pick\",\"timezoneOffsetInMinutes\":-420,\"contact\":\"Receiving\",\"needWeightScale\":false},{\"activities\":[{\"appointment\":{\"closeDateTime\":\"2021-02-12T23:59:00.0000000\",\"isCloseTimeSet\":true,\"isOpenTimeSet\":true,\"openDateTime\":\"2021-02-12T00:01:00.0000000\",\"scheduledBy\":\"ANDEREI\",\"scheduledCompany\":\"Salvation Army\",\"schedulingOptionCode\":\"O\",\"scheduledEnteredDateTimeUtc\":\"2021-02-11T22:26:00.0000000Z\",\"scheduledWithFirstName\":\"Susan\",\"scheduledWithLastName\":\"Tibbits\"},\"arrivedDateTime\":\"2021-02-11T15:40:00.0000000\",\"bookIds\":[\"0\"],\"departureDateTime\":\"2021-02-11T17:41:00.0000000\",\"isDropTrailer\":false,\"isRequestedTimeSet\":false,\"itemIds\":[\"3933385-0\"],\"stopActivityId\":\"102512673_1\",\"stopId\":\"1\"}],\"billToIds\":[\"0\"],\"customerIds\":[\"0\"],\"isBlind\":false,\"loadingType\":\"NotSet\",\"referenceNumbers\":[],\"requiredDriverWorkCode\":\"NT\",\"stopId\":\"1\",\"stopLocation\":{\"address1\":\"131 Belmont St.\",\"location\":{\"city\":\"Toledo\",\"coordinate\":{\"lat\":41.64810180,\"lon\":-83.54599700},\"countryCode\":\"US\",\"county\":\"Lucas\",\"postalCode\":\"43602\",\"stateCode\":\"OH\"},\"warehouseCloseTime\":\"PT23H59M\",\"warehouseCode\":\"W728913\",\"warehouseName\":\"Salvation Army\",\"warehouseOpenTime\":\"PT1M\",\"warehousePhoneNumber\":\"4192418231\"},\"stopNumber\":1,\"stopStyle\":\"N\",\"stopType\":\"Drop\",\"timezoneOffsetInMinutes\":-300,\"contact\":\"Susan Tibbits\",\"needWeightScale\":false}],\"totalCommodityValue\":15000.0000,\"rateLevel\":\"N\"}";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson));
        }
    }
}
