using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GsCapture.Views
{
    public partial class OverlayForm : Form
    {

        private bool isDrawing = false;
        private Point startPoint;
        private Rectangle selectionRectangle;

        public Rectangle CaptureArea { get; private set; }
        public OverlayForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Wheat;
            this.Opacity = 0.9;
            this.TransparencyKey = Color.Wheat;
            this.TopMost = true;
        }

        private void OverlayForm_Load(object sender, EventArgs e)
        {

        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                startPoint = e.Location;
                selectionRectangle = new Rectangle(e.Location, new Size(0, 0));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDrawing)
            {
                int width = Math.Abs(e.X - startPoint.X);
                int height = Math.Abs(e.Y - startPoint.Y);
                selectionRectangle = new Rectangle(
                    Math.Min(e.X, startPoint.X),
                    Math.Min(e.Y, startPoint.Y),
                    width,
                    height);
                Invalidate(); 
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                isDrawing = false;
                CaptureArea = selectionRectangle;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (isDrawing)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRectangle);
                }
            }
        }
    }
}
