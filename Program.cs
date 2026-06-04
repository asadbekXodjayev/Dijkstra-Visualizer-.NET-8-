using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show splash screen first
            Application.Run(new SplashForm());
            
            // After splash closes, show login form
            LoginForm login = new LoginForm();
            Application.Run(login);
            
            // Only proceed to the main form if the user actually authenticated.
            // If the login window was closed/cancelled, login.Username stays null
            // and we must not launch the main form.
            if (string.IsNullOrEmpty(login.Username))
                return;

            // After login, show main form
            MainForm mainForm = new MainForm(login.Username);
            Application.Run(mainForm);
        }
    }
}