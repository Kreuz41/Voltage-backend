using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VerifyBot;

public class VerifyTelegramBot
{
	private static TelegramBotClient _botClient;
	private static readonly HttpClient _client = new HttpClient();
	private string _botToken;

	public VerifyTelegramBot(string botToken)
	{
		_botToken = botToken;
	}

	private ReceiverOptions _receiverOptions = new()
	{
		AllowedUpdates = new[]
		{
			UpdateType.Message,
		},
	};
	public async Task BotStart()
	{
		_botClient = new TelegramBotClient(_botToken);
		_botClient.StartReceiving(updateHandler: HandleUpdateAsync, HandlePollingErrorAsync, _receiverOptions);
		var me = await _botClient.GetMeAsync();
		await Console.Out.WriteLineAsync(me.FirstName);
	}

	private async Task HandleUpdateAsync(ITelegramBotClient arg1, Update e, CancellationToken arg3)
	{
		try
		{
			if (e.Message.Text != null)
			{
				if (e.Message.Text.StartsWith("/start"))
				{
					if (!int.TryParse(GetCodeFromStartCommand(e.Message.Text), out int code))
						return;
					var result = await _client.GetAsync($"http://fp:80/api/verify/get/{code}");
					if (!result.IsSuccessStatusCode)
						return;
					var resultContent = await result.Content.ReadFromJsonAsync<int>();
					var userId = resultContent;
					if (userId == null)
						return;
					result = await _client.PutAsync($"http://fp:80/api/user/verifyTelegram?userId={userId}&chatId={e.Message.Chat.Id}", null);
					return;
				}
			}
			return;
		}
		catch (Exception ex) { return; }
	}

	private string GetCodeFromStartCommand(string startCommand)
	{
		var code = startCommand.Split(' ')[1];
		return code;
	}
	private async Task HandlePollingErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
	{
		return;
	}
}
