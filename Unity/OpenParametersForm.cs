using System;
using System.Drawing;
using System.Windows.Forms;

namespace HeatBot
{
    public class ParametersForm : Form
    {
        private Rectangle selectedRegion;
        private bool isDragging = false;
        private Point dragStartPoint;
        private Bitmap backgroundScreenshot;

        public Rectangle SelectedRegion => selectedRegion;

        public ParametersForm(Rectangle initialRegion)
        {

            selectedRegion = initialRegion;
            backgroundScreenshot = CaptureScreen();
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Kijo STP Met Tes Putain De Pod ici !";
            Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            BackColor = Color.Black;
            Opacity = 1;

            BackgroundImage = backgroundScreenshot;
            BackgroundImageLayout = ImageLayout.Stretch;

            MouseDown += PictureBox_MouseDown;
            MouseMove += PictureBox_MouseMove;
            MouseUp += PictureBox_MouseUp;
            Paint += PictureBox_Paint;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.Black
            };
            Controls.Add(buttonPanel);

            var setButton = new Button
            {
                Text = "Set",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Green,
                AutoSize = true,

            };
            setButton.Click += (s, e) => SaveAndClose();
            buttonPanel.Controls.Add(setButton);

            var cancelButton = new Button
            {
                Text = "Cancel",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Red,
                AutoSize = true,

            };
            cancelButton.Click += (s, e) => Close();
            buttonPanel.Controls.Add(cancelButton);
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
                selectedRegion = new Rectangle();
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {

                var x = Math.Min(dragStartPoint.X, e.X);
                var y = Math.Min(dragStartPoint.Y, e.Y);
                var width = Math.Abs(dragStartPoint.X - e.X);
                var height = Math.Abs(dragStartPoint.Y - e.Y);
                selectedRegion = new Rectangle(x, y, width, height);
                Invalidate();
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen, selectedRegion);
            }
        }

        private Bitmap CaptureScreen()
        {
            try
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                var bitmap = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error capturing screen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void SaveAndClose()
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}