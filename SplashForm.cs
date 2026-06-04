using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class SplashForm : Form
    {
        private System.Windows.Forms.Timer animationTimer;
        private int progress = 0;
        private float logoScale = 0.5f;
        private bool fadingOut = false;

        // Color Palette - Glassmorphic Dark Theme
        private Color bgColor = Color.FromArgb(26, 26, 46);
        private Color glassColor = Color.FromArgb(22, 33, 62);
        private Color glassLight = Color.FromArgb(30, 45, 80);
        private Color accentPrimary = Color.FromArgb(15, 52, 96);
        private Color accentSecondary = Color.FromArgb(233, 69, 96);
        private Color accentCyan = Color.FromArgb(0, 217, 255);
        private Color textPrimary = Color.FromArgb(255, 255, 255);
        private Color textSecondary = Color.FromArgb(176, 176, 176);

        public SplashForm()
        {
            InitializeComponent();
            StartAnimation();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = bgColor;
            this.DoubleBuffered = true;
            this.TopMost = true;
        }

        private void StartAnimation()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 30;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!fadingOut)
            {
                progress = Math.Min(progress + 1, 100);
                logoScale = Math.Min(logoScale + 0.02f, 1.2f);
                this.Invalidate();

                if (progress >= 100 && logoScale >= 1.2f)
                {
                    fadingOut = true;
                    animationTimer.Interval = 50;
                }
            }
            else
            {
                this.Opacity -= 0.1;
                if (this.Opacity <= 0)
                {
                    animationTimer.Stop();
                    this.Close();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Draw background
            g.FillRectangle(new SolidBrush(bgColor), ClientRectangle);

            // Draw glass panel
            Rectangle glassRect = new Rectangle(50, 50, 400, 250);
            DrawGlassPanel(g, glassRect);

            // Draw logo
            float centerX = glassRect.X + glassRect.Width / 2;
            float centerY = glassRect.Y + glassRect.Height / 2 - 20;

            using (Brush logoBrush = new SolidBrush(accentCyan))
            {
                g.FillEllipse(logoBrush, centerX - 40 * logoScale, centerY - 40 * logoScale, 
                              80 * logoScale, 80 * logoScale);
            }

            using (Font logoFont = new Font("Segoe UI", 48 * logoScale, FontStyle.Bold))
            {
                SizeF logoSize = g.MeasureString("D", logoFont);
                g.DrawString("D", logoFont, Brushes.White, 
                    centerX - logoSize.Width / 2, centerY - logoSize.Height / 2 - 10);
            }

            // Draw title
            using (Font titleFont = new Font("Segoe UI", 24, FontStyle.Bold))
            {
                SizeF titleSize = g.MeasureString("Dijkstra Path Finder", titleFont);
                g.DrawString("Dijkstra Path Finder", titleFont, Brushes.White,
                    centerX - titleSize.Width / 2, centerY + 30);
            }

            // Draw subtitle
            using (Font subtitleFont = new Font("Segoe UI", 10))
            {
                SizeF subSize = g.MeasureString("Visualizing Shortest Path Algorithm", subtitleFont);
                g.DrawString("Visualizing Shortest Path Algorithm", subtitleFont, 
                    new SolidBrush(textSecondary), centerX - subSize.Width / 2, centerY + 65);
            }

            // Draw progress bar
            Rectangle progressBarBg = new Rectangle(100, 240, 300, 20);
            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 70)), progressBarBg);
            g.DrawRectangle(Pens.Gray, progressBarBg);

            int progressWidth = (int)(300 * progress / 100.0);
            using (Brush progressBrush = new LinearGradientBrush(
                new Rectangle(100, 240, progressWidth, 20), accentCyan, accentPrimary, LinearGradientMode.Horizontal))
            {
                g.FillRectangle(progressBrush, 100, 240, progressWidth, 20);
            }

            // Draw progress text
            using (Font progressFont = new Font("Segoe UI", 9))
            {
                string progressText = $"Loading... {progress}%";
                SizeF progressSize = g.MeasureString(progressText, progressFont);
                g.DrawString(progressText, progressFont, Brushes.White,
                    centerX - progressSize.Width / 2, 245);
            }
        }

        private void DrawGlassPanel(Graphics g, Rectangle rect)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 15;
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, 
                    radius * 2, radius * 2, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();

                using (Brush fillBrush = new LinearGradientBrush(rect, glassColor, glassLight, LinearGradientMode.Vertical))
                {
                    g.FillPath(fillBrush, path);
                }

                using (Pen borderPen = new Pen(accentPrimary, 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }
        }
    }
}