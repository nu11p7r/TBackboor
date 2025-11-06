using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoneBot.Config
{
    internal sealed class CConfig
    {
        // Telegram Bot 변수
        public static string TelegramAPI = "";
        public static string TelegramChatID = "";

        // DDoS 플러그인 콘피그 관련 기능
        public static int MAX_LOOP = 10; 
    }
}
