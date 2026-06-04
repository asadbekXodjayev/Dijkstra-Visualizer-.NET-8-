using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblTitle;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblError;
        private Label lblToggleText;

        // Color Palette - Glassmorphic Dark Theme
        private Color bgColor = Color.FromArgb(26, 26, 46);
        private Color glassColor = Color.FromArgb(22, 33, 62);
        private Color glassLight = Color.FromArgb(30, 45, 80);
        private Color accentPrimary = Color.FromArgb(15, 52, 96);
        private Color accentSecondary = Color.FromArgb(233, 69, 96);
        private Color accentCyan = Color.FromArgb(0, 217, 255);
        private Color accentGreen = Color.FromArgb(0, 255, 136);
        private Color textPrimary = Color.FromArgb(255, 255, 255);
        private Color textSecondary = Color.FromArgb(176, 176, 176);
        private Color inputBgColor = Color.FromArgb(20, 30, 50);

        public string Username { get; private set; }
        private bool isLoginMode = true;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Size = new Size(450, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = bgColor;
            this.DoubleBuffered = true;

            // Title label
            lblTitle = new Label();
            lblTitle.Text = "Welcome";
            lblTitle.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTitle.ForeColor = textPrimary;
            lblTitle.Size = new Size(300, 50);
            lblTitle.Location = new Point(75, 30);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // Username label
            lblUsername = new Label();
            lblUsername.Text = "Username";
            lblUsername.Font = new Font("Segoe UI", 10);
            lblUsername.ForeColor = textSecondary;
            lblUsername.Size = new Size(150, 25);
            lblUsername.Location = new Point(50, 100);

            // Username input
            txtUsername = new TextBox();
            txtUsername.Font = new Font("Segoe UI", 12);
            txtUsername.ForeColor = textPrimary;
            txtUsername.BackColor = inputBgColor;
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Size = new Size(350, 30);
            txtUsername.Location = new Point(50, 125);
            txtUsername.TextChanged += TxtUsername_TextChanged;

            // Password label
            lblPassword = new Label();
            lblPassword.Text = "Password";
            lblPassword.Font = new Font("Segoe UI", 10);
            lblPassword.ForeColor = textSecondary;
            lblPassword.Size = new Size(150, 25);
            lblPassword.Location = new Point(50, 175);

            // Password input
            txtPassword = new TextBox();
            txtPassword.Font = new Font("Segoe UI", 12);
            txtPassword.ForeColor = textPrimary;
            txtPassword.BackColor = inputBgColor;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Size = new Size(350, 30);
            txtPassword.Location = new Point(50, 200);
            txtPassword.UseSystemPasswordChar = true;

            // Error label
            lblError = new Label();
            lblError.Text = "";
            lblError.Font = new Font("Segoe UI", 9);
            lblError.ForeColor = accentSecondary;
            lblError.Size = new Size(350, 40);
            lblError.Location = new Point(50, 240);
            lblError.TextAlign = ContentAlignment.MiddleCenter;

            // Login button
            btnLogin = new Button();
            btnLogin.Text = "Login";
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.ForeColor = Color.Black;
            btnLogin.BackColor = accentCyan;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Size = new Size(165, 45);
            btnLogin.Location = new Point(50, 290);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;

            // Register button
            btnRegister = new Button();
            btnRegister.Text = "Register";
            btnRegister.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnRegister.ForeColor = textPrimary;
            btnRegister.BackColor = accentPrimary;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Size = new Size(165, 45);
            btnRegister.Location = new Point(235, 290);
            btnRegister.Cursor = Cursors.Hand;
            btnRegister.Click += BtnRegister_Click;

            // Toggle text label
            lblToggleText = new Label();
            lblToggleText.Text = "Don't have an account?";
            lblToggleText.Font = new Font("Segoe UI", 9);
            lblToggleText.ForeColor = textSecondary;
            lblToggleText.Size = new Size(200, 25);
            lblToggleText.Location = new Point(125, 350);

            // Add controls
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblError);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnRegister);
            this.Controls.Add(lblToggleText);
        }

        private void TxtUsername_TextChanged(object sender, EventArgs e)
        {
            lblError.Text = "";
            txtUsername.BackColor = inputBgColor;
            txtPassword.BackColor = inputBgColor;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (isLoginMode)
            {
                // Login mode
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username))
                {
                    lblError.Text = "Please enter username";
                    txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    lblError.Text = "Please enter password";
                    txtPassword.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                if (AuthHelper.Login(username, password))
                {
                    Username = username;
                    btnLogin.BackColor = accentGreen;
                    btnLogin.Text = "Success!";
                    lblError.Text = "";
                    System.Threading.Thread.Sleep(500);
                    this.Close();
                }
                else
                {
                    lblError.Text = "Invalid username or password";
                    txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                    txtPassword.BackColor = Color.FromArgb(50, 20, 20);
                }
            }
            else
            {
                // Switch to login mode
                isLoginMode = true;
                UpdateUI();
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (!isLoginMode)
            {
                // Register mode
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username))
                {
                    lblError.Text = "Please enter username";
                    txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                if (username.Length < 2)
                {
                    lblError.Text = "Username must be at least 2 characters";
                    txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                if (username.Length > 30)
                {
                    lblError.Text = "Username must be less than 30 characters";
                    txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                foreach (char c in username)
                {
                    if (!char.IsLetterOrDigit(c) && c != '_')
                    {
                        lblError.Text = "Username can only contain letters, numbers, and underscores";
                        txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(password))
                {
                    lblError.Text = "Please enter password";
                    txtPassword.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                if (password.Length < 4)
                {
                    lblError.Text = "Password must be at least 4 characters";
                    txtPassword.BackColor = Color.FromArgb(50, 20, 20);
                    return;
                }

                if (AuthHelper.Register(username, password))
                {
                    Username = username;
                    btnRegister.BackColor = accentGreen;
                    btnRegister.Text = "Success!";
                    lblError.Text = "Registration successful! Logging in...";
                    System.Threading.Thread.Sleep(1000);
                    this.Close();
                }
                else
                {
                    lblError.Text = "Username already exists or invalid input";
                    txtUsername.BackColor = Color.FromArgb(50, 20, 20);
                    txtPassword.BackColor = Color.FromArgb(50, 20, 20);
                }
            }
            else
            {
                // Switch to register mode
                isLoginMode = false;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (isLoginMode)
            {
                lblTitle.Text = "Welcome Back";
                btnLogin.Text = "Login";
                btnLogin.BackColor = accentCyan;
                btnRegister.Text = "Register";
                btnRegister.BackColor = accentPrimary;
                lblToggleText.Text = "Don't have an account?";
                txtUsername.Text = "";
                txtPassword.Text = "";
                lblError.Text = "";
            }
            else
            {
                lblTitle.Text = "Create Account";
                btnLogin.Text = "Login";
                btnLogin.BackColor = accentPrimary;
                btnRegister.Text = "Register";
                btnRegister.BackColor = accentCyan;
                lblToggleText.Text = "Already have an account?";
                txtUsername.Text = "";
                txtPassword.Text = "";
                lblError.Text = "";
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw glass panel
            Rectangle glassRect = new Rectangle(25, 25, 400, 500);
            DrawGlassPanel(g, glassRect);
        }

        private void DrawGlassPanel(Graphics g, Rectangle rect)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 20;
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