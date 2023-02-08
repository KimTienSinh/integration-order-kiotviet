using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
// using Telegram.Bot;
// using Telegram.Bot.Types;
// using Telegram.Bot.Types.Enums;

namespace Kps.Integration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotTelegramController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public BotTelegramController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        [HttpPost("sendMessageTelegram")]
        public async Task<IActionResult> PostBotAsync(string groupid, string text)
        {
            // var botTelegramConfig = configuration.GetValue<string>("BotTelegramOptions:BotTelegrams:BotId");
            // var botClient = new TelegramBotClient(botTelegramConfig);
            // using var cts = new CancellationTokenSource();
            // Message sentMessage = await botClient.SendTextMessageAsync(
            //     chatId: groupid,
            //     text: text,
            //     cancellationToken: cts.Token);

            /*if (file != "")
            {
                string urlUc = file.Substring(0, 25) + "uc?export=download&id=";
                string urlIdView = file.Substring(file.IndexOf("/d/") + 3);
                string urlId = urlIdView.Split('/').First();
                string url = urlUc + urlId;
                Message message = await botClient.SendDocumentAsync(
                chatId: groupid,
                document: url,
                parseMode: ParseMode.Html,
                cancellationToken: cts.Token);
            }
            else
            {
                return null;
            }*/
            return Ok();
        }
    }
}
