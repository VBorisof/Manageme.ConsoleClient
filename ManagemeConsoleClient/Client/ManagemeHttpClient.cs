using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ManagemeConsoleClient.App;
using ManagemeConsoleClient.Exceptions;
using ManagemeConsoleClient.Forms;
using ManagemeConsoleClient.ViewModels;

namespace ManagemeConsoleClient.Client
{
    public class ManagemeHttpClient
    {
        private static readonly HttpClient _client = new HttpClient();
        private string _token = null;

        public bool IsLoggedIn => _token != null;

        private JsonSerializerOptions _jsonOpts;


        public ManagemeHttpClient()
        {
            _client.BaseAddress = new Uri("http://localhost:5000/api/");
            _jsonOpts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        
        public async Task<LoginViewModel> LoginAsync(LoginForm form)
        {
            var loginViewModel = await PostAsync<LoginViewModel>("login", form);
            
            _token = loginViewModel.Token;

            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _token);

            return loginViewModel;
        }

        public async Task<List<TodoViewModel>> GetTodosAsync(long categoryId)
        {
            if (!IsLoggedIn)
            {
                throw new NotLoggedInException(); 
            }

            return await GetAsync<List<TodoViewModel>>($"todo/{categoryId}");
        }

        public async Task<List<ReminderViewModel>> GetRemindersAsync()
        {
            if (!IsLoggedIn)
            {
                throw new NotLoggedInException(); 
            }

            return await GetAsync<List<ReminderViewModel>>($"reminder");
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            if (!IsLoggedIn)
            {
                throw new NotLoggedInException(); 
            }

            return await GetAsync<List<CategoryViewModel>>($"category");
        }

        public async Task AddTodoAsync(TodoForm form)
        {
            if (!IsLoggedIn)
            {
                throw new NotLoggedInException(); 
            }

            await PostAsync($"todo", form);
        }

        public async Task ToggleTodoDoneAsync(long todoId)
        {
            if (!IsLoggedIn)
            {
                throw new NotLoggedInException(); 
            }

            await PutAsync($"todo/{todoId}", new {});
        }

        public async Task DeleteTodoAsync(long todoId)
        {
            if (!IsLoggedIn)
            {
                throw new NotLoggedInException(); 
            }

            await DeleteAsync($"todo/{todoId}");
        }

        public async Task<T> DeleteAsync<T>(string url)
        {
            try
            {
                var resp = await _client.DeleteAsync(url);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOpts);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot DELETE url `{url}`", e);
            }
        }

        public async Task DeleteAsync(string url)
        {
            try
            {
                var resp = await _client.DeleteAsync(url);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot DELETE url `{url}`", e);
            }
        }

        public async Task<T> PutAsync<T>(string url, object payload)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(payload), 
                    Encoding.UTF8,
                    "application/json"
                );
                var resp = await _client.PutAsync(url, content);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOpts);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot PUT url `{url}`", e);
            }
        }
        
        public async Task PutAsync(string url, object payload)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(payload), 
                    Encoding.UTF8,
                    "application/json"
                );
                var resp = await _client.PutAsync(url, content);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot PUT url `{url}`", e);
            }
        }

        public async Task<T> PostAsync<T>(string url, object payload)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(payload), 
                    Encoding.UTF8,
                    "application/json"
                );
                var resp = await _client.PostAsync(url, content);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOpts);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot POST url `{url}`", e);
            }
        }

        public async Task PostAsync(string url, object payload)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(payload), 
                    Encoding.UTF8,
                    "application/json"
                );
                var resp = await _client.PostAsync(url, content);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot POST url `{url}`", e);
            }
        }

        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                var resp = await _client.GetAsync(url);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOpts);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Unsuccessful Response: {resp.StatusCode}"
                    );
                }
            }
            catch (HttpRequestException e)
            {
                throw new AppException($"Cannot GET url `{url}`", e);
            }
        }
    }
}
