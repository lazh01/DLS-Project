using NewsletterService.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NewsletterService.Services
{
    public class SubscriberApiService
    {
        private readonly HttpClient _httpClient;

        public SubscriberApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SubscriberDto>> GetAllSubscribersAsync()
        {
            try
            {
                var subscribers = await _httpClient.GetFromJsonAsync<List<SubscriberDto>>("api/subscribers");
                return subscribers ?? new List<SubscriberDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch subscribers: {ex.Message}");
                return new List<SubscriberDto>();
            }
        }
    }
}