using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCRClient.Forms
{
    public partial class CameraForm : Form
    {
        public CameraForm()
        {
            InitializeComponent();
            ApplyModernStyle();

            LoadCameras();
        }

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap capturedImage;

        private void LoadCameras()
        {
            videoDevices =
                new FilterInfoCollection(
                    FilterCategory.VideoInputDevice);

            cboCamera.Items.Clear();

            foreach (FilterInfo device in videoDevices)
            {
                cboCamera.Items.Add(device.Name);
            }

            if (cboCamera.Items.Count > 0)
            {
                cboCamera.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show(
                    "Không tìm thấy camera.",
                    "Camera",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void StartCamera()
        {
            if (videoDevices == null ||
                videoDevices.Count == 0)
                return;

            if (videoSource != null &&
                videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }

            videoSource =
                new VideoCaptureDevice(
                    videoDevices[cboCamera.SelectedIndex].MonikerString);

            videoSource.NewFrame += VideoSource_NewFrame;

            videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = null;

            try
            {
                frame = (Bitmap)eventArgs.Frame.Clone();

                if (picCamera.InvokeRequired)
                {
                    picCamera.BeginInvoke(
                        new Action<Bitmap>(UpdateCameraImage),
                        frame);
                }
                else
                {
                    UpdateCameraImage(frame);
                }
            }
            catch
            {
                frame?.Dispose();
            }
        }

        private void UpdateCameraImage(Bitmap image)
        {
            if (picCamera.Image != null)
            {
                picCamera.Image.Dispose();
            }

            picCamera.Image = image;
        }

        private void cboCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartCamera();
        }

        private void btnCapture_Click(
    object sender,
    EventArgs e)
        {
            if (picCamera.Image == null)
            {
                MessageBox.Show(
                    "Camera chưa có hình ảnh.",
                    "Camera",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            capturedImage?.Dispose();

            capturedImage =
                new Bitmap(picCamera.Image);

            DialogResult = DialogResult.OK;

            Close();
        }

        private void CameraForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCamera();
        }

        private void StopCamera()
        {
            if (videoSource != null &&
                videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }

            videoSource = null;
        }

        public Bitmap CapturedImage
        {
            get
            {
                return capturedImage;
            }
        }
    }

}
