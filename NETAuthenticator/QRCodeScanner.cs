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
            foreach (var cap in videoSource.VideoCapabilities)
            {
                int width = cap.FrameSize.Width;
                int height = cap.FrameSize.Height;

                if (Math.Abs((float)width / height - 4f / 3f) < 0.01f)
                {
                    videoSource.VideoResolution = cap;
                    break;
                }
            }
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

        private void btnCapturar_Click(object sender, EventArgs e)
        {
            if (pictureBoxVideo.Image != null)
            {
                try
                {
                    Bitmap snapshot = new Bitmap(pictureBoxVideo.Image);

                    var result = barcodeReader.Decode(snapshot);
                    if (result != null)
                    {
                        videoSource.SignalToStop();
                        QrCodeDetected?.Invoke(result.Text);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No se detectó ningún código QR en la imagen.");
                    }

                    snapshot.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al procesar la imagen: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("La imagen del video no está disponible.");
            }
        }
    }
}
