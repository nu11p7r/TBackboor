using NoneBot.Common;
using NoneBot.Process;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NoneBot.Telegram
{
    public class CTelegram
    {
        private static TelegramBotClient ms_hClient = new TelegramBotClient("");
        private static ReceiverOptions ms_hOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message },
            ThrowPendingUpdates = true
        };
        private static long ms_lChatID = 0;
        private static CancellationToken m_hCancellationToken = new CancellationToken();

        public static TelegramBotClient GetTelegramBotClient() { return ms_hClient; }
        public static ReceiverOptions GetReceiverOptions() { return ms_hOptions; }
        public static long GetChatID() { return ms_lChatID; }
        public static CancellationToken GetCancellationToken() { return m_hCancellationToken; }

        public async static Task HandleUpdateAsync(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
        {
            try
            {
                TSingleton<CProcess>.ms_hInstance.SubProcess(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                await SendTextMessageAsync(ex.ToString());
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            return Task.CompletedTask;
        }

        public static Task SendTextMessageAsync(string str)
        {
            ms_hClient.SendTextMessageAsync(ms_lChatID, str);
            return Task.CompletedTask;
        }
    }
}
