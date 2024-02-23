using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using Newtonsoft.Json;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Handlers.Plugins.Governments.NL.Models;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class NLGovernmentSchoolHolidaysPlugin(TeamsAdapter teamsAdapter,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : NLGovernmentBasePlugin(teamsAdapter, proactiveMessageService, driveRepository, "School Holidays")
    {

        [Action("Rijksoverheid.SearchSchoolHolidays")]
        [Description("Search for dutch (NL) school holidays")]
        [Parameter(name: "schoolYear", type: "string", required: true, description: "The school holiday year. For example: 2023-2024 for the school year 2023-2024")]
        public async Task<string> SearchSchoolHolidays([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);

            if (!parameters.TryGetValue("schoolYear", out var schoolYearObj))
            {
                return "School year parameter is missing";
            }

            var schoolYear = schoolYearObj?.ToString();

            var response = await client.GetAsync($"infotypes/schoolholidays/schoolyear/{schoolYear}?output=json");
            if (!response.IsSuccessStatusCode)
            {
                return "Failed to retrieve data";
            }

            var data = await response.Content.ReadFromJsonAsync<SchoolHoliday>();
            if (data?.Content == null)
            {
                return "No data found";
            }

            var flattened = data.Content?
                .SelectMany(h => h.Vacations ?? Enumerable.Empty<Vacation>())
                .SelectMany(v => v.Regions?.Select(r => new VacationRegionData
                {
                    EndDate = r.EndDate,
                    StartDate = r.StartDate,
                    Type = v.Type,
                    Region = r.Region
                }) ?? []) ?? [];

            if (flattened.Any())
            {
                var resultJson = JsonConvert.SerializeObject(flattened);
                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultJson, cardId);
                return resultJson;
            }

            return "No data found";
        }
    }
}
