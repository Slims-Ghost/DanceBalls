namespace DanceBalls
{
    partial class SkiaTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            glControl1 = new OpenTK.GLControl();
            lblStats = new Label();
            SuspendLayout();
            // 
            // glControl1
            // 
            glControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            glControl1.BackColor = Color.FromArgb(0, 0, 20);
            glControl1.BorderStyle = BorderStyle.FixedSingle;
            glControl1.Location = new Point(13, 29);
            glControl1.Margin = new Padding(4, 3, 4, 3);
            glControl1.Name = "glControl1";
            glControl1.Size = new Size(800, 400);
            glControl1.TabIndex = 0;
            glControl1.VSync = true;
            glControl1.SizeChanged += glControl1_SizeChanged;
            glControl1.Click += glControl1_Click;
            // 
            // lblStats
            // 
            lblStats.AutoSize = true;
            lblStats.ForeColor = Color.WhiteSmoke;
            lblStats.Location = new Point(691, 9);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(96, 15);
            lblStats.TabIndex = 2;
            lblStats.Text = "Refresh Rate: n/a";
            lblStats.TextAlign = ContentAlignment.MiddleRight;
            // 
            // SkiaTest
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(0, 0, 20);
            ClientSize = new Size(826, 441);
            Controls.Add(lblStats);
            Controls.Add(glControl1);
            MinimumSize = new Size(200, 200);
            Name = "SkiaTest";
            Text = "Skia Test";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OpenTK.GLControl glControl1;
        private Label lblStats;
    }
}