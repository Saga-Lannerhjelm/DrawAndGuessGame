using Newtonsoft.Json;

namespace webbAPI.Services
{
    public class WordService
    {
        public async Task<string> GetWord() 
        {
            // Link to API https://random-word-form.herokuapp.com
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync("https://random-word-form.herokuapp.com/random/noun");

            if (response.IsSuccessStatusCode)
            {
                string apiResp = await response.Content.ReadAsStringAsync();
                string[] words = JsonConvert.DeserializeObject<string[]>(apiResp) ?? [];
                return (words?.Length > 0) ? words[0] : "default word";
            } else {
                return "Default word";
            }
        }
    }
}