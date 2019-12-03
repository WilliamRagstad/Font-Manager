namespace CSGO_Font_Manager
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.apply_button = new System.Windows.Forms.Button();
            this.donate_button = new System.Windows.Forms.Button();
            this.title_label = new System.Windows.Forms.Label();
            this.version_label = new System.Windows.Forms.Label();
            this.fontPreview_richTextBox = new System.Windows.Forms.RichTextBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.remove_button = new System.Windows.Forms.Button();
            this.addFont_button = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(235)))), ((int)(((byte)(244)))));
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 20;
            this.listBox1.Location = new System.Drawing.Point(12, 37);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(252, 160);
            this.listBox1.TabIndex = 1;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_Click);
            this.listBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.fontLibrary_DragDrop);
            this.listBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.fontLibrary_DragEnter);
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(255)))), ((int)(((byte)(3)))));
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkColor = System.Drawing.Color.MediumSpringGreen;
            this.linkLabel1.Location = new System.Drawing.Point(232, 396);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(35, 13);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "About";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // apply_button
            // 
            this.apply_button.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.apply_button.BackColor = System.Drawing.Color.MediumSpringGreen;
            this.apply_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.apply_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.apply_button.Location = new System.Drawing.Point(12, 311);
            this.apply_button.Name = "apply_button";
            this.apply_button.Size = new System.Drawing.Size(252, 41);
            this.apply_button.TabIndex = 10;
            this.apply_button.Text = "Apply Selected Font";
            this.apply_button.UseVisualStyleBackColor = false;
            this.apply_button.Click += new System.EventHandler(this.apply_button_Click);
            // 
            // donate_button
            // 
            this.donate_button.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.donate_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(253)))), ((int)(((byte)(10)))));
            this.donate_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.donate_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.donate_button.ForeColor = System.Drawing.SystemColors.ControlText;
            this.donate_button.Location = new System.Drawing.Point(12, 358);
            this.donate_button.Name = "donate_button";
            this.donate_button.Size = new System.Drawing.Size(252, 29);
            this.donate_button.TabIndex = 11;
            this.donate_button.Text = "Donate ♡";
            this.donate_button.UseVisualStyleBackColor = false;
            this.donate_button.Click += new System.EventHandler(this.donate_button_Click);
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title_label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(235)))), ((int)(((byte)(244)))));
            this.title_label.Location = new System.Drawing.Point(8, 7);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(55, 24);
            this.title_label.TabIndex = 13;
            this.title_label.Text = "[Title]";
            // 
            // version_label
            // 
            this.version_label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.version_label.AutoSize = true;
            this.version_label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.version_label.Location = new System.Drawing.Point(9, 396);
            this.version_label.Name = "version_label";
            this.version_label.Size = new System.Drawing.Size(48, 13);
            this.version_label.TabIndex = 17;
            this.version_label.Text = "[Version]";
            // 
            // fontPreview_richTextBox
            // 
            this.fontPreview_richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fontPreview_richTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(35)))), ((int)(((byte)(36)))));
            this.fontPreview_richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fontPreview_richTextBox.DetectUrls = false;
            this.fontPreview_richTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fontPreview_richTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(235)))), ((int)(((byte)(244)))));
            this.fontPreview_richTextBox.Location = new System.Drawing.Point(12, 235);
            this.fontPreview_richTextBox.Name = "fontPreview_richTextBox";
            this.fontPreview_richTextBox.ReadOnly = true;
            this.fontPreview_richTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.fontPreview_richTextBox.Size = new System.Drawing.Size(252, 67);
            this.fontPreview_richTextBox.TabIndex = 18;
            this.fontPreview_richTextBox.Text = "[Font Preview]";
            this.fontPreview_richTextBox.TextChanged += new System.EventHandler(this.fontPreview_richTextBox_TextChanged);
            // 
            // linkLabel2
            // 
            this.linkLabel2.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(255)))), ((int)(((byte)(3)))));
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.LinkColor = System.Drawing.Color.MediumSpringGreen;
            this.linkLabel2.Location = new System.Drawing.Point(191, 396);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(35, 13);
            this.linkLabel2.TabIndex = 19;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Reset";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(255)))), ((int)(((byte)(3)))));
            this.linkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.LinkColor = System.Drawing.Color.MediumSpringGreen;
            this.linkLabel3.Location = new System.Drawing.Point(130, 396);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(55, 13);
            this.linkLabel3.TabIndex = 20;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "Feedback";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // remove_button
            // 
            this.remove_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.remove_button.BackColor = System.Drawing.Color.Transparent;
            this.remove_button.BackgroundImage = global::CSGO_Font_Manager.Properties.Resources.remove_retro1;
            this.remove_button.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.remove_button.FlatAppearance.BorderSize = 0;
            this.remove_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.remove_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.remove_button.Location = new System.Drawing.Point(242, 9);
            this.remove_button.Name = "remove_button";
            this.remove_button.Size = new System.Drawing.Size(22, 22);
            this.remove_button.TabIndex = 3;
            this.remove_button.UseVisualStyleBackColor = false;
            this.remove_button.Click += new System.EventHandler(this.button2_Click);
            // 
            // addFont_button
            // 
            this.addFont_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addFont_button.BackColor = System.Drawing.Color.Transparent;
            this.addFont_button.BackgroundImage = global::CSGO_Font_Manager.Properties.Resources.add_retro;
            this.addFont_button.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.addFont_button.FlatAppearance.BorderSize = 0;
            this.addFont_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.addFont_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addFont_button.Location = new System.Drawing.Point(215, 9);
            this.addFont_button.Name = "addFont_button";
            this.addFont_button.Size = new System.Drawing.Size(20, 20);
            this.addFont_button.TabIndex = 2;
            this.addFont_button.UseVisualStyleBackColor = false;
            this.addFont_button.Click += new System.EventHandler(this.addFont_button_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 2;
            this.trackBar1.Location = new System.Drawing.Point(12, 203);
            this.trackBar1.Maximum = 5;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(252, 45);
            this.trackBar1.TabIndex = 50;
            this.trackBar1.Value = 3;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(35)))), ((int)(((byte)(36)))));
            this.ClientSize = new System.Drawing.Size(279, 418);
            this.Controls.Add(this.linkLabel3);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.fontPreview_richTextBox);
            this.Controls.Add(this.version_label);
            this.Controls.Add(this.title_label);
            this.Controls.Add(this.donate_button);
            this.Controls.Add(this.apply_button);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.remove_button);
            this.Controls.Add(this.addFont_button);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.trackBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 500);
            this.MinimumSize = new System.Drawing.Size(200, 350);
            this.Name = "Form1";
            this.Text = "Font Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button addFont_button;
        private System.Windows.Forms.Button remove_button;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button apply_button;
        private System.Windows.Forms.Button donate_button;
        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label version_label;
        private System.Windows.Forms.RichTextBox fontPreview_richTextBox;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.TrackBar trackBar1;
    }
}

