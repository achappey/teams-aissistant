using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace MathBot.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController(IBotFrameworkHttpAdapter adapter, IBot bot) : ControllerBase
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
