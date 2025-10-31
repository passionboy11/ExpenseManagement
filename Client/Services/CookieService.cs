using Microsoft.JSInterop;

namespace Client.Services
{
    public class CookieService
    {
        private readonly IJSRuntime jsRuntime;
        public CookieService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }
        public async Task<string> Get(string key)
        {
            return await jsRuntime.InvokeAsync<string>("getCookie", key);
        }
        public async Task Remove(string key)
        {
            await jsRuntime.InvokeVoidAsync("deleteCookie", key);
        }
        public async Task Set(string key, string value, int days)
        {
            await jsRuntime.InvokeVoidAsync("setCookie", key, value, days);
        }
    }
}
