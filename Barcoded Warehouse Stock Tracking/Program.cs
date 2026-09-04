using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Barcoded_Warehouse_Stock_Tracking
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                var appDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BarcodedWarehouse");
                Directory.CreateDirectory(appDir);
                AppDomain.CurrentDomain.SetData("DataDirectory", appDir);

                FeatureManager.LoadConfig();

                // EnsureDatabase LoginForm içinde arka planda çalışır — burada bekleme yok
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new Form1());
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Uygulama başlatılırken bir hata oluştu.\n\n" +
                    ex.Message +
                    "\n\nDetay:\n" + ex,
                    "Başlatma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogError(e.Exception);
            MessageBox.Show("Sistemde beklenmeyen bir hata oluştu. Hata log dosyasına kaydedildi.", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError(ex);
            }
            MessageBox.Show("Sistemde kritik bir hata oluştu ve uygulama kapatılacak.", "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void LogError(Exception ex)
        {
            try
            {
                var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BarcodedWarehouse");
                var logFile = Path.Combine(appDir, "error.log");
                var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{new string('-', 50)}{Environment.NewLine}";
                File.AppendAllText(logFile, message);
            }
            catch { }
        }
    }
}
