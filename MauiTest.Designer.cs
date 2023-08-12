namespace TestMauiGraphics
{
    partial class MauiTest
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            skglControl1 = new SkiaSharp.Views.Desktop.SKGLControl();
            lblStats = new Label();
            SuspendLayout();
            // 
            // skglControl1
            // 
            skglControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            skglControl1.BackColor = Color.FromArgb(0, 0, 20);
            skglControl1.BorderStyle = BorderStyle.FixedSingle;
            skglControl1.Location = new Point(13, 27);
            skglControl1.Margin = new Padding(4, 3, 4, 3);
            skglControl1.Name = "skglControl1";
            skglControl1.Size = new Size(800, 400);
            skglControl1.TabIndex = 0;
            skglControl1.VSync = true;
            skglControl1.PaintSurface += skglControl1_PaintSurface;
            // 
            // lblStats
            // 
            lblStats.AutoSize = true;
            lblStats.ForeColor = Color.WhiteSmoke;
            lblStats.Location = new Point(691, 9);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(96, 15);
            lblStats.TabIndex = 3;
            lblStats.Text = "Refresh Rate: n/a";
            lblStats.TextAlign = ContentAlignment.MiddleRight;
            // 
            // MauiTest
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(0, 0, 20);
            ClientSize = new Size(826, 439);
            Controls.Add(lblStats);
            Controls.Add(skglControl1);
            Name = "MauiTest";
            Text = "Maui Test";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SkiaSharp.Views.Desktop.SKGLControl skglControl1;
        private Label lblStats;
    }
}