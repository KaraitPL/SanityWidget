using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Interop;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace SanityWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        private bool isDragging = false;
        private Point offset;
        private int maxSanity = 200;
        private double currentSanity = 20;
        private int currentSanityState = 2;

        private DispatcherTimer timer;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _hookCallback;

        public MainWindow()
        {
            InitializeComponent();
            progressBar.Value = (currentSanity / maxSanity) * 100;
            sanityAmount.Text = ((int)currentSanity).ToString();
            _hookCallback = HookCallback;
            SetHook();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Przykładowy interwał, co sekundę
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (currentSanityState){
                case 0:
                    currentSanity -= 0.2;
                    break;
                case 1:
                    currentSanity -= 0.1;
                    break;
                case 2:
                    break;
                case 3:
                    currentSanity += 0.1;
                    break;
                case 4:
                    currentSanity += 0.2;
                    break;
            }
            progressBar.Value = (currentSanity / maxSanity) * 100;
            sanityAmount.Text = ((int)currentSanity).ToString();
        }

        private void SetHook()
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Tutaj możesz przetworzyć wcisnięty klawisz
                if(vkCode == 38)
                {
                    currentSanity += 10;
                    progressBar.Value = (currentSanity / maxSanity) * 100;
                    sanityAmount.Text = ((int)currentSanity).ToString();
                }
                else if(vkCode == 40)
                {
                    currentSanity -= 10;
                    progressBar.Value = (currentSanity/maxSanity) * 100;
                    sanityAmount.Text = ((int)currentSanity).ToString();
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            UnhookWindowsHookEx(_hookID);
        }

        // Importowane funkcje z user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = SystemParameters.WorkArea.Right - Width;
            Top = 80;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            offset = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentMousePosition = e.GetPosition(this);
                this.Left = currentMousePosition.X - offset.X + this.Left;
                this.Top = currentMousePosition.Y - offset.Y + this.Top;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

    }
}