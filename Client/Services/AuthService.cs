using Client.DTO;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Client.Services
{
    public class AuthService
    {
        private readonly AccessTokenService accessTokenService;
        private readonly NavigationManager nav;
        private HttpClient client;
        public AuthService(AccessTokenService accessTokenService,
            NavigationManager nav,
            IHttpClientFactory httpClientFactory)
        {
            this.accessTokenService = accessTokenService;
            this.nav = nav;
            client = httpClientFactory.CreateClient("ApiClient");

        }
        public async Task<bool> Login(string email, string password)
        {
            var status = await client.PostAsJsonAsync("controller", new { email, password });
            if (status.IsSuccessStatusCode)
            {
                var token = await status.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AuthResponse>(token);
                await accessTokenService.SetToken(result.AccessToken);
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> IsApiAvailable()
        {
            try
            {
                var response = await client.GetAsync("auth/login");
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Register(string email, string password, string role)
        {
            var status = await client.PostAsJsonAsync("auth/register", new { email, password, role });
            return status.IsSuccessStatusCode;
        }

        public async Task Logout()
        {
            //await accessTokenService.SetToken("");
            //nav.NavigateTo("/login");

            var responseMessage = await client.PostAsync("auth/logout", null);
            if (responseMessage.IsSuccessStatusCode)
            {
                await accessTokenService.RemoveToken();
                nav.NavigateTo("/login", forceLoad: true);
            }
        }
    }
}
