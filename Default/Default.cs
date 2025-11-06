using System.Diagnostics;
using System.Text;

namespace Default
{
    public class CDefault
    {
        public string ExcuteShell(string strCommand)
        {
            ProcessStartInfo hCommandPrompt = new ProcessStartInfo();
            System.Diagnostics.Process hProcess = new System.Diagnostics.Process();

            hCommandPrompt.FileName = @"cmd";
            hCommandPrompt.WindowStyle = ProcessWindowStyle.Hidden;
            hCommandPrompt.CreateNoWindow = true;
            hCommandPrompt.UseShellExecute = false;
            hCommandPrompt.RedirectStandardOutput = true;
            hCommandPrompt.RedirectStandardInput = true;
            hCommandPrompt.RedirectStandardError = true;

            hProcess.EnableRaisingEvents = false;
            hProcess.StartInfo = hCommandPrompt;
            hProcess.Start();
            hProcess.StandardInput.Write(strCommand + Environment.NewLine);
            hProcess.StandardInput.Close();

            string strResult = hProcess.StandardOutput.ReadToEnd();
            StringBuilder hStringBuilder = new StringBuilder();

            hStringBuilder.Append(strResult);
            hStringBuilder.Append("\r\n");

            return hStringBuilder.ToString();
        }
    }
}
