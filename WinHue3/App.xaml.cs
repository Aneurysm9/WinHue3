﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using log4net.Repository.Hierarchy;
using WinHue3.Logs;
using WinHue3.Philips_Hue.Communication;
using WinHue3.Utils;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;
using System.Threading;
using WinHue3.Functions.Application_Settings.Settings;
using Form_EventLog = WinHue3.Functions.EventViewer.Form_EventLog;

namespace WinHue3
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Form_EventLog _fel = new Form_EventLog();

        public App()
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Hierarchy hier = log4net.LogManager.GetRepository() as Hierarchy;

            if (hier != null)
            {
                DataGridViewAppender dgva = (DataGridViewAppender)hier.GetAppenders().FirstOrDefault(appender => appender.Name.Equals("DataGridViewAppender"));
                dgva.DgEventLog = _fel.ViewModel.EventViewerModel.ListLogEntries;
            }

            Log.Info(WinHueSettings.settings.Language);
            if (!string.IsNullOrEmpty(WinHueSettings.settings.Language) &&
                !string.IsNullOrWhiteSpace(WinHueSettings.settings.Language))
            {
                var culture = new CultureInfo(WinHueSettings.settings.Language);
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Log.Info($@"WinHue {Assembly.GetExecutingAssembly().GetName().Version.ToString()} started");
            Log.Info($"User is running as administrator : {UacHelper.IsProcessElevated}");
            MainForm.MainWindow wnd = new MainForm.MainWindow(_fel);
            MainWindow.Title = "WinHue " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            double height = SystemParameters.WorkArea.Height * 0.75 >= MainWindow.MinHeight
                ? SystemParameters.WorkArea.Height*0.75
                : MainWindow.MinHeight;

            double width = SystemParameters.WorkArea.Width * 0.75 >= MainWindow.MinWidth
                ? SystemParameters.WorkArea.Width * 0.75
                : MainWindow.MinWidth;

            MainWindow.Height = height;
            MainWindow.Width = width;

            switch (WinHueSettings.settings.StartMode)
            {
                case 0:
                    wnd.WindowState = WindowState.Normal;
                    wnd.Show();
                    break;
                case 1:
                    wnd.Hide();
                    break;
                case 2:
                    wnd.WindowState = WindowState.Minimized;
                    wnd.Show();
                    break;
                default:
                    wnd.Show();
                    wnd.WindowState = WindowState.Normal;
                    break;
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
           
            MessageBox.Show("Sorry but an unexpected exception occured. Please report the exception on the support website so the developper can fix the issues. Please include the most recent log located in the logs folder.");
            string ex = Serializer.SerializeToJson(e.ExceptionObject);
            Log.Fatal("Unexpected Exception : ",(Exception)e.ExceptionObject);
            Log.Fatal(ex);
            MiniDump.MiniDumpToFile(path + "\\log\\" + $"WinHue3_dump_{DateTime.Now}.dmp");
        }


    }


    public class MiniDump
    {

        internal enum MINIDUMP_TYPE
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000
        }
        [DllImport("dbghelp.dll")]
        private static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            Int32 ProcessId,
            IntPtr hFile,
            MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallackParam);

        public static void MiniDumpToFile(string fileToDump)
        {
            FileStream fsToDump = null;
            fsToDump = File.Exists(fileToDump) ? File.Open(fileToDump, FileMode.Append) : File.Create(fileToDump);
            Process thisProcess = Process.GetCurrentProcess();
            MiniDumpWriteDump(thisProcess.Handle, thisProcess.Id,
                fsToDump.SafeFileHandle.DangerousGetHandle(), MINIDUMP_TYPE.MiniDumpNormal,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            fsToDump.Close();
        }
    };
}
