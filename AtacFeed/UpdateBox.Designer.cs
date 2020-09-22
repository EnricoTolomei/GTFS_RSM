namespace AtacFeed
{
    partial class UpdateBox
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelResultConfUpdate = new System.Windows.Forms.Label();
            this.buttonRestart = new System.Windows.Forms.Button();
            this.buttonAggiorna = new System.Windows.Forms.Button();
            this.labelUpdateConf = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelLastVersion = new System.Windows.Forms.Label();
            this.labelActualVersion = new System.Windows.Forms.Label();
            this.linkHome = new System.Windows.Forms.LinkLabel();
            this.linkUrlDownload = new System.Windows.Forms.LinkLabel();
            this.labelUpdateProgram = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelConfResult = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 389);
            this.panel1.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.labelConfResult);
            this.groupBox2.Controls.Add(this.labelResultConfUpdate);
            this.groupBox2.Controls.Add(this.buttonRestart);
            this.groupBox2.Controls.Add(this.buttonAggiorna);
            this.groupBox2.Controls.Add(this.labelUpdateConf);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(12, 153);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(476, 191);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "File di configurazione";
            // 
            // labelResultConfUpdate
            // 
            this.labelResultConfUpdate.AutoSize = true;
            this.labelResultConfUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResultConfUpdate.Location = new System.Drawing.Point(19, 120);
            this.labelResultConfUpdate.Name = "labelResultConfUpdate";
            this.labelResultConfUpdate.Size = new System.Drawing.Size(375, 30);
            this.labelResultConfUpdate.TabIndex = 3;
            this.labelResultConfUpdate.Text = "Il download dei file di configurazione è terminato.\r\nLa configurazione sarà utili" +
    "zzata al prossimo riavvio edl programma";
            this.labelResultConfUpdate.Visible = false;
            // 
            // buttonRestart
            // 
            this.buttonRestart.Image = global::AtacFeed.Properties.Resources.power_2_16;
            this.buttonRestart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonRestart.Location = new System.Drawing.Point(187, 159);
            this.buttonRestart.Name = "buttonRestart";
            this.buttonRestart.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonRestart.Size = new System.Drawing.Size(126, 26);
            this.buttonRestart.TabIndex = 0;
            this.buttonRestart.Text = "Riavvia Ora";
            this.buttonRestart.UseVisualStyleBackColor = true;
            this.buttonRestart.Visible = false;
            this.buttonRestart.Click += new System.EventHandler(this.buttonRestart_Click);
            // 
            // buttonAggiorna
            // 
            this.buttonAggiorna.Image = global::AtacFeed.Properties.Resources.cloud_download_16;
            this.buttonAggiorna.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonAggiorna.Location = new System.Drawing.Point(187, 82);
            this.buttonAggiorna.Name = "buttonAggiorna";
            this.buttonAggiorna.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.buttonAggiorna.Size = new System.Drawing.Size(126, 26);
            this.buttonAggiorna.TabIndex = 0;
            this.buttonAggiorna.Text = "Scarica";
            this.buttonAggiorna.UseVisualStyleBackColor = true;
            this.buttonAggiorna.Click += new System.EventHandler(this.ButtonUpdate_ClickAsync);
            // 
            // labelUpdateConf
            // 
            this.labelUpdateConf.AutoSize = true;
            this.labelUpdateConf.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUpdateConf.Location = new System.Drawing.Point(6, 17);
            this.labelUpdateConf.MinimumSize = new System.Drawing.Size(462, 0);
            this.labelUpdateConf.Name = "labelUpdateConf";
            this.labelUpdateConf.Size = new System.Drawing.Size(462, 15);
            this.labelUpdateConf.TabIndex = 1;
            this.labelUpdateConf.Text = "Verifica versione in corso....";
            this.labelUpdateConf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.labelLastVersion);
            this.groupBox1.Controls.Add(this.labelActualVersion);
            this.groupBox1.Controls.Add(this.linkHome);
            this.groupBox1.Controls.Add(this.linkUrlDownload);
            this.groupBox1.Controls.Add(this.labelUpdateProgram);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(476, 135);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Programma";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(225, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "L\'intero progetto è presente su GitHub";
            // 
            // labelLastVersion
            // 
            this.labelLastVersion.AutoSize = true;
            this.labelLastVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastVersion.Location = new System.Drawing.Point(19, 101);
            this.labelLastVersion.Name = "labelLastVersion";
            this.labelLastVersion.Size = new System.Drawing.Size(92, 15);
            this.labelLastVersion.TabIndex = 1;
            this.labelLastVersion.Text = "Ultima versione";
            // 
            // labelActualVersion
            // 
            this.labelActualVersion.AutoSize = true;
            this.labelActualVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelActualVersion.Location = new System.Drawing.Point(19, 48);
            this.labelActualVersion.Name = "labelActualVersion";
            this.labelActualVersion.Size = new System.Drawing.Size(91, 15);
            this.labelActualVersion.TabIndex = 1;
            this.labelActualVersion.Text = "Versione in uso";
            // 
            // linkHome
            // 
            this.linkHome.AutoSize = true;
            this.linkHome.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkHome.Location = new System.Drawing.Point(332, 73);
            this.linkHome.Name = "linkHome";
            this.linkHome.Size = new System.Drawing.Size(72, 15);
            this.linkHome.TabIndex = 4;
            this.linkHome.TabStop = true;
            this.linkHome.Text = "Home page";
            this.linkHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkHome_LinkClicked);
            // 
            // linkUrlDownload
            // 
            this.linkUrlDownload.AutoSize = true;
            this.linkUrlDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkUrlDownload.Location = new System.Drawing.Point(332, 103);
            this.linkUrlDownload.Name = "linkUrlDownload";
            this.linkUrlDownload.Size = new System.Drawing.Size(63, 15);
            this.linkUrlDownload.TabIndex = 4;
            this.linkUrlDownload.TabStop = true;
            this.linkUrlDownload.Text = "Download";
            this.linkUrlDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkUrlDownload_LinkClicked);
            // 
            // labelUpdateProgram
            // 
            this.labelUpdateProgram.AutoSize = true;
            this.labelUpdateProgram.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUpdateProgram.Location = new System.Drawing.Point(6, 17);
            this.labelUpdateProgram.MinimumSize = new System.Drawing.Size(462, 0);
            this.labelUpdateProgram.Name = "labelUpdateProgram";
            this.labelUpdateProgram.Size = new System.Drawing.Size(462, 15);
            this.labelUpdateProgram.TabIndex = 1;
            this.labelUpdateProgram.Text = "Verifica versione in corso....";
            this.labelUpdateProgram.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AtacFeed.Properties.Resources.github_logo;
            this.pictureBox1.Location = new System.Drawing.Point(273, 75);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(43, 43);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(3, 17);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(494, 13);
            this.progressBar.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.progressBar);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 356);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(500, 33);
            this.panel2.TabIndex = 2;
            // 
            // labelConfResult
            // 
            this.labelConfResult.AutoSize = true;
            this.labelConfResult.Location = new System.Drawing.Point(19, 47);
            this.labelConfResult.Name = "labelConfResult";
            this.labelConfResult.Size = new System.Drawing.Size(334, 15);
            this.labelConfResult.TabIndex = 4;
            this.labelConfResult.Text = "E\' possibile reimpostare i file con l\'ultima versione rilasciata.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(407, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Questa operazione sovrascriverà i file di configurazioni attualmente in uso";
            // 
            // UpdateBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 389);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "UpdateBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verifica Aggiornamenti";
            this.Load += new System.EventHandler(this.UpdateBox_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonAggiorna;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelUpdateProgram;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelLastVersion;
        private System.Windows.Forms.Label labelActualVersion;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkUrlDownload;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelResultConfUpdate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkHome;
        private System.Windows.Forms.Label labelUpdateConf;
        private System.Windows.Forms.Button buttonRestart;
        private System.Windows.Forms.Label labelConfResult;
        private System.Windows.Forms.Label label3;
    }
}