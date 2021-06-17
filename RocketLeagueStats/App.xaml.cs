using Microsoft.Win32;

using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace RocketLeagueStats
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //[DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    if (!IsAssociated())
        //        Associate();
        //    base.OnStartup(e);
        //}

        ////public static void SetAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        ////{
        ////    // The stuff that was above here is basically the same

        ////    // Delete the key instead of trying to change it
        ////    CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
        ////    CurrentUser.DeleteSubKey("UserChoice", false);
        ////    CurrentUser.Close();

        ////    // Tell explorer the file association has been changed
        ////    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        ////}

        //public static bool IsAssociated()
        //{
        //    return Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.rpls", false) == null;
        //}

        //public static void Associate()
        //{
        //    using (var fileReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.rpls"))
        //    {
        //        fileReg.CreateSubKey("DefaultIcon").SetValue(string.Empty, @"C:\Developement\Github repos\RocketLeagueStats\RocketLeagueStats\RocketLeagueStatsIcon.ico");
        //        fileReg.CreateSubKey("PerceivedType").SetValue(string.Empty, "Text");
        //    }
        //    using (var appReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Applications\\RocketLeagueStats.exe"))
        //    {
        //        appReg.CreateSubKey("shell\\open\\command").SetValue(string.Empty, "\"" + @"C:\Developement\Github repos\RocketLeagueStats\RocketLeagueStats\bin\Debug\net5.0-windows\RocketLeagueStats.exe" + "\" %1");
        //        appReg.CreateSubKey("shell\\edit\\command").SetValue(string.Empty, "\"" + @"C:\Developement\Github repos\RocketLeagueStats\RocketLeagueStats\bin\Debug\net5.0-windows\RocketLeagueStats.exe" + "\" %1");
        //        appReg.CreateSubKey("DefaultIcon").SetValue(string.Empty, @"C:\Developement\Github repos\RocketLeagueStats\RocketLeagueStats\RocketLeagueStatsIcon.ico");
        //    }
        //    using (var appAssoc = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.rpls", true))
        //    {
        //        appAssoc.DeleteSubKey("UserChoice", false);
        //        appAssoc.CreateSubKey("UserChoice").SetValue("Progid", "Applications\\RocketLeagueStats.exe");
        //    }
        //    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        //}
    }
}
