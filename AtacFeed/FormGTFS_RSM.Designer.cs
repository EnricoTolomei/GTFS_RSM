namespace AtacFeed
{
    partial class FormGTFS_RSM
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGTFS_RSM));
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridVetture = new System.Windows.Forms.DataGridView();
            this.idVetturaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.matricolaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.routeIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lineaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gestoreDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currentStopSequenceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.congestionLevelDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.occupancyStatusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tripIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.primaVoltaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ultimaVoltaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rimessaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.euroDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modelloDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Latitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Longitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.extendedVehicleInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button3 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.labelTotaleIdVettura = new System.Windows.Forms.Label();
            this.labelTotaleMatricola = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.labelTotaleRighe = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelFeedLetti = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.labelTot = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.labelTPL = new System.Windows.Forms.Label();
            this.lblOraLettura = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.labelAtac = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.labelWait = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tabGrafico = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.formsPlotTPL = new ScottPlot.FormsPlot();
            this.formsPlotAtac = new ScottPlot.FormsPlot();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBoxExport = new System.Windows.Forms.GroupBox();
            this.checkAlert = new System.Windows.Forms.CheckBox();
            this.checkMonitoraggio = new System.Windows.Forms.CheckBox();
            this.checkXlsx = new System.Windows.Forms.CheckBox();
            this.checkCSV = new System.Windows.Forms.CheckBox();
            this.checkGrafico = new System.Windows.Forms.CheckBox();
            this.groupBoxMonitoraggio = new System.Windows.Forms.GroupBox();
            this.buttonResetRegole = new System.Windows.Forms.Button();
            this.checkBoxStorico = new System.Windows.Forms.CheckBox();
            this.labelRaggruppaAlert = new System.Windows.Forms.Label();
            this.radioNonRaggruppare = new System.Windows.Forms.RadioButton();
            this.checkReset = new System.Windows.Forms.CheckBox();
            this.radioLineaRegola = new System.Windows.Forms.RadioButton();
            this.radioLinea = new System.Windows.Forms.RadioButton();
            this.dateTimeReset = new System.Windows.Forms.DateTimePicker();
            this.checkTripVuoti = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkTripDuplicati = new System.Windows.Forms.CheckBox();
            this.groupBoxServerRSM = new System.Windows.Forms.GroupBox();
            this.checkFeedTrip = new System.Windows.Forms.CheckBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.urlVehicle = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.urlAlert = new System.Windows.Forms.TextBox();
            this.urlTrip = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.secondi = new System.Windows.Forms.NumericUpDown();
            this.minuti = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGriglia = new System.Windows.Forms.TabPage();
            this.tabMonitoraggio = new System.Windows.Forms.TabPage();
            this.dataGridViolazioni = new System.Windows.Forms.DataGridView();
            this.lineaDataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lineaMonitorataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.labelTotaleMatricolaATAC = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.labelTotaleMatricolaTPL = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolTipFeedTrip = new System.Windows.Forms.ToolTip(this.components);
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVetture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.extendedVehicleInfoBindingSource)).BeginInit();
            this.tabGrafico.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBoxExport.SuspendLayout();
            this.groupBoxMonitoraggio.SuspendLayout();
            this.groupBoxServerRSM.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.secondi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minuti)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabGriglia.SuspendLayout();
            this.tabMonitoraggio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViolazioni)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lineaMonitorataBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackgroundImage = global::AtacFeed.Properties.Resources.play;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(1085, 557);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(50, 46);
            this.button1.TabIndex = 0;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // dataGridVetture
            // 
            this.dataGridVetture.AllowUserToAddRows = false;
            this.dataGridVetture.AllowUserToDeleteRows = false;
            this.dataGridVetture.AllowUserToOrderColumns = true;
            this.dataGridVetture.AutoGenerateColumns = false;
            this.dataGridVetture.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridVetture.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridVetture.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridVetture.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idVetturaDataGridViewTextBoxColumn,
            this.matricolaDataGridViewTextBoxColumn,
            this.routeIdDataGridViewTextBoxColumn,
            this.lineaDataGridViewTextBoxColumn,
            this.gestoreDataGridViewTextBoxColumn,
            this.currentStopSequenceDataGridViewTextBoxColumn,
            this.congestionLevelDataGridViewTextBoxColumn,
            this.occupancyStatusDataGridViewTextBoxColumn,
            this.tripIdDataGridViewTextBoxColumn,
            this.primaVoltaDataGridViewTextBoxColumn,
            this.ultimaVoltaDataGridViewTextBoxColumn,
            this.rimessaDataGridViewTextBoxColumn,
            this.euroDataGridViewTextBoxColumn,
            this.modelloDataGridViewTextBoxColumn,
            this.Latitude,
            this.Longitude});
            this.dataGridVetture.DataSource = this.extendedVehicleInfoBindingSource;
            this.dataGridVetture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridVetture.Location = new System.Drawing.Point(3, 3);
            this.dataGridVetture.Name = "dataGridVetture";
            this.dataGridVetture.ReadOnly = true;
            this.dataGridVetture.Size = new System.Drawing.Size(1022, 648);
            this.dataGridVetture.TabIndex = 8;
            // 
            // idVetturaDataGridViewTextBoxColumn
            // 
            this.idVetturaDataGridViewTextBoxColumn.DataPropertyName = "IdVettura";
            this.idVetturaDataGridViewTextBoxColumn.HeaderText = "IdVettura";
            this.idVetturaDataGridViewTextBoxColumn.Name = "idVetturaDataGridViewTextBoxColumn";
            this.idVetturaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // matricolaDataGridViewTextBoxColumn
            // 
            this.matricolaDataGridViewTextBoxColumn.DataPropertyName = "Matricola";
            this.matricolaDataGridViewTextBoxColumn.HeaderText = "Matricola";
            this.matricolaDataGridViewTextBoxColumn.Name = "matricolaDataGridViewTextBoxColumn";
            this.matricolaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // routeIdDataGridViewTextBoxColumn
            // 
            this.routeIdDataGridViewTextBoxColumn.DataPropertyName = "RouteId";
            this.routeIdDataGridViewTextBoxColumn.HeaderText = "RouteId";
            this.routeIdDataGridViewTextBoxColumn.Name = "routeIdDataGridViewTextBoxColumn";
            this.routeIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // lineaDataGridViewTextBoxColumn
            // 
            this.lineaDataGridViewTextBoxColumn.DataPropertyName = "Linea";
            this.lineaDataGridViewTextBoxColumn.HeaderText = "Linea";
            this.lineaDataGridViewTextBoxColumn.Name = "lineaDataGridViewTextBoxColumn";
            this.lineaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // gestoreDataGridViewTextBoxColumn
            // 
            this.gestoreDataGridViewTextBoxColumn.DataPropertyName = "Gestore";
            this.gestoreDataGridViewTextBoxColumn.HeaderText = "Gestore";
            this.gestoreDataGridViewTextBoxColumn.Name = "gestoreDataGridViewTextBoxColumn";
            this.gestoreDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // currentStopSequenceDataGridViewTextBoxColumn
            // 
            this.currentStopSequenceDataGridViewTextBoxColumn.DataPropertyName = "CurrentStopSequence";
            this.currentStopSequenceDataGridViewTextBoxColumn.HeaderText = "CurrentStopSequence";
            this.currentStopSequenceDataGridViewTextBoxColumn.Name = "currentStopSequenceDataGridViewTextBoxColumn";
            this.currentStopSequenceDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // congestionLevelDataGridViewTextBoxColumn
            // 
            this.congestionLevelDataGridViewTextBoxColumn.DataPropertyName = "CongestionLevel";
            this.congestionLevelDataGridViewTextBoxColumn.HeaderText = "CongestionLevel";
            this.congestionLevelDataGridViewTextBoxColumn.Name = "congestionLevelDataGridViewTextBoxColumn";
            this.congestionLevelDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // occupancyStatusDataGridViewTextBoxColumn
            // 
            this.occupancyStatusDataGridViewTextBoxColumn.DataPropertyName = "OccupancyStatus";
            this.occupancyStatusDataGridViewTextBoxColumn.HeaderText = "OccupancyStatus";
            this.occupancyStatusDataGridViewTextBoxColumn.Name = "occupancyStatusDataGridViewTextBoxColumn";
            this.occupancyStatusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // tripIdDataGridViewTextBoxColumn
            // 
            this.tripIdDataGridViewTextBoxColumn.DataPropertyName = "TripId";
            this.tripIdDataGridViewTextBoxColumn.HeaderText = "TripId";
            this.tripIdDataGridViewTextBoxColumn.Name = "tripIdDataGridViewTextBoxColumn";
            this.tripIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // primaVoltaDataGridViewTextBoxColumn
            // 
            this.primaVoltaDataGridViewTextBoxColumn.DataPropertyName = "PrimaVolta";
            this.primaVoltaDataGridViewTextBoxColumn.HeaderText = "PrimaVolta";
            this.primaVoltaDataGridViewTextBoxColumn.Name = "primaVoltaDataGridViewTextBoxColumn";
            this.primaVoltaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // ultimaVoltaDataGridViewTextBoxColumn
            // 
            this.ultimaVoltaDataGridViewTextBoxColumn.DataPropertyName = "UltimaVolta";
            this.ultimaVoltaDataGridViewTextBoxColumn.HeaderText = "UltimaVolta";
            this.ultimaVoltaDataGridViewTextBoxColumn.Name = "ultimaVoltaDataGridViewTextBoxColumn";
            this.ultimaVoltaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // rimessaDataGridViewTextBoxColumn
            // 
            this.rimessaDataGridViewTextBoxColumn.DataPropertyName = "Rimessa";
            this.rimessaDataGridViewTextBoxColumn.HeaderText = "Rimessa";
            this.rimessaDataGridViewTextBoxColumn.Name = "rimessaDataGridViewTextBoxColumn";
            this.rimessaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // euroDataGridViewTextBoxColumn
            // 
            this.euroDataGridViewTextBoxColumn.DataPropertyName = "Euro";
            this.euroDataGridViewTextBoxColumn.HeaderText = "Euro";
            this.euroDataGridViewTextBoxColumn.Name = "euroDataGridViewTextBoxColumn";
            this.euroDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // modelloDataGridViewTextBoxColumn
            // 
            this.modelloDataGridViewTextBoxColumn.DataPropertyName = "Modello";
            this.modelloDataGridViewTextBoxColumn.HeaderText = "Modello";
            this.modelloDataGridViewTextBoxColumn.Name = "modelloDataGridViewTextBoxColumn";
            this.modelloDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Latitude
            // 
            this.Latitude.DataPropertyName = "Latitude";
            this.Latitude.HeaderText = "Latitude";
            this.Latitude.Name = "Latitude";
            this.Latitude.ReadOnly = true;
            // 
            // Longitude
            // 
            this.Longitude.DataPropertyName = "Longitude";
            this.Longitude.HeaderText = "Longitude";
            this.Longitude.Name = "Longitude";
            this.Longitude.ReadOnly = true;
            // 
            // extendedVehicleInfoBindingSource
            // 
            this.extendedVehicleInfoBindingSource.DataSource = typeof(AtacFeed.ExtendedVehicleInfo);
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(1058, 479);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(102, 23);
            this.button3.TabIndex = 28;
            this.button3.Text = "Dati Random";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.Random);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(1048, 398);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "# Righe";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTotaleIdVettura
            // 
            this.labelTotaleIdVettura.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTotaleIdVettura.Location = new System.Drawing.Point(1120, 376);
            this.labelTotaleIdVettura.Name = "labelTotaleIdVettura";
            this.labelTotaleIdVettura.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTotaleIdVettura.Size = new System.Drawing.Size(40, 13);
            this.labelTotaleIdVettura.TabIndex = 11;
            this.labelTotaleIdVettura.Text = "0";
            this.labelTotaleIdVettura.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTotaleMatricola
            // 
            this.labelTotaleMatricola.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTotaleMatricola.Location = new System.Drawing.Point(1120, 333);
            this.labelTotaleMatricola.Name = "labelTotaleMatricola";
            this.labelTotaleMatricola.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTotaleMatricola.Size = new System.Drawing.Size(40, 13);
            this.labelTotaleMatricola.TabIndex = 25;
            this.labelTotaleMatricola.Text = "0";
            this.labelTotaleMatricola.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(1048, 376);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "# IdVettura";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTotaleRighe
            // 
            this.labelTotaleRighe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTotaleRighe.Location = new System.Drawing.Point(1120, 398);
            this.labelTotaleRighe.Name = "labelTotaleRighe";
            this.labelTotaleRighe.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTotaleRighe.Size = new System.Drawing.Size(40, 13);
            this.labelTotaleRighe.TabIndex = 23;
            this.labelTotaleRighe.Text = "0";
            this.labelTotaleRighe.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1048, 333);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "# Matricola";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelFeedLetti
            // 
            this.labelFeedLetti.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelFeedLetti.Location = new System.Drawing.Point(1120, 253);
            this.labelFeedLetti.MinimumSize = new System.Drawing.Size(15, 0);
            this.labelFeedLetti.Name = "labelFeedLetti";
            this.labelFeedLetti.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelFeedLetti.Size = new System.Drawing.Size(40, 13);
            this.labelFeedLetti.TabIndex = 21;
            this.labelFeedLetti.Text = "0";
            this.labelFeedLetti.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1048, 253);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "# Feed";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(1057, 35);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(100, 13);
            this.label12.TabIndex = 29;
            this.label12.Text = "Ultimo feed letto";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(1052, 79);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(35, 13);
            this.label13.TabIndex = 39;
            this.label13.Text = "ATAC";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTot
            // 
            this.labelTot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTot.Location = new System.Drawing.Point(1120, 148);
            this.labelTot.Name = "labelTot";
            this.labelTot.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTot.Size = new System.Drawing.Size(40, 13);
            this.labelTot.TabIndex = 38;
            this.labelTot.Text = "0";
            this.labelTot.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(1052, 102);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(27, 13);
            this.label15.TabIndex = 37;
            this.label15.Text = "TPL";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTPL
            // 
            this.labelTPL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTPL.Location = new System.Drawing.Point(1120, 102);
            this.labelTPL.Name = "labelTPL";
            this.labelTPL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTPL.Size = new System.Drawing.Size(40, 13);
            this.labelTPL.TabIndex = 36;
            this.labelTPL.Text = "0";
            this.labelTPL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOraLettura
            // 
            this.lblOraLettura.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOraLettura.Location = new System.Drawing.Point(1082, 56);
            this.lblOraLettura.MinimumSize = new System.Drawing.Size(15, 0);
            this.lblOraLettura.Name = "lblOraLettura";
            this.lblOraLettura.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblOraLettura.Size = new System.Drawing.Size(78, 13);
            this.lblOraLettura.TabIndex = 35;
            this.lblOraLettura.Text = "--:--:--";
            this.lblOraLettura.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(1052, 148);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(37, 13);
            this.label19.TabIndex = 33;
            this.label19.Text = "Totale";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelAtac
            // 
            this.labelAtac.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAtac.Location = new System.Drawing.Point(1120, 79);
            this.labelAtac.Name = "labelAtac";
            this.labelAtac.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelAtac.Size = new System.Drawing.Size(40, 13);
            this.labelAtac.TabIndex = 32;
            this.labelAtac.Text = "0";
            this.labelAtac.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(1052, 56);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 40;
            this.label11.Text = "Orario";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelWait
            // 
            this.labelWait.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWait.Location = new System.Drawing.Point(1120, 125);
            this.labelWait.Name = "labelWait";
            this.labelWait.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelWait.Size = new System.Drawing.Size(40, 13);
            this.labelWait.TabIndex = 42;
            this.labelWait.Text = "0";
            this.labelWait.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(1052, 125);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(49, 13);
            this.label16.TabIndex = 41;
            this.label16.Text = "In Attesa";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabGrafico
            // 
            this.tabGrafico.Controls.Add(this.tableLayoutPanel2);
            this.tabGrafico.Location = new System.Drawing.Point(4, 22);
            this.tabGrafico.Margin = new System.Windows.Forms.Padding(0);
            this.tabGrafico.Name = "tabGrafico";
            this.tabGrafico.Size = new System.Drawing.Size(1028, 654);
            this.tabGrafico.TabIndex = 2;
            this.tabGrafico.Text = "Grafico";
            this.tabGrafico.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.formsPlotTPL, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.formsPlotAtac, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1022, 648);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // formsPlotTPL
            // 
            this.formsPlotTPL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlotTPL.Location = new System.Drawing.Point(0, 324);
            this.formsPlotTPL.Margin = new System.Windows.Forms.Padding(0);
            this.formsPlotTPL.Name = "formsPlotTPL";
            this.formsPlotTPL.Size = new System.Drawing.Size(1022, 324);
            this.formsPlotTPL.TabIndex = 1;
            // 
            // formsPlotAtac
            // 
            this.formsPlotAtac.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlotAtac.Location = new System.Drawing.Point(0, 0);
            this.formsPlotAtac.Margin = new System.Windows.Forms.Padding(0);
            this.formsPlotAtac.Name = "formsPlotAtac";
            this.formsPlotAtac.Size = new System.Drawing.Size(1022, 324);
            this.formsPlotAtac.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage1.Controls.Add(this.groupBoxExport);
            this.tabPage1.Controls.Add(this.groupBoxMonitoraggio);
            this.tabPage1.Controls.Add(this.groupBoxServerRSM);
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1028, 654);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Impostazioni";
            // 
            // groupBoxExport
            // 
            this.groupBoxExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExport.Controls.Add(this.checkAlert);
            this.groupBoxExport.Controls.Add(this.checkMonitoraggio);
            this.groupBoxExport.Controls.Add(this.checkXlsx);
            this.groupBoxExport.Controls.Add(this.checkCSV);
            this.groupBoxExport.Controls.Add(this.checkGrafico);
            this.groupBoxExport.Location = new System.Drawing.Point(6, 219);
            this.groupBoxExport.Name = "groupBoxExport";
            this.groupBoxExport.Size = new System.Drawing.Size(886, 119);
            this.groupBoxExport.TabIndex = 28;
            this.groupBoxExport.TabStop = false;
            this.groupBoxExport.Text = "Impostazioni Export";
            // 
            // checkAlert
            // 
            this.checkAlert.AutoSize = true;
            this.checkAlert.Checked = true;
            this.checkAlert.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAlert.Location = new System.Drawing.Point(133, 97);
            this.checkAlert.Name = "checkAlert";
            this.checkAlert.Size = new System.Drawing.Size(93, 17);
            this.checkAlert.TabIndex = 27;
            this.checkAlert.Text = "Includi gli alert";
            this.checkAlert.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkAlert.UseVisualStyleBackColor = true;
            // 
            // checkMonitoraggio
            // 
            this.checkMonitoraggio.AutoSize = true;
            this.checkMonitoraggio.Location = new System.Drawing.Point(133, 68);
            this.checkMonitoraggio.Name = "checkMonitoraggio";
            this.checkMonitoraggio.Size = new System.Drawing.Size(152, 17);
            this.checkMonitoraggio.TabIndex = 26;
            this.checkMonitoraggio.Text = "Includi il monitoraggio linee";
            this.checkMonitoraggio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkMonitoraggio.UseVisualStyleBackColor = true;
            // 
            // checkXlsx
            // 
            this.checkXlsx.AutoSize = true;
            this.checkXlsx.Checked = true;
            this.checkXlsx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkXlsx.Location = new System.Drawing.Point(118, 19);
            this.checkXlsx.Name = "checkXlsx";
            this.checkXlsx.Size = new System.Drawing.Size(114, 17);
            this.checkXlsx.TabIndex = 22;
            this.checkXlsx.Text = "Esporta come .xlsx";
            this.checkXlsx.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkXlsx.UseVisualStyleBackColor = true;
            this.checkXlsx.CheckedChanged += new System.EventHandler(this.CheckXlsx_CheckedChanged);
            // 
            // checkCSV
            // 
            this.checkCSV.AutoSize = true;
            this.checkCSV.Checked = true;
            this.checkCSV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkCSV.Location = new System.Drawing.Point(560, 19);
            this.checkCSV.Name = "checkCSV";
            this.checkCSV.Size = new System.Drawing.Size(114, 17);
            this.checkCSV.TabIndex = 23;
            this.checkCSV.Text = "Esporta come .csv";
            this.checkCSV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkCSV.UseVisualStyleBackColor = true;
            // 
            // checkGrafico
            // 
            this.checkGrafico.AutoSize = true;
            this.checkGrafico.Checked = true;
            this.checkGrafico.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkGrafico.Location = new System.Drawing.Point(133, 42);
            this.checkGrafico.Name = "checkGrafico";
            this.checkGrafico.Size = new System.Drawing.Size(99, 17);
            this.checkGrafico.TabIndex = 25;
            this.checkGrafico.Text = "Includi il grafico";
            this.checkGrafico.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkGrafico.UseVisualStyleBackColor = true;
            // 
            // groupBoxMonitoraggio
            // 
            this.groupBoxMonitoraggio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMonitoraggio.Controls.Add(this.buttonResetRegole);
            this.groupBoxMonitoraggio.Controls.Add(this.checkBoxStorico);
            this.groupBoxMonitoraggio.Controls.Add(this.labelRaggruppaAlert);
            this.groupBoxMonitoraggio.Controls.Add(this.radioNonRaggruppare);
            this.groupBoxMonitoraggio.Controls.Add(this.checkReset);
            this.groupBoxMonitoraggio.Controls.Add(this.radioLineaRegola);
            this.groupBoxMonitoraggio.Controls.Add(this.radioLinea);
            this.groupBoxMonitoraggio.Controls.Add(this.dateTimeReset);
            this.groupBoxMonitoraggio.Controls.Add(this.checkTripVuoti);
            this.groupBoxMonitoraggio.Controls.Add(this.comboBox1);
            this.groupBoxMonitoraggio.Controls.Add(this.label4);
            this.groupBoxMonitoraggio.Controls.Add(this.checkTripDuplicati);
            this.groupBoxMonitoraggio.Location = new System.Drawing.Point(6, 120);
            this.groupBoxMonitoraggio.Name = "groupBoxMonitoraggio";
            this.groupBoxMonitoraggio.Size = new System.Drawing.Size(1014, 93);
            this.groupBoxMonitoraggio.TabIndex = 27;
            this.groupBoxMonitoraggio.TabStop = false;
            this.groupBoxMonitoraggio.Text = "Impostazioni Monitoraggio";
            // 
            // buttonResetRegole
            // 
            this.buttonResetRegole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResetRegole.Location = new System.Drawing.Point(911, 30);
            this.buttonResetRegole.Name = "buttonResetRegole";
            this.buttonResetRegole.Size = new System.Drawing.Size(83, 23);
            this.buttonResetRegole.TabIndex = 48;
            this.buttonResetRegole.Text = "Rleggi regole";
            this.buttonResetRegole.UseVisualStyleBackColor = true;
            this.buttonResetRegole.Click += new System.EventHandler(this.RileggiRegole);
            // 
            // checkBoxStorico
            // 
            this.checkBoxStorico.AutoSize = true;
            this.checkBoxStorico.Location = new System.Drawing.Point(661, 68);
            this.checkBoxStorico.Name = "checkBoxStorico";
            this.checkBoxStorico.Size = new System.Drawing.Size(138, 17);
            this.checkBoxStorico.TabIndex = 32;
            this.checkBoxStorico.Text = "Mostra storico violazioni";
            this.checkBoxStorico.UseVisualStyleBackColor = true;
            // 
            // labelRaggruppaAlert
            // 
            this.labelRaggruppaAlert.AutoSize = true;
            this.labelRaggruppaAlert.Location = new System.Drawing.Point(658, 30);
            this.labelRaggruppaAlert.Name = "labelRaggruppaAlert";
            this.labelRaggruppaAlert.Size = new System.Drawing.Size(114, 13);
            this.labelRaggruppaAlert.TabIndex = 31;
            this.labelRaggruppaAlert.Text = "Raggruppa gli alert per";
            // 
            // radioNonRaggruppare
            // 
            this.radioNonRaggruppare.AutoSize = true;
            this.radioNonRaggruppare.Location = new System.Drawing.Point(776, 49);
            this.radioNonRaggruppare.Margin = new System.Windows.Forms.Padding(1);
            this.radioNonRaggruppare.Name = "radioNonRaggruppare";
            this.radioNonRaggruppare.Size = new System.Drawing.Size(105, 17);
            this.radioNonRaggruppare.TabIndex = 30;
            this.radioNonRaggruppare.Text = "Non raggruppare";
            this.radioNonRaggruppare.UseVisualStyleBackColor = true;
            // 
            // checkReset
            // 
            this.checkReset.AutoSize = true;
            this.checkReset.Location = new System.Drawing.Point(295, 67);
            this.checkReset.Name = "checkReset";
            this.checkReset.Size = new System.Drawing.Size(193, 17);
            this.checkReset.TabIndex = 24;
            this.checkReset.Text = "Azzera e riavvia il monitoraggio alle ";
            this.checkReset.UseVisualStyleBackColor = true;
            this.checkReset.CheckedChanged += new System.EventHandler(this.CheckReset_CheckedChanged);
            // 
            // radioLineaRegola
            // 
            this.radioLineaRegola.AutoSize = true;
            this.radioLineaRegola.Checked = true;
            this.radioLineaRegola.Location = new System.Drawing.Point(776, 30);
            this.radioLineaRegola.Margin = new System.Windows.Forms.Padding(1);
            this.radioLineaRegola.Name = "radioLineaRegola";
            this.radioLineaRegola.Size = new System.Drawing.Size(97, 17);
            this.radioLineaRegola.TabIndex = 29;
            this.radioLineaRegola.TabStop = true;
            this.radioLineaRegola.Text = "Linea e Regola";
            this.radioLineaRegola.UseVisualStyleBackColor = true;
            // 
            // radioLinea
            // 
            this.radioLinea.AutoSize = true;
            this.radioLinea.Location = new System.Drawing.Point(776, 11);
            this.radioLinea.Margin = new System.Windows.Forms.Padding(1);
            this.radioLinea.Name = "radioLinea";
            this.radioLinea.Size = new System.Drawing.Size(51, 17);
            this.radioLinea.TabIndex = 28;
            this.radioLinea.Text = "Linea";
            this.radioLinea.UseVisualStyleBackColor = true;
            // 
            // dateTimeReset
            // 
            this.dateTimeReset.CustomFormat = "HH:mm";
            this.dateTimeReset.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimeReset.Location = new System.Drawing.Point(488, 67);
            this.dateTimeReset.Name = "dateTimeReset";
            this.dateTimeReset.ShowUpDown = true;
            this.dateTimeReset.Size = new System.Drawing.Size(50, 20);
            this.dateTimeReset.TabIndex = 22;
            this.dateTimeReset.Value = new System.DateTime(2020, 9, 6, 0, 0, 0, 0);
            // 
            // checkTripVuoti
            // 
            this.checkTripVuoti.AutoSize = true;
            this.checkTripVuoti.Location = new System.Drawing.Point(295, 21);
            this.checkTripVuoti.Name = "checkTripVuoti";
            this.checkTripVuoti.Size = new System.Drawing.Size(188, 17);
            this.checkTripVuoti.TabIndex = 21;
            this.checkTripVuoti.Text = "Escludi le vetture con TripId vuoto";
            this.checkTripVuoti.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkTripVuoti.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(124, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(165, 21);
            this.comboBox1.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(77, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Linea";
            // 
            // checkTripDuplicati
            // 
            this.checkTripDuplicati.AutoSize = true;
            this.checkTripDuplicati.Location = new System.Drawing.Point(295, 44);
            this.checkTripDuplicati.Name = "checkTripDuplicati";
            this.checkTripDuplicati.Size = new System.Drawing.Size(317, 17);
            this.checkTripDuplicati.TabIndex = 14;
            this.checkTripDuplicati.Text = "Includi più volte la stessa vettura se è rilevata su TripId diversi";
            this.checkTripDuplicati.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkTripDuplicati.UseVisualStyleBackColor = true;
            // 
            // groupBoxServerRSM
            // 
            this.groupBoxServerRSM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxServerRSM.Controls.Add(this.checkFeedTrip);
            this.groupBoxServerRSM.Controls.Add(this.label20);
            this.groupBoxServerRSM.Controls.Add(this.label6);
            this.groupBoxServerRSM.Controls.Add(this.label7);
            this.groupBoxServerRSM.Controls.Add(this.urlVehicle);
            this.groupBoxServerRSM.Controls.Add(this.label2);
            this.groupBoxServerRSM.Controls.Add(this.urlAlert);
            this.groupBoxServerRSM.Controls.Add(this.urlTrip);
            this.groupBoxServerRSM.Controls.Add(this.label5);
            this.groupBoxServerRSM.Controls.Add(this.secondi);
            this.groupBoxServerRSM.Controls.Add(this.minuti);
            this.groupBoxServerRSM.Location = new System.Drawing.Point(6, 6);
            this.groupBoxServerRSM.Name = "groupBoxServerRSM";
            this.groupBoxServerRSM.Size = new System.Drawing.Size(1014, 108);
            this.groupBoxServerRSM.TabIndex = 26;
            this.groupBoxServerRSM.TabStop = false;
            this.groupBoxServerRSM.Text = "Impostazioni Server RSM";
            // 
            // checkFeedTrip
            // 
            this.checkFeedTrip.AutoSize = true;
            this.checkFeedTrip.Location = new System.Drawing.Point(47, 63);
            this.checkFeedTrip.Name = "checkFeedTrip";
            this.checkFeedTrip.Size = new System.Drawing.Size(71, 17);
            this.checkFeedTrip.TabIndex = 20;
            this.checkFeedTrip.Text = "Feed Trip";
            this.toolTipFeedTrip.SetToolTip(this.checkFeedTrip, "Può essere tranquillamnte disabilitato. Al momento l\'informazione del feed non è " +
        "utilizzata per il conteggio delle vetture");
            this.checkFeedTrip.UseVisualStyleBackColor = true;
            this.checkFeedTrip.CheckedChanged += new System.EventHandler(this.CheckFeedTrip_CheckedChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(63, 85);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(55, 13);
            this.label20.TabIndex = 6;
            this.label20.Text = "Feed Alert";
            this.label20.Visible = false;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(908, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "secondi";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(824, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "minuti";
            // 
            // urlVehicle
            // 
            this.urlVehicle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlVehicle.Location = new System.Drawing.Point(124, 27);
            this.urlVehicle.Name = "urlVehicle";
            this.urlVehicle.Size = new System.Drawing.Size(569, 20);
            this.urlVehicle.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Feed Vehicle";
            // 
            // urlAlert
            // 
            this.urlAlert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlAlert.Location = new System.Drawing.Point(124, 82);
            this.urlAlert.Name = "urlAlert";
            this.urlAlert.Size = new System.Drawing.Size(569, 20);
            this.urlAlert.TabIndex = 3;
            this.urlAlert.Visible = false;
            // 
            // urlTrip
            // 
            this.urlTrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlTrip.Location = new System.Drawing.Point(124, 60);
            this.urlTrip.Name = "urlTrip";
            this.urlTrip.Size = new System.Drawing.Size(569, 20);
            this.urlTrip.TabIndex = 3;
            this.toolTipFeedTrip.SetToolTip(this.urlTrip, "Url opzionale, al momento usato solo per fini statistici");
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(717, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Ripeti ogni";
            // 
            // secondi
            // 
            this.secondi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.secondi.Location = new System.Drawing.Point(864, 28);
            this.secondi.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.secondi.Name = "secondi";
            this.secondi.Size = new System.Drawing.Size(38, 20);
            this.secondi.TabIndex = 16;
            this.secondi.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // minuti
            // 
            this.minuti.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minuti.Location = new System.Drawing.Point(780, 27);
            this.minuti.Name = "minuti";
            this.minuti.Size = new System.Drawing.Size(38, 20);
            this.minuti.TabIndex = 15;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.textBox3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBox4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 341);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1020, 312);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // textBox3
            // 
            this.textBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox3.Location = new System.Drawing.Point(343, 3);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(334, 306);
            this.textBox3.TabIndex = 9;
            // 
            // textBox4
            // 
            this.textBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox4.Location = new System.Drawing.Point(683, 3);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox4.Size = new System.Drawing.Size(334, 306);
            this.textBox4.TabIndex = 10;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.textBox2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.textBox1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(340, 312);
            this.tableLayoutPanel3.TabIndex = 11;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(3, 159);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(334, 150);
            this.textBox2.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(334, 150);
            this.textBox1.TabIndex = 4;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(898, 257);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(122, 23);
            this.button2.TabIndex = 20;
            this.button2.Text = "Salva Impostazioni";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.SalvaImpostazioni);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabGrafico);
            this.tabControl1.Controls.Add(this.tabGriglia);
            this.tabControl1.Controls.Add(this.tabMonitoraggio);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1036, 680);
            this.tabControl1.TabIndex = 27;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1_SelectedIndexChanged);
            // 
            // tabGriglia
            // 
            this.tabGriglia.Controls.Add(this.dataGridVetture);
            this.tabGriglia.Location = new System.Drawing.Point(4, 22);
            this.tabGriglia.Name = "tabGriglia";
            this.tabGriglia.Padding = new System.Windows.Forms.Padding(3);
            this.tabGriglia.Size = new System.Drawing.Size(1028, 654);
            this.tabGriglia.TabIndex = 3;
            this.tabGriglia.Text = "Griglia";
            this.tabGriglia.UseVisualStyleBackColor = true;
            // 
            // tabMonitoraggio
            // 
            this.tabMonitoraggio.Controls.Add(this.dataGridViolazioni);
            this.tabMonitoraggio.Location = new System.Drawing.Point(4, 22);
            this.tabMonitoraggio.Name = "tabMonitoraggio";
            this.tabMonitoraggio.Padding = new System.Windows.Forms.Padding(3);
            this.tabMonitoraggio.Size = new System.Drawing.Size(1028, 654);
            this.tabMonitoraggio.TabIndex = 4;
            this.tabMonitoraggio.Text = "Monitoraggio Linee";
            this.tabMonitoraggio.UseVisualStyleBackColor = true;
            // 
            // dataGridViolazioni
            // 
            this.dataGridViolazioni.AllowUserToAddRows = false;
            this.dataGridViolazioni.AllowUserToDeleteRows = false;
            this.dataGridViolazioni.AllowUserToOrderColumns = true;
            this.dataGridViolazioni.AutoGenerateColumns = false;
            this.dataGridViolazioni.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViolazioni.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViolazioni.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.lineaDataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8,
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10});
            this.dataGridViolazioni.DataSource = this.lineaMonitorataBindingSource;
            this.dataGridViolazioni.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViolazioni.Location = new System.Drawing.Point(3, 3);
            this.dataGridViolazioni.Name = "dataGridViolazioni";
            this.dataGridViolazioni.ReadOnly = true;
            this.dataGridViolazioni.Size = new System.Drawing.Size(1022, 648);
            this.dataGridViolazioni.TabIndex = 0;
            // 
            // lineaDataGridViewTextBoxColumn2
            // 
            this.lineaDataGridViewTextBoxColumn2.DataPropertyName = "Linea";
            this.lineaDataGridViewTextBoxColumn2.HeaderText = "Linea";
            this.lineaDataGridViewTextBoxColumn2.Name = "lineaDataGridViewTextBoxColumn2";
            this.lineaDataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Giorno";
            this.dataGridViewTextBoxColumn3.HeaderText = "Giorno";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "Da";
            this.dataGridViewTextBoxColumn4.HeaderText = "Da";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "A";
            this.dataGridViewTextBoxColumn5.HeaderText = "A";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "TempoBonus";
            this.dataGridViewTextBoxColumn6.HeaderText = "TempoBonus";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "VetturePreviste";
            this.dataGridViewTextBoxColumn7.HeaderText = "VetturePreviste";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.DataPropertyName = "VettureRilevate";
            this.dataGridViewTextBoxColumn8.HeaderText = "VettureRilevate";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "OraPrimaViolazione";
            dataGridViewCellStyle1.Format = "HH:mm:ss";
            dataGridViewCellStyle1.NullValue = null;
            this.dataGridViewTextBoxColumn9.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn9.HeaderText = "OraPrimaViolazione";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.DataPropertyName = "OraUltimaViolazione";
            dataGridViewCellStyle2.Format = "HH:mm:ss";
            this.dataGridViewTextBoxColumn10.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewTextBoxColumn10.HeaderText = "OraUltimaViolazione";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.ReadOnly = true;
            // 
            // lineaMonitorataBindingSource
            // 
            this.lineaMonitorataBindingSource.DataSource = typeof(AtacFeed.LineaMonitorata);
            // 
            // labelTotaleMatricolaATAC
            // 
            this.labelTotaleMatricolaATAC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTotaleMatricolaATAC.Location = new System.Drawing.Point(1120, 277);
            this.labelTotaleMatricolaATAC.Name = "labelTotaleMatricolaATAC";
            this.labelTotaleMatricolaATAC.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTotaleMatricolaATAC.Size = new System.Drawing.Size(40, 13);
            this.labelTotaleMatricolaATAC.TabIndex = 44;
            this.labelTotaleMatricolaATAC.Text = "0";
            this.labelTotaleMatricolaATAC.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(1048, 277);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(45, 13);
            this.label17.TabIndex = 43;
            this.label17.Text = "# ATAC";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTotaleMatricolaTPL
            // 
            this.labelTotaleMatricolaTPL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTotaleMatricolaTPL.Location = new System.Drawing.Point(1120, 305);
            this.labelTotaleMatricolaTPL.Name = "labelTotaleMatricolaTPL";
            this.labelTotaleMatricolaTPL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelTotaleMatricolaTPL.Size = new System.Drawing.Size(40, 13);
            this.labelTotaleMatricolaTPL.TabIndex = 46;
            this.labelTotaleMatricolaTPL.Text = "0";
            this.labelTotaleMatricolaTPL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label18
            // 
            this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(1048, 305);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(37, 13);
            this.label18.TabIndex = 45;
            this.label18.Text = "# TPL";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(1057, 228);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(88, 13);
            this.label14.TabIndex = 47;
            this.label14.Text = "Dati Aggregati";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "RegolaViolata";
            this.dataGridViewTextBoxColumn1.HeaderText = "RegolaViolata";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "RegolaViolata";
            this.dataGridViewTextBoxColumn2.HeaderText = "RegolaViolata";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // toolTipFeedTrip
            // 
            this.toolTipFeedTrip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipFeedTrip.ToolTipTitle = "GTFS TRIP";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1060, 663);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 23);
            this.button4.TabIndex = 48;
            this.button4.Text = "buttonReset";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.ResetAcquisizione);
            // 
            // FormGTFS_RSM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1169, 705);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.labelTotaleMatricolaTPL);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.labelTotaleMatricolaATAC);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.labelWait);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.labelTot);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.labelTPL);
            this.Controls.Add(this.lblOraLettura);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.labelAtac);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.labelTotaleMatricola);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.labelTotaleRighe);
            this.Controls.Add(this.labelFeedLetti);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelTotaleIdVettura);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormGTFS_RSM";
            this.Text = "Monitoraggio Trasporti  Roma";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVetture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.extendedVehicleInfoBindingSource)).EndInit();
            this.tabGrafico.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBoxExport.ResumeLayout(false);
            this.groupBoxExport.PerformLayout();
            this.groupBoxMonitoraggio.ResumeLayout(false);
            this.groupBoxMonitoraggio.PerformLayout();
            this.groupBoxServerRSM.ResumeLayout(false);
            this.groupBoxServerRSM.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.secondi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minuti)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabGriglia.ResumeLayout(false);
            this.tabMonitoraggio.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViolazioni)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lineaMonitorataBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridVetture;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.BindingSource extendedVehicleInfoBindingSource;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label labelTotaleIdVettura;
        private System.Windows.Forms.Label labelTotaleMatricola;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelTotaleRighe;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelFeedLetti;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label labelTot;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label labelTPL;
        private System.Windows.Forms.Label lblOraLettura;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label labelAtac;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label labelWait;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TabPage tabGrafico;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox urlVehicle;
        private System.Windows.Forms.TextBox urlTrip;
        private System.Windows.Forms.CheckBox checkCSV;
        private System.Windows.Forms.CheckBox checkXlsx;
        private System.Windows.Forms.NumericUpDown minuti;
        private System.Windows.Forms.CheckBox checkTripVuoti;
        private System.Windows.Forms.CheckBox checkTripDuplicati;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown secondi;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.CheckBox checkGrafico;
        private System.Windows.Forms.TabPage tabGriglia;
        private System.Windows.Forms.Label labelTotaleMatricolaATAC;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label labelTotaleMatricolaTPL;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ScottPlot.FormsPlot formsPlotTPL;
        private ScottPlot.FormsPlot formsPlotAtac;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TabPage tabMonitoraggio;
        private System.Windows.Forms.DataGridView dataGridViolazioni;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.BindingSource lineaMonitorataBindingSource;
        private System.Windows.Forms.Button buttonResetRegole;
        private System.Windows.Forms.GroupBox groupBoxServerRSM;
        private System.Windows.Forms.GroupBox groupBoxMonitoraggio;
        private System.Windows.Forms.GroupBox groupBoxExport;
        private System.Windows.Forms.DateTimePicker dateTimeReset;
        private System.Windows.Forms.CheckBox checkReset;
        private System.Windows.Forms.CheckBox checkMonitoraggio;
        private System.Windows.Forms.RadioButton radioLineaRegola;
        private System.Windows.Forms.RadioButton radioLinea;
        private System.Windows.Forms.CheckBox checkAlert;
        private System.Windows.Forms.RadioButton radioNonRaggruppare;
        private System.Windows.Forms.Label labelRaggruppaAlert;
        private System.Windows.Forms.DataGridViewTextBoxColumn idVetturaDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn matricolaDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn routeIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lineaDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gestoreDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn currentStopSequenceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn congestionLevelDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn occupancyStatusDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tripIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn primaVoltaDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ultimaVoltaDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rimessaDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn euroDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn modelloDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Latitude;
        private System.Windows.Forms.DataGridViewTextBoxColumn Longitude;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox urlAlert;
        private System.Windows.Forms.CheckBox checkBoxStorico;
        private System.Windows.Forms.DataGridViewTextBoxColumn lineaDataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.ToolTip toolTipFeedTrip;
        private System.Windows.Forms.CheckBox checkFeedTrip;
        private System.Windows.Forms.Button button4;
    }
}

