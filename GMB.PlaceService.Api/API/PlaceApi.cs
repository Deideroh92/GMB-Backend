using GMB.Sdk.Core;
using GMB.Sdk.Core.Types.Models;

namespace GMB.PlaceService.Api.API
{
    /// <summary>
    /// Business Service.
    /// </summary>
    public class PlaceApi {

        public static readonly string API_KEY = "AIzaSyCHUy9kawuZ69nHW-XvzkzdvnZQ_FcRhk0";
        public static readonly string LANGUAGE = "fr";

        /// <summary>
        /// Get Business info by Place Id from Google API
        /// </summary>
        /// <param name="placeId"></param>
        public static async Task<PlaceDetailsResponse?> GetGMB(string placeId)
        {
            try
            {
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=" +
                    $"name" +
                    $"%2Crating" +
                    $"%2Cformatted_phone_number" +
                    $"%2Cbusiness_status" +
                    $"%2Cgeometry" +
                    $"%2Cplace_id" +
                    $"%2Cplus_code" +
                    $"%2Curl" +
                    $"%2Caddress_components" +
                    $"%2Cuser_ratings_total" +
                    $"%2Cinternational_phone_number" +
                    $"%2Cwebsite" +
                    $"%2Cformatted_address" +
                    $"%2Ctypes" +
                    $"&key={API_KEY}&language={LANGUAGE}");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    PlaceDetailsResponse? placeDetailsResponse = PlaceDetailsResponse.FromJson(responseBody);
                    return placeDetailsResponse;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Couldn't get GMB from Google API for placeId = [{placeId}]", e);
            }
        }
        /// <summary>
        /// Get Place Id by query from Google API
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<string?> GetPlaceId(string query)
        {
            try
            {
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?input=%2{query}&inputtype=textquery&key={API_KEY}&language={LANGUAGE}");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    PlaceIdResponse? placeIdResponse = PlaceIdResponse.FromJson(responseBody);

                    if (placeIdResponse != null && placeIdResponse.PlaceIds != null && placeIdResponse.PlaceIds.Count > 0)
                    {
                        string? firstPlaceId = placeIdResponse.PlaceIds[0]?.PlaceId;
                        return firstPlaceId;
                    }
                } else
                {
                    throw new HttpRequestException($"HTTP request failed with status code: {response.StatusCode}");
                }

                return null;
            } catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP request failed: {ex.Message}");
            } catch (Exception ex)
            {
                throw new Exception($"An error occurred while getting Place ID from Google API using query: {query}", ex);
            }
        }
    }
}