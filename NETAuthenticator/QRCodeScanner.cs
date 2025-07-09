using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;

namespace NETAuthenticator
{
    public partial class QRCodeScanner : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private BarcodeReader barcodeReader;

        public event Action<string> QrCodeDetected;
        public QRCodeScanner()
        {
            InitializeComponent();
            barcodeReader = new BarcodeReader
            {
                AutoRotate = true
            };


            // Obtener cámaras disponibles
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No se encontró ninguna cámara.");
                this.Close();
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

                // Mostrar video en PictureBox
                pictureBoxVideo.Image?.Dispose();
                pictureBoxVideo.Image = (Bitmap)bitmap.Clone();

                // Detectar QR
                var result = barcodeReader.Decode(bitmap);
                if (result != null)
                {
                    // QR detectado, enviar evento
                    videoSource.SignalToStop();

                    // Invocar en UI thread
                    this.Invoke(new Action(() =>
                    {
                        QrCodeDetected?.Invoke(result.Text);
                        this.Close();
                    }));
                }
                bitmap.Dispose();
            }
            catch { }
        }

        private void QrScannerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        private void QrScannerForm_Load(object sender, EventArgs e)
        {
            videoSource.Start();
        }
    }
}
