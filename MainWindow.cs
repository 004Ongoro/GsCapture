using GsCapture.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Windows.UI.Notifications;
using Accord.Video.FFMPEG;

namespace GsCapture
{
    public partial class MainWindow : Form
    {

        // ACTIVE WINDOW LOGIC AND DLL
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        // HOTKEYS 
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int VK_S = 0x53;

        private IntPtr hookId = IntPtr.Zero;


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        string savePath = "Test Image screenshot.Png";
        float brightnessValue, contrastValue;
        Bitmap originalImage;
        Bitmap currentImage;
        Image screenshot;

        // FIELDS FOR SCREEN RECORDING FUNCTIONALITY
        private Timer captureTimer;
        private VideoFileWriter videoWriter;
        private bool isRecording = false;
        private string videoOutputPath;
        Rectangle videoArea;
        public MainWindow(string imagePath) 
        {
            InitializeComponent();
            InitializeHotKey();
            LoadImage(imagePath);
            InitTimer();
            editorPanel.Visible = false;
        }
        /// <summary>
        /// Initialises the capture Timer
        /// capture timer is used to capture the screen at regular intervals, in this case
        /// after every 100ms(10 frames per second -fps)
        /// </summary>
        private void InitTimer()
        {
            captureTimer = new Timer()
            {
                Interval = 100
            };

            captureTimer.Tick += CaptureTimer_Tick;
        }


        /// <summary>
        /// While the timer ticks, we capture the screen and write the 
        /// frame to a file using videoWriter from the
        /// Accord.Video.FFMPEG
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void CaptureTimer_Tick(object sender, EventArgs e)
        {
            int captureWidth = videoArea.Width, captureHeight = videoArea.Height;
            
            using (Bitmap bmp = new Bitmap(captureWidth, captureHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, videoArea.Size);
                }
                videoWriter.WriteVideoFrame(bmp);
                previewZone.Image = (Bitmap)bmp.Clone();

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void buttonTakeScreenshot_Click(object sender, EventArgs e)
        {
            //GetCaptureArea();
            //TakeScreenShot(Screen.PrimaryScreen.Bounds);
            using (var overlay = new OverlayForm())
            {
                if(overlay.ShowDialog() == DialogResult.OK)
                {
                    var captureArea = overlay.CaptureArea;
                    TakeScreenShot(captureArea);
                }
            }
        }


        /// <summary>
        /// Contains the logic for taking the screenshot using Bitmap
        /// and saves it to a file.
        /// </summary>
        /// <param name="rectArea"></param>
        private void TakeScreenShot(Rectangle rectArea)
        {
            
            try
            {
                Rectangle captureArea = Screen.PrimaryScreen.Bounds;
                using (Bitmap bmp = new Bitmap(rectArea.Width, rectArea.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(rectArea.Location, Point.Empty, captureArea.Size);
                    }

                    screenshot = bmp.GetThumbnailImage(captureArea.Width, captureArea.Height, null, IntPtr.Zero);
                    previewZone.Image = screenshot;
                    originalImage = (Bitmap)screenshot;
                    currentImage = originalImage;

                    //string fileName = $"{savePath}\\screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    SaveImageToFile(savePath, screenshot);
                    Clipboard.SetImage((Bitmap)screenshot.Clone());
                    SendNotification("Screenshot saved to clipboard. You can now open editor to edit it.", "Screenshot Taken!");
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.Message + "\n\n " +
                    "The application will now Quit.", "An error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            finally
            {
               // Application.Exit();
            }
            
        }

        //load image

        /// <summary>
        /// Loads and image when the application starts, it accepts an imagePath which is obtained
        /// from the main method, when the user right clicks on a supported image and clicks Open with
        /// GSCapture on the context menu. It sets the preview zOne image to the obtained image
        /// </summary>
        /// <param name="imagePath"></param>
        private void LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                this.previewZone.Image = Image.FromFile(imagePath);
            }
            else
            {
                MessageBox.Show("File not found: " + imagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // HOTKEYS
        /// <summary>
        /// Initialises the keyboard shotcust
        /// </summary>
        private void InitializeHotKey()
        {
            //hookId = SetHook(HookCallback);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //UnhookWindowsHookEx(hookId);
        }

        //private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        //private IntPtr SetHook(LowLevelKeyboardProc proc)
        //{
        //    using (Process curProcess = Process.GetCurrentProcess())
        //    using (ProcessModule curModule = curProcess.MainModule)
        //    {
        //        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
        //            GetModuleHandle(curModule.ModuleName), 0);
        //    }
        //}

        //private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        //{
        //    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        //    {
        //        int vkCode = Marshal.ReadInt32(lParam);
        //        Keys key = (Keys)vkCode;

        //        // Check for Ctrl + Shift + S hotkey (change as needed)
        //        if (key == Keys.S && Control.ModifierKeys == (Keys.Control | Keys.Shift))
        //        {
        //            using (var overlay = new OverlayForm())
        //            {
        //                if (overlay.ShowDialog() == DialogResult.OK)
        //                {
        //                    var captureArea = overlay.CaptureArea;
        //                    TakeScreenShot(captureArea);
        //                }
        //            }
        //            // Prevent further processing of this key event
        //            return (IntPtr)1;
        //        }
        //    }

        //    return CallNextHookEx(hookId, nCode, wParam, lParam);
        //}

        //#region Win32 API Declarations

        //private const int WH_KEYBOARD_LL = 13;
        //private const int WM_KEYDOWN = 0x0100;

        //private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern IntPtr GetModuleHandle(string lpModuleName);

        //#endregion
        /// <summary>
        /// Captures the active window
        /// </summary>
        private void CaptureActiveWindow()
        {
            IntPtr hWnd = GetForegroundWindow();

            if(hWnd != IntPtr.Zero)
            {
                RECT windowRect;

                if(GetWindowRect(hWnd, out windowRect))
                {
                    int width = windowRect.Right - windowRect.Left;
                    int height = windowRect.Bottom - windowRect.Top;

                    using (Bitmap bitmap = new Bitmap(width, height))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0, new Size(width, height));
                        }

                        previewZone.Image = bitmap;
                    }
                }
            }
        }


        /// <summary>
        /// saves the image file to a desired location.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="image"></param>
        private void SaveImageToFile(string fileName, Image image)
        {
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp |All Files (*.*) | *.*";
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap bmp = new Bitmap(image))
                {
                    bmp.Save(saveImageDialog.FileName, ImageFormat.Png);
                    saveImageDialog.AddExtension = true;
                    Clipboard.SetImage(bmp);
                }
            }

        }


