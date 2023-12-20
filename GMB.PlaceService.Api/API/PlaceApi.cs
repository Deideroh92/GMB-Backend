using GMB.Sdk.Core.Types.Models;
using System.Text;

namespace GMB.PlaceService.Api.API
{
    /// <summary>
    /// Business Service.
    /// </summary>
    public class PlaceApi {

        public static readonly string API_KEY = "AIzaSyBxTQaKxPyZ815_maffoRKjLqs40olcQhw";
        public static readonly string LANGUAGE = "fr";

        /// <summary>
        /// Get Place by place ID from Google API
        /// </summary>
        /// <param name="placeId"></param>
        /// <returns>Place or null if nothing found</returns>
        public static async Task<Place?> GetPlaceByPlaceId(string placeId)
        {
            try
            {
                using HttpClient client = new();
                string apiUrl = "https://places.googleapis.com/v1/places/" + placeId;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                request.Headers.Add("X-Goog-Api-Key", API_KEY);
                request.Headers.Add("X-Goog-FieldMask", "id,displayName,formattedAddress,internationalPhoneNumber," +
                    "nationalPhoneNumber,websiteUri,userRatingCount,googleMapsUri,addressComponents,plusCode,shortFormattedAddress," +
                    "location,rating,businessStatus");

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Place? place = Place.FromJson(responseBody);
                    return place;
                }

                return null;
            } catch (Exception e)
            {
                throw new Exception($"Couldn't get place from Google API for placeId = [{placeId}]", e);
            }
        }
        /// <summary>
        /// Get Place ID by query from Google API
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Place ID or null if nothing found</returns>
        public static async Task<string?> GetPlaceIdByQuery(string query)
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
        /// <summary>
        /// Get Places by Query from Google API
        /// </summary>
        /// <param name="query"></param>
        /// <returns>List of possible matching places</returns>
        public static async Task<Place[]?> GetPlacesByQuery(string query)
        {
            try
            {
                using HttpClient client = new();
                Root? placeResponse = new();

                string apiUrl = "https://places.googleapis.com/v1/places:searchText";
                client.DefaultRequestHeaders.Add("X-Goog-Api-Key", API_KEY);
                //client.DefaultRequestHeaders.Add("languageCode", LANGUAGE);
                client.DefaultRequestHeaders.Add("X-Goog-FieldMask", "places.id,places.displayName,places.formattedAddress,places.internationalPhoneNumber," +
                    "places.nationalPhoneNumber,places.websiteUri,places.userRatingCount,places.googleMapsUri,places.addressComponents,places.plusCode,places.shortFormattedAddress," +
                    "places.location,places.rating,places.businessStatus");
                string requestBody = $"{{\"textQuery\": \"{query}\"}}";

                HttpResponseMessage response = await client.PostAsync(apiUrl, new StringContent(requestBody, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    placeResponse = Root.FromJson(responseBody);
                }

                return placeResponse.Places;
            } catch (Exception e)
            {
                throw new Exception($"Couldn't get places from Google API for query = [{query}]", e);
            }
        }
    }
}