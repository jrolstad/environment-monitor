using System;
using System.Windows.Forms;
using environment_monitor.Properties;

namespace environment_monitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var process = new MonitorProcess())
            {
                process.Start();
                Application.Run();
            }
        }
    }

    public class MonitorProcess : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Timer _timer;

        private readonly TimeSpan _monitorInterval;
        private readonly TimeSpan _alertTime;

        private bool _isMuted;

        private static string _applicationName = "Environment Monitor";

        public MonitorProcess() : this(new NotifyIcon(), new Timer())
        {

        }

        public MonitorProcess(NotifyIcon icon, Timer timer) : this(new NotifyIcon(), new Timer(), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10))
        {

        }

        public MonitorProcess(NotifyIcon icon, Timer timer, TimeSpan monitorInterval, TimeSpan alertTime)
        {
            _monitorInterval = monitorInterval;
            _alertTime = alertTime;
            _notifyIcon = icon;
            _timer = timer;

            _isMuted = false;

            _timer.Interval = (int)_monitorInterval.TotalMilliseconds;
            _timer.Tick += _timer_Tick;
        }

        public void Dispose()
        {
            _timer.Stop();

            _notifyIcon.Dispose();
        }

        public void Start()
        {
            _notifyIcon.Icon = Resources.PathIcon;
            _notifyIcon.Text = _applicationName;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = CreateContextMenu();

            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_isMuted) return;

            var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);

            if (path != null && path.Length > 2048)
            {
                var message = $"Path variable length ({path.Length}) exceeds 2048 characters";
                _notifyIcon.ShowBalloonTip((int)_alertTime.TotalMilliseconds, _applicationName, message, ToolTipIcon.Warning);
            }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            var exitMenuItem = new ToolStripMenuItem {Text = "Exit"};
            exitMenuItem.Click += (sender, args) => Application.Exit();
            menu.Items.Add(exitMenuItem);

            var muteMenuItem = new ToolStripMenuItem { Text = "Mute" };
            muteMenuItem.Click += (sender, args) =>
            {
                _isMuted = !_isMuted;
                muteMenuItem.Text = _isMuted ? "Mute" : "Activate";
            };
            menu.Items.Add(muteMenuItem);

            return menu;
        }

       
    }
}