        /// <summary>
        /// Sends the notification to the user informing.
        /// </summary>
        private void SendNotification(string message, string title)
        {
            ToastContent content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },

                            new AdaptiveText()
                            {
                                Text = message
                            }
                        }
                    }
                }
            };

            ToastNotification toast = new ToastNotification(content.GetXml());
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        private void ApplyGrayscaleFilter()
        {
            for (int y = 0; y < currentImage.Height; y++)
            {
                for (int x = 0; x < currentImage.Width; x++)
                {
                    Color pixelColor = currentImage.GetPixel(x, y);
                    int grayValue = (int)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                    currentImage.SetPixel(x, y, grayColor);
                }
            }
        }
        private void ApplyBrightnessFilter(float brightness)
        {
            for (int y = 0; y < currentImage.Height; y++)
            {
                for (int x = 0; x < currentImage.Width; x++)
                {
                    Color pixelColor = currentImage.GetPixel(x, y);
                    int r = (int)(pixelColor.R * brightness);
                    int g = (int)(pixelColor.G * brightness);
                    int b = (int)(pixelColor.B * brightness);
                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));
                    Color newColor = Color.FromArgb(r, g, b);
                    currentImage.SetPixel(x, y, newColor);
                }
            }
        }
        private void ApplyContrastFilter(float contrast)
        {
            float adjustment = (100.0f - contrast) / 100.0f;
            float translate = 128.0f * adjustment;
            float scale = contrast / 100.0f;

            for (int y = 0; y < currentImage.Height; y++)
            {
                for (int x = 0; x < currentImage.Width; x++)
                {
                    Color pixelColor = currentImage.GetPixel(x, y);
                    int r = (int)(scale * (pixelColor.R - 128) + 128 + translate);
                    int g = (int)(scale * (pixelColor.G - 128) + 128 + translate);
                    int b = (int)(scale * (pixelColor.B - 128) + 128 + translate);
                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));
                    Color newColor = Color.FromArgb(r, g, b);
                    currentImage.SetPixel(x, y, newColor);
                }
            }
        }

        // Screen Recording Functionalities
        private void StartRecordingVideo(Rectangle captureArea)
        {
            videoWriter = new VideoFileWriter();
            videoWriter.Open(videoOutputPath, captureArea.Width, captureArea.Height,10, VideoCodec.MPEG4);
            captureTimer.Start();
            buttonStartRecording.Enabled = false;
            buttonStopRecording.Enabled = true;
            isRecording = true;
        }

        private void StopRecordingVideo()
        {
            captureTimer.Stop();
            videoWriter.Close();
            videoWriter.Dispose();
            buttonStartRecording.Enabled=true;
            isRecording = false;

            SendNotification("Video saved to " + videoOutputPath + ", you can now edit it", "Recording Completed");

            MessageBox.Show("Recording saved to " + videoOutputPath, "Recording Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void activeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureActiveWindow();
        }

        private void buttonRotate_Click(object sender, EventArgs e)
        {
            if(screenshot != null)
            {
                screenshot.RotateFlip(RotateFlipType.Rotate90FlipNone);
                previewZone.Image = screenshot;
            }
            else
            {
                MessageBox.Show("Please capture an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void openEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorPanel.Visible = true;
        }

        private void buttonCloseEditor_Click(object sender, EventArgs e)
        {
            editorPanel.Visible = false;
        }

        private void saveCurrentImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentImage != null)
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ImageFormat format = ImageFormat.Png; // Default to PNG
                        switch (saveDialog.FilterIndex)
                        {
                            case 2:
                                format = ImageFormat.Jpeg;
                                break;
                            case 3:
                                format = ImageFormat.Bmp;
                                break;
                        }
                        currentImage.Save(saveDialog.FileName, format);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please capture an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cbGrayScale_CheckedChanged(object sender, EventArgs e)
        {
            if (cbGrayScale.Checked)
            {
                ApplyGrayscaleFilter();
                previewZone.Image = currentImage;
            }
            else
            {
                previewZone.Image = originalImage;
            }
        }

        private void valueBrightness_ValueChanged(object sender, EventArgs e)
        {
            brightnessValue = (float)valueBrightness.Value;
            if (currentImage != null)
            {
                float brightness = brightnessValue;
                ApplyBrightnessFilter(brightness);
                previewZone.Image = currentImage;
            }
        }

        private void valueContrast_ValueChanged(object sender, EventArgs e)
        {
            contrastValue = (float)valueContrast.Value /5;
            if (currentImage != null)
            {
                float contrast = contrastValue; 
                ApplyContrastFilter(contrast);
                previewZone.Image = currentImage;
            }
            Console.WriteLine(contrastValue);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control && e.KeyCode == Keys.S)
            {
                TakeScreenShot(Screen.PrimaryScreen.Bounds);
            }else if(e.KeyCode == Keys.G)
            {
                Console.WriteLine($"{e.KeyValue}, {e.KeyCode}, {e.KeyData}");
            }
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TakeScreenShot(Screen.PrimaryScreen.Bounds);
        }

        private void hotKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("" +
                "Ctrl + S - Capture fullscreen when app is open.\n" +
                "Ctrl + Shift + S - Initialize Capture when app is minimised.\n", "Hot Keys", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void buttonStartRecording_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Video Files | *.mp4;|*.avi;|*.mkv";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                videoOutputPath = dialog.FileName;
                videoArea = Screen.PrimaryScreen.Bounds;

                StartRecordingVideo(videoArea);

            }
        }

        private void buttonStopRecording_Click(object sender, EventArgs e)
        {
            StopRecordingVideo();
        }

        private void fullScreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            buttonStartRecording_Click(sender, EventArgs.Empty);
        }

        private void customRegionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Video Files | *.mp4;|*.avi;|*.mkv";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                videoOutputPath = dialog.FileName;

                using (var overlay = new OverlayForm())
                {
                    if (overlay.ShowDialog() == DialogResult.OK)
                    {
                        videoArea = overlay.CaptureArea;
                        StartRecordingVideo(videoArea);
                    }
                }

            }
        }

        private void termsOfServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Views.License license = new Views.License();
            license.Show();
        }

        private void privacyStatementsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void versionHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About about = new About();

            about.Show();
           
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                string imageToOpen = dialog.FileName;
                screenshot = new Bitmap(imageToOpen);
                originalImage = new Bitmap(imageToOpen);
                currentImage = new Bitmap(originalImage);
                previewZone.ImageLocation = imageToOpen;
            }
            
        }

    }
}
