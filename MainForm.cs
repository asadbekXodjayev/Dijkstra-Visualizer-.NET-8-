using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class Node
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public bool IsTarget { get; set; }
        public bool IsSource { get; set; }
        public bool IsVisited { get; set; }
        public bool IsCurrent { get; set; }
        public double Distance { get; set; }
        public int PreviousNodeId { get; set; }
        public List<int> AlternativePreviousNodes { get; set; }

        public Node(int id, Point pos)
        {
            Id = id;
            Position = pos;
            IsTarget = false;
            IsSource = false;
            IsVisited = false;
            IsCurrent = false;
            Distance = double.PositiveInfinity;
            PreviousNodeId = -1;
            AlternativePreviousNodes = new List<int>();
        }

        public void Reset()
        {
            IsVisited = false;
            IsCurrent = false;
            Distance = double.PositiveInfinity;
            PreviousNodeId = -1;
            AlternativePreviousNodes.Clear();
        }
    }

    public class Edge
    {
        public int FromNodeId { get; set; }
        public int ToNodeId { get; set; }
        public int Weight { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsTentative { get; set; }
        public bool IsAlternativePath { get; set; }

        public Edge(int from, int to, int weight)
        {
            FromNodeId = from;
            ToNodeId = to;
            Weight = weight;
            IsHighlighted = false;
            IsTentative = false;
            IsAlternativePath = false;
        }

        public void Reset()
        {
            IsHighlighted = false;
            IsTentative = false;
            IsAlternativePath = false;
        }
    }

    public enum OperationMode { None, AddNode, AddEdge, SetSource, SetTarget }

    public class MainForm : Form
    {
        private Panel canvasPanel;
        private Panel controlPanel;
        private Panel headerPanel;
        private Label lblStatus;
        private Label lblMode;
        private Label lblTitle;
        private Label lblUsername;
        private Button btnAddNode;
        private Button btnAddEdge;
        private Button btnSetSource;
        private Button btnSetTarget;
        private Button btnRunDijkstra;
        private Button btnClearViz;
        private Button btnClearGraph;
        private Button btnCloseApp;
        private Button btnMinimizeApp;

        private string userName;
        private OperationMode currentMode;
        private Dictionary<int, Node> nodes;
        private List<Edge> edges;
        private int nextNodeId;
        private int? firstEdgeNodeId;
        private int? sourceNodeId;
        private int? targetNodeId;
        private System.Windows.Forms.Timer dijkstraTimer;
        private bool isRunning;
        private List<int> unvisitedNodes;
        private Node currentProcessingNode;
        private Point dragPoint;

        // Enhanced Color Palette
        private Color bgColor = Color.FromArgb(20, 20, 35);
        private Color glassColor = Color.FromArgb(28, 38, 68);
        private Color glassLight = Color.FromArgb(35, 50, 90);
        private Color accentPrimary = Color.FromArgb(20, 60, 110);
        private Color accentPrimaryHover = Color.FromArgb(30, 80, 140);
        private Color accentSecondary = Color.FromArgb(243, 79, 106);
        private Color accentCyan = Color.FromArgb(0, 227, 255);
        private Color accentCyanHover = Color.FromArgb(50, 240, 255);
        private Color accentGreen = Color.FromArgb(0, 255, 146);
        private Color accentPurple = Color.FromArgb(138, 43, 226);
        private Color accentOrange = Color.FromArgb(255, 140, 0);
        private Color textPrimary = Color.FromArgb(255, 255, 255);
        private Color textSecondary = Color.FromArgb(186, 186, 186);
        private Color textMuted = Color.FromArgb(120, 120, 140);
        private Color canvasBgColor = Color.FromArgb(25, 35, 60);
        private Color canvasGridColor = Color.FromArgb(35, 45, 70);

        public MainForm(string username)
        {
            userName = username;
            InitializeComponent();
            InitializeGraph();
        }

        private void InitializeComponent()
        {
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Dijkstra Path Finder";
            this.BackColor = bgColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = false;
            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;

            // Header Panel
            headerPanel = new Panel();
            headerPanel.Location = new Point(0, 0);
            headerPanel.Size = new Size(this.Width, 50);
            headerPanel.BackColor = glassColor;
            headerPanel.Paint += HeaderPanel_Paint;

            // Title Label
            lblTitle = new Label();
            lblTitle.Text = "🔷 Dijkstra Path Finder";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = textPrimary;
            lblTitle.Location = new Point(20, 12);
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(350, 30);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Username Label
            lblUsername = new Label();
            lblUsername.Text = $"User: {userName}";
            lblUsername.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblUsername.ForeColor = textSecondary;
            lblUsername.Location = new Point(380, 15);
            lblUsername.AutoSize = true;

            // Window Control Buttons
            btnMinimizeApp = CreateWindowControlButton("-", 880, 12, 35, 25, textSecondary);
            btnMinimizeApp.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnCloseApp = CreateWindowControlButton("✕", 920, 12, 35, 25, accentSecondary);
            btnCloseApp.Click += (s, e) => this.Close();

            // Get screen size for full canvas
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Canvas Panel with Glassmorphic Design - Full Screen
            canvasPanel = new Panel();
            canvasPanel.Location = new Point(30, 120);
            canvasPanel.Size = new Size(screenWidth - 60, screenHeight - 200);
            canvasPanel.BackColor = canvasBgColor;
            SetDoubleBuffered(canvasPanel, true);
            canvasPanel.Paint += CanvasPanel_Paint;
            canvasPanel.MouseClick += CanvasPanel_MouseClick;

            // Control Panel with Enhanced Design - Full Width
            controlPanel = new Panel();
            controlPanel.Location = new Point(30, 70);
            controlPanel.Size = new Size(screenWidth - 60, 65);
            controlPanel.BackColor = glassColor;
            controlPanel.Paint += ControlPanel_Paint;

            // Buttons with improved layout and styling
            btnAddNode = CreateEnhancedButton("➕ Add Node", 20, 18, 130, 32, accentPrimary, accentCyan);
            btnAddEdge = CreateEnhancedButton("🔗 Add Edge", 160, 18, 130, 32, accentPrimary, accentCyan);
            btnSetSource = CreateEnhancedButton("🟥 Set Source", 300, 18, 135, 32, accentPrimary, accentSecondary);
            btnSetTarget = CreateEnhancedButton("🟩 Set Target", 445, 18, 135, 32, accentPrimary, accentGreen);
            btnRunDijkstra = CreateEnhancedButton("▶ Run Dijkstra", 590, 18, 150, 32, accentCyan, Color.Black);
            btnClearViz = CreateEnhancedButton("🔄 Clear Viz", 750, 18, 130, 32, accentPrimary, textSecondary);
            btnClearGraph = CreateEnhancedButton("🗑 Clear Graph", 890, 18, 130, 32, accentSecondary, textPrimary);

            btnAddNode.Click += (s, e) => SetMode(OperationMode.AddNode);
            btnAddEdge.Click += (s, e) => { if (nodes.Count >= 2) SetMode(OperationMode.AddEdge); else lblStatus.Text = "Add at least 2 nodes first."; };
            btnSetSource.Click += (s, e) => { if (nodes.Count > 0) SetMode(OperationMode.SetSource); else lblStatus.Text = "Add at least 1 node first."; };
            btnSetTarget.Click += (s, e) => { if (nodes.Count > 0) SetMode(OperationMode.SetTarget); else lblStatus.Text = "Add at least 1 node first."; };
            btnRunDijkstra.Click += BtnRunDijkstra_Click;
            btnClearViz.Click += (s, e) => { ResetGraphStates(); sourceNodeId = null; targetNodeId = null; canvasPanel.Invalidate(); lblStatus.Text = "Visualization cleared."; };
            btnClearGraph.Click += (s, e) => { if (MessageBox.Show("Clear entire graph?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { nodes.Clear(); edges.Clear(); nextNodeId = 1; firstEdgeNodeId = null; sourceNodeId = null; targetNodeId = null; SetMode(OperationMode.None); canvasPanel.Invalidate(); lblStatus.Text = "Graph cleared."; } };

            // Calculate dynamic positions based on canvas size
            int canvasBottom = canvasPanel.Bottom;

            // Mode label with icon
            lblMode = new Label();
            lblMode.Location = new Point(30, canvasBottom + 10);
            lblMode.Size = new Size(600, 28);
            lblMode.Text = "Mode: Select an option";
            lblMode.ForeColor = accentCyan;
            lblMode.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            // Status label with glass panel background
            lblStatus = new Label();
            lblStatus.Location = new Point(30, canvasBottom + 45);
            lblStatus.Size = new Size(canvasPanel.Width, 35);
            lblStatus.Text = $"Welcome {userName}! Click 'Add Node' to start building your graph.";
            lblStatus.ForeColor = textPrimary;
            lblStatus.Font = new Font("Segoe UI", 10);
            lblStatus.BackColor = Color.FromArgb(20, 30, 50);
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.Padding = new Padding(10, 5, 10, 5);

            // Add controls to header
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblUsername);
            headerPanel.Controls.Add(btnMinimizeApp);
            headerPanel.Controls.Add(btnCloseApp);

            // Add controls to control panel
            controlPanel.Controls.Add(btnAddNode);
            controlPanel.Controls.Add(btnAddEdge);
            controlPanel.Controls.Add(btnSetSource);
            controlPanel.Controls.Add(btnSetTarget);
            controlPanel.Controls.Add(btnRunDijkstra);
            controlPanel.Controls.Add(btnClearViz);
            controlPanel.Controls.Add(btnClearGraph);

            // Add all panels to form
            this.Controls.Add(headerPanel);
            this.Controls.Add(controlPanel);
            this.Controls.Add(canvasPanel);
            this.Controls.Add(lblMode);
            this.Controls.Add(lblStatus);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw gradient background
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Rectangle(0, 0, headerPanel.Width, 50),
                glassColor, glassLight, LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, headerPanel.ClientRectangle);
            }

            // Draw bottom border
            using (Pen pen = new Pen(accentPrimary, 2))
            {
                g.DrawLine(pen, 0, 49, headerPanel.Width, 49);
            }
        }

        private void ControlPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw rounded corners effect
            using (GraphicsPath path = CreateRoundedPath(controlPanel.ClientRectangle, 10))
            {
                using (Brush brush = new LinearGradientBrush(
                    controlPanel.ClientRectangle, glassColor, glassLight, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(accentPrimary, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Button CreateWindowControlButton(string text, int x, int y, int width, int height, Color backColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, height);
            btn.BackColor = backColor;
            btn.ForeColor = textPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Button CreateEnhancedButton(string text, int x, int y, int width, int height, Color backColor, Color hoverColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, height);
            btn.BackColor = backColor;
            btn.ForeColor = textPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;

            // Store hover color in Tag for use in MouseEnter/Leave events
            btn.Tag = hoverColor;

            btn.MouseEnter += (s, e) =>
            {
                Button b = (Button)s;
                b.BackColor = (Color)b.Tag;
            };
            btn.MouseLeave += (s, e) =>
            {
                Button b = (Button)s;
                b.BackColor = backColor;
            };

            return btn;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragPoint = new Point(e.X, e.Y);
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - dragPoint.X, this.Location.Y + e.Y - dragPoint.Y);
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            dragPoint = Point.Empty;
        }


        private void InitializeGraph()
        {
            currentMode = OperationMode.None;
            nodes = new Dictionary<int, Node>();
            edges = new List<Edge>();
            nextNodeId = 1;
            firstEdgeNodeId = null;
            sourceNodeId = null;
            targetNodeId = null;
            isRunning = false;
            lblStatus.Text = $"Welcome {userName}! Click 'Add Node' to start building your graph.";
        }

        private void SetMode(OperationMode mode)
        {
            currentMode = mode;
            firstEdgeNodeId = null;
            switch (mode)
            {
                case OperationMode.AddNode:
                    lblMode.Text = "Mode: Click on canvas to add a node";
                    lblMode.ForeColor = accentCyan;
                    break;
                case OperationMode.AddEdge:
                    lblMode.Text = "Mode: Click two nodes to connect them";
                    lblMode.ForeColor = accentCyan;
                    break;
                case OperationMode.SetSource:
                    lblMode.Text = "Mode: Click a node to set as source";
                    lblMode.ForeColor = accentCyan;
                    break;
                case OperationMode.SetTarget:
                    lblMode.Text = "Mode: Click a node to set as target";
                    lblMode.ForeColor = accentCyan;
                    break;
                default:
                    lblMode.Text = "Mode: Select an option";
                    lblMode.ForeColor = textSecondary;
                    break;
            }
        }

        private void DrawNode(Graphics g, Node node)
        {
            int centerX = node.Position.X;
            int centerY = node.Position.Y;
            int radius = 28;

            // Determine node color based on state
            Color nodeColor = node.IsSource ? accentSecondary :
                             node.IsTarget ? accentGreen :
                             node.IsCurrent ? Color.FromArgb(255, 215, 0) :
                             node.IsVisited ? accentCyan : Color.FromArgb(100, 70, 160);

            // Draw outer glow for special states
            if (node.IsSource || node.IsTarget || node.IsCurrent)
            {
                using (Pen glowPen = new Pen(node.IsSource ? accentSecondary :
                                              node.IsTarget ? accentGreen : Color.FromArgb(255, 215, 0), 6))
                {
                    glowPen.EndCap = LineCap.Round;
                    g.DrawEllipse(glowPen, centerX - radius - 3, centerY - radius - 3, (radius * 2) + 6, (radius * 2) + 6);
                }
            }

            // Draw gradient fill for node
            Rectangle nodeRect = new Rectangle(centerX - radius, centerY - radius, radius * 2, radius * 2);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(nodeRect);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = ControlPaint.Light(nodeColor);
                    brush.SurroundColors = new Color[] { nodeColor };
                    g.FillPath(brush, path);
                }
            }

            // Draw inner border
            using (Pen innerPen = new Pen(Color.White, 2))
            {
                g.DrawEllipse(innerPen, nodeRect);
            }

            // Draw outer border
            using (Pen outerPen = new Pen(accentPrimary, 1))
            {
                g.DrawEllipse(outerPen, centerX - radius - 1, centerY - radius - 1, radius * 2 + 2, radius * 2 + 2);
            }

            // Draw label with shadow
            string labelText = node.IsSource ? "S" : node.IsTarget ? "T" : node.Id.ToString();
            using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(labelText, font);

                // Draw shadow
                g.DrawString(labelText, font, Brushes.Black,
                    centerX - textSize.Width / 2 + 1, centerY - textSize.Height / 2 + 1);

                // Draw main text
                g.DrawString(labelText, font, Brushes.White,
                    centerX - textSize.Width / 2, centerY - textSize.Height / 2);
            }

            // Draw distance for visited nodes
            if (node.IsVisited && node.Distance != double.PositiveInfinity)
            {
                string distText = $"d={node.Distance}";
                using (Font distFont = new Font("Segoe UI", 7, FontStyle.Regular))
                {
                    SizeF distSize = g.MeasureString(distText, distFont);
                    g.DrawString(distText, distFont, new SolidBrush(accentCyan),
                        centerX - distSize.Width / 2, centerY + radius + 2);
                }
            }
        }

        private void DrawEdge(Graphics g, Edge edge, Node fromNode, Node toNode)
        {
            Color edgeColor;
            float penWidth;
            bool dashed = false;

            if (edge.IsHighlighted)
            {
                edgeColor = accentGreen;
                penWidth = 5;
            }
            else if (edge.IsAlternativePath)
            {
                edgeColor = accentOrange;
                penWidth = 3;
                dashed = true;
            }
            else if (edge.IsTentative)
            {
                edgeColor = Color.FromArgb(255, 165, 0);
                penWidth = 3;
            }
            else
            {
                edgeColor = Color.FromArgb(80, 90, 120);
                penWidth = 2;
            }

            // Draw edge with shadow
            using (Pen shadowPen = new Pen(Color.Black, penWidth))
            {
                shadowPen.StartCap = LineCap.Round;
                shadowPen.EndCap = LineCap.Round;
                g.DrawLine(shadowPen, fromNode.Position.X + 1, fromNode.Position.Y + 1,
                          toNode.Position.X + 1, toNode.Position.Y + 1);
            }

            using (Pen pen = new Pen(edgeColor, penWidth))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                if (dashed)
                {
                    pen.DashStyle = DashStyle.Dash;
                }
                g.DrawLine(pen, fromNode.Position, toNode.Position);
            }

            // Draw arrowhead at destination (skip for dashed alternative paths)
            if (!dashed)
            {
                DrawArrowhead(g, fromNode.Position, toNode.Position, edgeColor, penWidth);
            }

            // Draw weight in styled box at midpoint
            PointF midPoint = new PointF((fromNode.Position.X + toNode.Position.X) / 2,
                                         (fromNode.Position.Y + toNode.Position.Y) / 2);

            string weightText = edge.Weight.ToString();
            using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(weightText, font);
                int boxWidth = (int)textSize.Width + 12;
                int boxHeight = (int)textSize.Height + 6;

                Rectangle boxRect = new Rectangle(
                    (int)(midPoint.X - boxWidth / 2),
                    (int)(midPoint.Y - boxHeight / 2),
                    boxWidth, boxHeight);

                // Draw weight background box
                using (Brush boxBrush = new SolidBrush(bgColor))
                {
                    g.FillEllipse(boxBrush, boxRect.X - 5, boxRect.Y - 3, boxWidth + 10, boxHeight + 6);
                }

                // Draw weight text with appropriate color
                Color weightColor = edge.IsHighlighted ? accentGreen :
                                    edge.IsAlternativePath ? accentOrange :
                                    edge.IsTentative ? Color.FromArgb(255, 165, 0) :
                                    textPrimary;
                using (SolidBrush textBrush = new SolidBrush(weightColor))
                {
                    g.DrawString(weightText, font, textBrush,
                        midPoint.X - textSize.Width / 2, midPoint.Y - textSize.Height / 2);
                }
            }
        }

        private void DrawArrowhead(Graphics g, PointF from, PointF to, Color color, float lineWidth)
        {
            float angle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
            float arrowSize = 12f;

            PointF[] arrowPoints = new PointF[3];
            arrowPoints[0] = to;
            arrowPoints[1] = new PointF(
                to.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                to.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6)
            );
            arrowPoints[2] = new PointF(
                to.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                to.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6)
            );

            using (Brush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, arrowPoints);
            }
        }

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(canvasBgColor);

            // Draw grid pattern
            DrawGrid(g, canvasPanel.ClientRectangle);

            // Draw edges first
            foreach (var edge in edges)
            {
                if (nodes.TryGetValue(edge.FromNodeId, out Node fromNode) && nodes.TryGetValue(edge.ToNodeId, out Node toNode))
                {
                    DrawEdge(g, edge, fromNode, toNode);
                }
            }

            // Draw nodes
            foreach (var node in nodes.Values)
            {
                DrawNode(g, node);
            }
        }

        private void DrawGrid(Graphics g, Rectangle rect)
        {
            int gridSize = 30;

            using (Pen gridPen = new Pen(canvasGridColor, 1))
            {
                for (int x = rect.X; x <= rect.Right; x += gridSize)
                {
                    g.DrawLine(gridPen, x, rect.Top, x, rect.Bottom);
                }
                for (int y = rect.Top; y <= rect.Bottom; y += gridSize)
                {
                    g.DrawLine(gridPen, rect.Left, y, rect.Right, y);
                }
            }
        }

        private void CanvasPanel_MouseClick(object sender, MouseEventArgs e)
        {
            Point pos = e.Location;

            switch (currentMode)
            {
                case OperationMode.AddNode:
                    Node newNode = new Node(nextNodeId++, pos);
                    nodes.Add(newNode.Id, newNode);
                    lblStatus.Text = $"Node {newNode.Id} added.";
                    canvasPanel.Invalidate();
                    break;

                case OperationMode.AddEdge:
                    HandleEdgeCreation(pos);
                    break;

                case OperationMode.SetSource:
                    HandleSetSource(pos);
                    break;

                case OperationMode.SetTarget:
                    HandleSetTarget(pos);
                    break;
            }
        }

        private Node FindNodeAtPosition(Point pos)
        {
            foreach (var node in nodes.Values)
            {
                int dx = pos.X - node.Position.X;
                int dy = pos.Y - node.Position.Y;
                if (Math.Sqrt(dx * dx + dy * dy) <= 30) return node;
            }
            return null;
        }

        private void HandleEdgeCreation(Point pos)
        {
            Node clickedNode = FindNodeAtPosition(pos);
            if (clickedNode == null) return;

            if (!firstEdgeNodeId.HasValue)
            {
                firstEdgeNodeId = clickedNode.Id;
                lblStatus.Text = $"Node {clickedNode.Id} selected. Click destination node.";
            }
            else if (clickedNode.Id != firstEdgeNodeId.Value)
            {
                // Ask for edge weight before creating the edge
                using (var weightForm = new Form())
                {
                    weightForm.Text = "Enter Edge Weight";
                    weightForm.Size = new Size(300, 150);
                    weightForm.StartPosition = FormStartPosition.CenterParent;
                    weightForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    weightForm.MaximizeBox = false;

                    var label = new Label();
                    label.Text = "Enter the weight (number) for this edge:";
                    label.Location = new Point(15, 15);
                    label.Size = new Size(250, 25);

                    var weightInput = new TextBox();
                    weightInput.Location = new Point(15, 45);
                    weightInput.Size = new Size(250, 25);
                    weightInput.Text = "1";

                    var okButton = new Button();
                    okButton.Text = "OK";
                    okButton.Location = new Point(15, 80);
                    okButton.Size = new Size(100, 30);
                    okButton.DialogResult = DialogResult.OK;

                    var cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.Location = new Point(140, 80);
                    cancelButton.Size = new Size(100, 30);
                    cancelButton.DialogResult = DialogResult.Cancel;

                    weightForm.Controls.Add(label);
                    weightForm.Controls.Add(weightInput);
                    weightForm.Controls.Add(okButton);
                    weightForm.Controls.Add(cancelButton);
                    weightForm.AcceptButton = okButton;
                    weightForm.CancelButton = cancelButton;

                    if (weightForm.ShowDialog() == DialogResult.OK)
                    {
                        if (int.TryParse(weightInput.Text, out int weight) && weight > 0)
                        {
                            edges.Add(new Edge(firstEdgeNodeId.Value, clickedNode.Id, weight));
                            lblStatus.Text = $"Edge created between {firstEdgeNodeId} and {clickedNode.Id} with weight {weight}.";
                        }
                        else
                        {
                            lblStatus.Text = "Invalid weight. Edge not created. Using default weight of 1.";
                            edges.Add(new Edge(firstEdgeNodeId.Value, clickedNode.Id, 1));
                        }
                    }
                    else
                    {
                        lblStatus.Text = "Edge creation cancelled.";
                    }

                    firstEdgeNodeId = null;
                    canvasPanel.Invalidate();
                }
            }
        }

        private void HandleSetSource(Point pos)
        {
            Node clickedNode = FindNodeAtPosition(pos);
            if (clickedNode == null) return;

            if (sourceNodeId.HasValue && nodes.ContainsKey(sourceNodeId.Value))
                nodes[sourceNodeId.Value].IsSource = false;

            sourceNodeId = clickedNode.Id;
            clickedNode.IsSource = true;
            clickedNode.Distance = 0;
            lblStatus.Text = $"Node {clickedNode.Id} set as source.";
            canvasPanel.Invalidate();
        }

        private void HandleSetTarget(Point pos)
        {
            Node clickedNode = FindNodeAtPosition(pos);
            if (clickedNode == null) return;

            if (targetNodeId.HasValue && nodes.ContainsKey(targetNodeId.Value))
                nodes[targetNodeId.Value].IsTarget = false;

            targetNodeId = clickedNode.Id;
            clickedNode.IsTarget = true;
            lblStatus.Text = $"Node {clickedNode.Id} set as target.";
            canvasPanel.Invalidate();
        }

        private void BtnRunDijkstra_Click(object sender, EventArgs e)
        {
            if (isRunning) return;
            if (nodes.Count < 2) { lblStatus.Text = "Add at least 2 nodes."; return; }
            if (edges.Count == 0) { lblStatus.Text = "Add at least 1 edge."; return; }
            if (!sourceNodeId.HasValue) { lblStatus.Text = "Set a source node."; return; }
            if (!targetNodeId.HasValue) { lblStatus.Text = "Set a target node."; return; }

            ResetGraphStates();
            nodes[sourceNodeId.Value].Distance = 0;
            isRunning = true;

            // Initialize Dijkstra state
            InitializeDijkstra();

            dijkstraTimer = new System.Windows.Forms.Timer { Interval = 300 };
            dijkstraTimer.Tick += DijkstraTimer_Tick;
            dijkstraTimer.Start();
            lblStatus.Text = "Dijkstra running...";
        }

        private void SetDoubleBuffered(Panel panel, bool value)
        {
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.SetProperty,
                null, panel, new object[] { value });
        }

        private void InitializeDijkstra()
        {
            unvisitedNodes = new List<int>();
            foreach (var node in nodes.Values) unvisitedNodes.Add(node.Id);
            currentProcessingNode = null;
        }

        private void DijkstraTimer_Tick(object sender, EventArgs e)
        {
            if (currentProcessingNode != null) currentProcessingNode.IsCurrent = false;

            // Find min distance node
            Node minNode = null;
            double minDist = double.PositiveInfinity;
            foreach (int id in unvisitedNodes)
            {
                if (nodes.TryGetValue(id, out Node node) && node.Distance < minDist)
                {
                    minDist = node.Distance;
                    minNode = node;
                }
            }

            if (minNode == null || minNode.Id == targetNodeId.Value)
            {
                dijkstraTimer.Stop();
                isRunning = false;
                if (minNode != null && minNode.Id == targetNodeId.Value) ShowResult();
                else { lblStatus.Text = "No path found."; }
                return;
            }

            currentProcessingNode = minNode;
            currentProcessingNode.IsCurrent = true;
            currentProcessingNode.IsVisited = true;
            unvisitedNodes.Remove(currentProcessingNode.Id);

            // Relax edges - track all predecessors with same distance
            foreach (var edge in edges)
            {
                if (edge.FromNodeId == currentProcessingNode.Id && nodes.TryGetValue(edge.ToNodeId, out Node neighbor))
                {
                    edge.IsTentative = true;
                    double newDist = currentProcessingNode.Distance + edge.Weight;
                    if (newDist < neighbor.Distance)
                    {
                        neighbor.Distance = newDist;
                        neighbor.PreviousNodeId = currentProcessingNode.Id;
                        neighbor.AlternativePreviousNodes.Clear();
                    }
                    else if (Math.Abs(newDist - neighbor.Distance) < 0.0001 && neighbor.PreviousNodeId != currentProcessingNode.Id)
                    {
                        // Found an alternative path with same distance
                        if (!neighbor.AlternativePreviousNodes.Contains(currentProcessingNode.Id))
                        {
                            neighbor.AlternativePreviousNodes.Add(currentProcessingNode.Id);
                        }
                    }
                }
                else if (edge.ToNodeId == currentProcessingNode.Id && nodes.TryGetValue(edge.FromNodeId, out Node neighbor2))
                {
                    edge.IsTentative = true;
                    double newDist = currentProcessingNode.Distance + edge.Weight;
                    if (newDist < neighbor2.Distance)
                    {
                        neighbor2.Distance = newDist;
                        neighbor2.PreviousNodeId = currentProcessingNode.Id;
                        neighbor2.AlternativePreviousNodes.Clear();
                    }
                    else if (Math.Abs(newDist - neighbor2.Distance) < 0.0001 && neighbor2.PreviousNodeId != currentProcessingNode.Id)
                    {
                        // Found an alternative path with same distance
                        if (!neighbor2.AlternativePreviousNodes.Contains(currentProcessingNode.Id))
                        {
                            neighbor2.AlternativePreviousNodes.Add(currentProcessingNode.Id);
                        }
                    }
                }
            }

            canvasPanel.Invalidate();
        }

        private void FindAndHighlightAlternativePaths()
        {
            // Find all alternative paths from target back to source using BFS
            Queue<int> queue = new Queue<int>();
            HashSet<int> visitedAlternativeNodes = new HashSet<int>();

            queue.Enqueue(targetNodeId.Value);
            visitedAlternativeNodes.Add(targetNodeId.Value);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                if (currentId == sourceNodeId.Value) continue;

                if (!nodes.TryGetValue(currentId, out Node currentNode)) continue;

                // Check main path predecessor
                if (currentNode.PreviousNodeId != -1)
                {
                    foreach (var edge in edges)
                    {
                        if ((edge.FromNodeId == currentNode.PreviousNodeId && edge.ToNodeId == currentId) ||
                            (edge.FromNodeId == currentId && edge.ToNodeId == currentNode.PreviousNodeId))
                        {
                            if (!edge.IsHighlighted)
                            {
                                edge.IsAlternativePath = true;
                            }
                            break;
                        }
                    }

                    if (!visitedAlternativeNodes.Contains(currentNode.PreviousNodeId))
                    {
                        visitedAlternativeNodes.Add(currentNode.PreviousNodeId);
                        queue.Enqueue(currentNode.PreviousNodeId);
                    }
                }

                // Check alternative predecessors
                foreach (int altPrevId in currentNode.AlternativePreviousNodes)
                {
                    foreach (var edge in edges)
                    {
                        if ((edge.FromNodeId == altPrevId && edge.ToNodeId == currentId) ||
                            (edge.FromNodeId == currentId && edge.ToNodeId == altPrevId))
                        {
                            if (!edge.IsHighlighted)
                            {
                                edge.IsAlternativePath = true;
                            }
                            break;
                        }
                    }

                    if (!visitedAlternativeNodes.Contains(altPrevId))
                    {
                        visitedAlternativeNodes.Add(altPrevId);
                        queue.Enqueue(altPrevId);
                    }
                }
            }
        }

        private void ResetGraphStates()
        {
            foreach (var node in nodes.Values) node.Reset();
            foreach (var edge in edges) edge.Reset();
        }

        private void ShowResult()
        {
            Node target = nodes[targetNodeId.Value];
            if (target.Distance == double.PositiveInfinity)
            {
                lblStatus.Text = "No path exists!";
                return;
            }

            // Highlight path
            List<int> path = new List<int>();
            int currentId = targetNodeId.Value;
            while (currentId != -1)
            {
                path.Add(currentId);
                // Guard against a missing node: reading node.PreviousNodeId when
                // TryGetValue failed would dereference a null reference.
                if (!nodes.TryGetValue(currentId, out Node node))
                    break;
                node.IsVisited = true;
                currentId = node.PreviousNodeId;
            }

            foreach (var edge in edges)
            {
                if (path.Contains(edge.FromNodeId) && path.Contains(edge.ToNodeId))
                    edge.IsHighlighted = true;
            }

            canvasPanel.Invalidate();

            // Find and highlight alternative paths
            FindAndHighlightAlternativePaths();
            canvasPanel.Invalidate();

            // Count alternative path edges
            int alternativeEdgeCount = edges.Count(e => e.IsAlternativePath);

            // Show result dialog
            string pathStr = string.Join(" → ", path.AsEnumerable().Reverse());
            string altPathInfo = alternativeEdgeCount > 0 ?
                $"\n\n💡 {alternativeEdgeCount} alternative path(s) found with same distance (shown with dashed orange lines)" :
                "\n\n✓ No alternative paths with same distance";

            MessageBox.Show($"Shortest Path Found!\n\nDistance: {target.Distance}\nPath: {pathStr}\nNodes in Path: {path.Count}{altPathInfo}",
                "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}