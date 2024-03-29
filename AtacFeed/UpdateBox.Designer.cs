﻿namespace AtacFeed
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelUpdateCSV = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonAggiornaCSV = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelConfResult = new System.Windows.Forms.Label();
            this.buttonAggiornaGTFS = new System.Windows.Forms.Button();
            this.labelUpdateGTFS = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelLastVersion = new System.Windows.Forms.Label();
            this.labelActualVersion = new System.Windows.Forms.Label();
            this.linkHome = new System.Windows.Forms.LinkLabel();
            this.linkUrlDownload = new System.Windows.Forms.LinkLabel();
            this.labelUpdateProgram = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelResultConfUpdate = new System.Windows.Forms.Label();
            this.buttonRestart = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.labelResultConfUpdate);
            this.panel1.Controls.Add(this.buttonRestart);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(840, 667);
            this.panel1.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelUpdateCSV);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.buttonAggiornaCSV);
            this.groupBox3.Location = new System.Drawing.Point(16, 335);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Size = new System.Drawing.Size(811, 122);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "File di configurazione - DettagliVettura.csv";
            // 
            // labelUpdateCSV
            // 
            this.labelUpdateCSV.AutoSize = true;
            this.labelUpdateCSV.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUpdateCSV.Location = new System.Drawing.Point(7, 31);
            this.labelUpdateCSV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateCSV.MinimumSize = new System.Drawing.Size(616, 0);
            this.labelUpdateCSV.Name = "labelUpdateCSV";
            this.labelUpdateCSV.Size = new System.Drawing.Size(616, 18);
            this.labelUpdateCSV.TabIndex = 1;
            this.labelUpdateCSV.Text = "Verifica versione in corso....";
            this.labelUpdateCSV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(25, 76);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(362, 16);
            this.label6.TabIndex = 4;
            this.label6.Text = "E\' possibile reimpostare i file con l\'ultima versione rilasciata.";
            // 
            // buttonAggiornaCSV
            // 
            this.buttonAggiornaCSV.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAggiornaCSV.Image = global::AtacFeed.Properties.Resources.cloud_download_16;
            this.buttonAggiornaCSV.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonAggiornaCSV.Location = new System.Drawing.Point(677, 60);
            this.buttonAggiornaCSV.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonAggiornaCSV.Name = "buttonAggiornaCSV";
            this.buttonAggiornaCSV.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.buttonAggiornaCSV.Size = new System.Drawing.Size(125, 32);
            this.buttonAggiornaCSV.TabIndex = 0;
            this.buttonAggiornaCSV.Text = "   Scarica";
            this.buttonAggiornaCSV.UseVisualStyleBackColor = true;
            this.buttonAggiornaCSV.Click += new System.EventHandler(this.ButtonAggiornaCSV_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.labelConfResult);
            this.groupBox2.Controls.Add(this.buttonAggiornaGTFS);
            this.groupBox2.Controls.Add(this.labelUpdateGTFS);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(16, 193);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(811, 119);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "File di configurazione - GTFS Statico";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 71);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(491, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Questa operazione sovrascriverà i file di configurazioni attualmente in uso";
            // 
            // labelConfResult
            // 
            this.labelConfResult.AutoSize = true;
            this.labelConfResult.Location = new System.Drawing.Point(25, 54);
            this.labelConfResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelConfResult.Name = "labelConfResult";
            this.labelConfResult.Size = new System.Drawing.Size(381, 18);
            this.labelConfResult.TabIndex = 4;
            this.labelConfResult.Text = "E\' possibile scaricare l\'ultimo file GFTS Statico rilasciato.";
            // 
            // buttonAggiornaGTFS
            // 
            this.buttonAggiornaGTFS.Image = global::AtacFeed.Properties.Resources.cloud_download_16;
            this.buttonAggiornaGTFS.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonAggiornaGTFS.Location = new System.Drawing.Point(677, 58);
            this.buttonAggiornaGTFS.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonAggiornaGTFS.Name = "buttonAggiornaGTFS";
            this.buttonAggiornaGTFS.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.buttonAggiornaGTFS.Size = new System.Drawing.Size(125, 32);
            this.buttonAggiornaGTFS.TabIndex = 0;
            this.buttonAggiornaGTFS.Text = "   Scarica";
            this.buttonAggiornaGTFS.UseVisualStyleBackColor = true;
            this.buttonAggiornaGTFS.Click += new System.EventHandler(this.ButtonAggiornaGTFS_Click);
            // 
            // labelUpdateGTFS
            // 
            this.labelUpdateGTFS.AutoSize = true;
            this.labelUpdateGTFS.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUpdateGTFS.Location = new System.Drawing.Point(8, 21);
            this.labelUpdateGTFS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateGTFS.MinimumSize = new System.Drawing.Size(616, 0);
            this.labelUpdateGTFS.Name = "labelUpdateGTFS";
            this.labelUpdateGTFS.Size = new System.Drawing.Size(616, 18);
            this.labelUpdateGTFS.TabIndex = 1;
            this.labelUpdateGTFS.Text = "Verifica versione in corso....";
            this.labelUpdateGTFS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.labelLastVersion);
            this.groupBox1.Controls.Add(this.labelActualVersion);
            this.groupBox1.Controls.Add(this.linkHome);
            this.groupBox1.Controls.Add(this.linkUrlDownload);
            this.groupBox1.Controls.Add(this.labelUpdateProgram);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(811, 166);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Programma";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(369, 59);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(258, 18);
            this.label2.TabIndex = 5;
            this.label2.Text = "L\'intero progetto è presente su GitHub";
            // 
            // labelLastVersion
            // 
            this.labelLastVersion.AutoSize = true;
            this.labelLastVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastVersion.Location = new System.Drawing.Point(25, 124);
            this.labelLastVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLastVersion.Name = "labelLastVersion";
            this.labelLastVersion.Size = new System.Drawing.Size(110, 18);
            this.labelLastVersion.TabIndex = 1;
            this.labelLastVersion.Text = "Ultima versione";
            // 
            // labelActualVersion
            // 
            this.labelActualVersion.AutoSize = true;
            this.labelActualVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelActualVersion.Location = new System.Drawing.Point(25, 59);
            this.labelActualVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelActualVersion.Name = "labelActualVersion";
            this.labelActualVersion.Size = new System.Drawing.Size(110, 18);
            this.labelActualVersion.TabIndex = 1;
            this.labelActualVersion.Text = "Versione in uso";
            // 
            // linkHome
            // 
            this.linkHome.AutoSize = true;
            this.linkHome.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkHome.Location = new System.Drawing.Point(477, 92);
            this.linkHome.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkHome.Name = "linkHome";
            this.linkHome.Size = new System.Drawing.Size(85, 18);
            this.linkHome.TabIndex = 4;
            this.linkHome.TabStop = true;
            this.linkHome.Text = "Home page";
            this.linkHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkHome_LinkClicked);
            // 
            // linkUrlDownload
            // 
            this.linkUrlDownload.AutoSize = true;
            this.linkUrlDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkUrlDownload.Location = new System.Drawing.Point(479, 124);
            this.linkUrlDownload.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkUrlDownload.Name = "linkUrlDownload";
            this.linkUrlDownload.Size = new System.Drawing.Size(75, 18);
            this.linkUrlDownload.TabIndex = 4;
            this.linkUrlDownload.TabStop = true;
            this.linkUrlDownload.Text = "Download";
            this.linkUrlDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkUrlDownload_LinkClicked);
            // 
            // labelUpdateProgram
            // 
            this.labelUpdateProgram.AutoSize = true;
            this.labelUpdateProgram.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUpdateProgram.Location = new System.Drawing.Point(105, 21);
            this.labelUpdateProgram.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUpdateProgram.MinimumSize = new System.Drawing.Size(616, 0);
            this.labelUpdateProgram.Name = "labelUpdateProgram";
            this.labelUpdateProgram.Size = new System.Drawing.Size(616, 18);
            this.labelUpdateProgram.TabIndex = 1;
            this.labelUpdateProgram.Text = "Verifica versione in corso....";
            this.labelUpdateProgram.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AtacFeed.Properties.Resources.github_logo;
            this.pictureBox1.Location = new System.Drawing.Point(373, 92);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(57, 53);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // labelResultConfUpdate
            // 
            this.labelResultConfUpdate.AutoSize = true;
            this.labelResultConfUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResultConfUpdate.Location = new System.Drawing.Point(24, 510);
            this.labelResultConfUpdate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelResultConfUpdate.Name = "labelResultConfUpdate";
            this.labelResultConfUpdate.Size = new System.Drawing.Size(461, 36);
            this.labelResultConfUpdate.TabIndex = 3;
            this.labelResultConfUpdate.Text = "Il download è terminato.\r\nLa configurazione sarà utilizzata al prossimo riavvio d" +
    "el monitoraggio\r\n";
            this.labelResultConfUpdate.Visible = false;
            // 
            // buttonRestart
            // 
            this.buttonRestart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRestart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRestart.Image = global::AtacFeed.Properties.Resources.power_2_16;
            this.buttonRestart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonRestart.Location = new System.Drawing.Point(557, 510);
            this.buttonRestart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonRestart.Name = "buttonRestart";
            this.buttonRestart.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.buttonRestart.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buttonRestart.Size = new System.Drawing.Size(261, 46);
            this.buttonRestart.TabIndex = 0;
            this.buttonRestart.Text = "   Riavvia ora il monitoraggio";
            this.buttonRestart.UseVisualStyleBackColor = true;
            this.buttonRestart.Visible = false;
            this.buttonRestart.Click += new System.EventHandler(this.ButtonRestart_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(16, 12);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(811, 16);
            this.progressBar.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.progressBar);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 626);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(840, 41);
            this.panel2.TabIndex = 2;
            // 
            // UpdateBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 667);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "UpdateBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verifica Aggiornamenti";
            this.Shown += new System.EventHandler(this.UpdateBox_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonAggiornaGTFS;
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
        private System.Windows.Forms.Label labelUpdateGTFS;
        private System.Windows.Forms.Button buttonRestart;
        private System.Windows.Forms.Label labelConfResult;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelUpdateCSV;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonAggiornaCSV;
    }
}