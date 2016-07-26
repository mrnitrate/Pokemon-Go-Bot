#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Logic;
using System.Windows.Forms;

#endregion

namespace PokemonGo.RocketAPI.Console
{
    internal class Program
    {
        private static liveView _liveView;
        private static bool _useLiveview = true;

        [STAThread]
        private static void Main()
        {
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));

            Task.Run(() =>
            {
                try
                {
                    Start();
                }
                catch (PtcOfflineException)
                {
                    Logger.Write("PTC Servers are probably down OR your credentials are wrong. Try google",
                        LogLevel.Error);
                    Logger.Write("Trying again in 20 seconds...");
                    Thread.Sleep(20000);

                    Start();
                }
                catch (AccountNotVerifiedException)
                {
                    Logger.Write("Account not verified. - Exiting");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Logger.Write($"Unhandled exception: {ex}", LogLevel.Error);
                    Start();
                }
            });
            System.Console.ReadLine();
        }

        private static void Start()
        {
            if (_useLiveview)
            {
                if (_liveView == null)
                {
                    Thread t = new Thread(StartLiveView);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    // Wait 2secs for form to open to get the reference
                    while(_liveView == null)
                        t.Join(1000);
                }
                new Logic.Logic(new Settings(), _liveView).Execute().Wait();
            }
            else
            {
                new Logic.Logic(new Settings()).Execute().Wait();
            }
        }

        private static void StartLiveView()
        {
            _liveView = new liveView();
            _liveView.FormClosed += StopLiveView;

            Application.EnableVisualStyles();
            Application.Run(_liveView);
        }

        public static void StopLiveView(object sender, EventArgs e)
        {
            _liveView = null;
        }
    }
}