using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PadelInDubai.Controllers;
using PadelInDubai.DAL.Entities;
using PadelInDubai.Models;
using PadelInDubai.Services.Interfaces;

namespace PadelInDubai.Services
{
    public class AltegioClient : IExternalEventService
    {
        private readonly HttpClient _httpClient;
        private static string _bearer = Environment.GetEnvironmentVariable("PD_bearer");
        public const int CompanyId = 768552;

        public AltegioClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<EventData>> GetUpcomingEvents()
        {
            var url = $"https://api.alteg.io/api/v1/activity/{CompanyId}/search";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api.v2+json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to call API. Status code: {0}", response.StatusCode);
                return null;
            }

            var apiResponse = await DeserializeResponseAsync(response);
            if (apiResponse == null || !apiResponse.Success)
            {
                Console.WriteLine("API response indicates failure or is null.");
                return null;
            }

            return apiResponse.Data;
        }

        public async Task<EventData> GetEvent(int activityId)
        {
            var url = $"https://api.alteg.io/api/v1/activity/{CompanyId}/{activityId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api.v2+json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to call API. Status code: {0}", response.StatusCode);
                return null;
            }

            var apiResponse = await DeserializeEventDtoResponseAsync(response);
            if (apiResponse == null || !apiResponse.Success)
            {
                Console.WriteLine("API response indicates failure or is null.");
                return null;
            }

            return apiResponse.Data;
        }

        public async Task<IEnumerable<RecordData>> GetRecords(int activityId)
        {
            var url = $"https://api.alteg.io/api/v1/records/{CompanyId}?activity_id={activityId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api.v2+json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to call API. Status code: {0}", response.StatusCode);
                return Enumerable.Empty<RecordData>();
            }

            var apiResponse = await DeserializeRecordtDtoResponseAsync(response);
            if (apiResponse == null || !apiResponse.Success)
            {
                Console.WriteLine("API response indicates failure or is null.");
                return Enumerable.Empty<RecordData>();
            }

            return apiResponse.Data;
        }

        private async Task<EventsResponse> DeserializeResponseAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var settings = new JsonSerializerSettings
            {
                // Use SnakeCaseNamingStrategy so JSON "service_id" maps to ServiceId.
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new FlexibleDateTimeConverter());
            return JsonConvert.DeserializeObject<EventsResponse>(json, settings);
        }

        private async Task<EventResponse> DeserializeEventDtoResponseAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var settings = new JsonSerializerSettings
            {
                // Use SnakeCaseNamingStrategy so JSON "service_id" maps to ServiceId.
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new FlexibleDateTimeConverter());
            return JsonConvert.DeserializeObject<EventResponse>(json, settings);
        }

        private async Task<RecordResponse> DeserializeRecordtDtoResponseAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var settings = new JsonSerializerSettings
            {
                // Use SnakeCaseNamingStrategy so JSON "service_id" maps to ServiceId.
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new FlexibleDateTimeConverter());
            return JsonConvert.DeserializeObject<RecordResponse>(json, settings);
        }
    }
}
