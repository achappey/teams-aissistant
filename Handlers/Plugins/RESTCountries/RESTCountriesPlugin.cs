using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using Newtonsoft.Json;
using TeamsAIssistant.Attributes;
using RESTCountries.NET.Services;
using RESTCountries.NET.Models;

namespace TeamsAIssistant.Handlers.Plugins.SchoolHolidays
{
    public class RESTCountriesPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
        : PluginBase(driveRepository, proactiveMessageService, "Countries", "GitHub", "RESTCountries", "v3")
    {

        [Action("GitHub.SearchCountryCodes")]
        [Description("Searches country codes and names")]
        [Parameter(name: "name", type: "string", description: "Search query by name (contains)")]
        public async Task<string> GetCountries([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);

            var items = parameters.TryGetValue("name", out var nameValue) && !string.IsNullOrEmpty(nameValue?.ToString())
                     ? RestCountriesService.GetAllCountries()
                     : RestCountriesService.GetCountriesByNameContains(nameValue?.ToString() ?? string.Empty);

            var countries = items.Select(t => new { t.Name, t.Cca2 });

            if (countries.Any())
            {
                var resultJson = JsonConvert.SerializeObject(countries);
                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultJson, cardId);
                
                return resultJson;
            }

            return "No data found";
        }

        [Action("GitHub.GetCountryDetail")]
        [Description("Gets all country details by the alpha-2 code")]
        [Parameter(name: "cca2", type: "string", required: true, description: "The alpha-2 code of the country")]
        public async Task<string> GetCountryDetail([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);

            if (parameters.TryGetValue("cca2", out var nameValue) && !string.IsNullOrEmpty(nameValue?.ToString()))
            {
                Country? result = RestCountriesService.GetCountryByCode(nameValue.ToString()!);
                var resultJson = JsonConvert.SerializeObject(result);
                
                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultJson, cardId);
                
                return resultJson;
            }

            return "Cca2 is required";
        }
    }
}
