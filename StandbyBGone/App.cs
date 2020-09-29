using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using FormsTimer = System.Windows.Forms.Timer;

namespace StandbyBGone
{
    public class App : ApplicationContext
    {
        /// <summary>
        /// name of the control mutex
        /// </summary>
        const string MUTEX_NAME = "standbybgone-mutex";

        #region Main
        /// <summary>
        /// Global app instance
        /// </summary>
        public static App Instance { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            //forms stuff
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            //only one instance, enforce using mutex
            try
            {
                using (Mutex m = new Mutex(true, MUTEX_NAME))
                {
                    //check if already running
                    if (!m.WaitOne(0, true))
                        throw new ApplicationException("already running");

                    //run app, starts listen thread internally
                    Instance = new App();
                    Application.Run(Instance);
                }
            }
            catch (ApplicationException)
            {
                //mutex already owned = one instance already running, just exit
            }
        }
        #endregion

        /// <summary>
        /// dispatcher for the main thread
        /// </summary>
        Dispatcher dispatcher;

        /// <summary>
        /// Timer to refresh Thread Execution State
        /// </summary>
        FormsTimer refreshTimer = new FormsTimer()
        {
            Interval = 120000 //120s = 2min
        };

        /// <summary>
        /// tray icon to show activity
        /// </summary>
        NotifyIcon trayIcon;

        public App()
        {
            //register application exit
            Application.ApplicationExit += OnAppExit;

            //dispatch init
            dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.BeginInvoke(() =>
            {
                //init tray ui
                InitTray();

                //start timer
                refreshTimer.Tick += RefreshThreadExState;
                refreshTimer.Start();
            });
        }

        /// <summary>
        /// initialize and show the tray icon
        /// </summary>
        void InitTray()
        {
            //init context menu
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, (s, e) =>
            {
                Application.Exit();
            });

            //init tray
            trayIcon = new NotifyIcon()
            {
                Text = "StandbyBGone",
                Icon = Resources.icon1,
                ContextMenuStrip = contextMenu,
                Visible = true
            };
        }

        /// <summary>
        /// Called by refreshTimer to refresh Thread Execution State
        /// </summary>
        void RefreshThreadExState(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Refreshing ThreadExecutionState...");
                Native.SetThreadExecutionState(Native.EXECUTION_STATE.ES_CONTINUOUS | Native.EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in Native.SetThreadExecutionState: {ex}");
            }
        }

        /// <summary>
        /// called when the app exits
        /// </summary>
        void OnAppExit(object sender, EventArgs e)
        {
            //dispose tray
            trayIcon?.Dispose();
        }
    }
}
