﻿using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class NLGovernmentGeneralPlugin(TeamsAdapter teamsAdapter,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : NLGovernmentBasePlugin(teamsAdapter, proactiveMessageService, driveRepository, "General")
    {

        [Action("Rijksoverheid.GetMinistries")]
        [Description("Gets a list of NL government ministries")]
        public Task<string> GetMinistries([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters, "infotypes/ministry");
        }

        [Action("Rijksoverheid.GetSubjects")]
        [Description("Gets a list of NL government subjects")]
        public Task<string> GetSubjects([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters, "infotypes/subject");
        }
    }
}
