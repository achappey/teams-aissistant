using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Teams.AI;

namespace MathBot.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController(TeamsAdapter adapter, IBot bot) : ControllerBase
    {
        
        [HttpPost]
        public async Task PostAsync(CancellationToken cancellationToken = default)
        {
            await adapter.ProcessAsync
            (
                Request,
                Response,
                bot,
                cancellationToken
            );
        }
    }
}
