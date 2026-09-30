using Guna.UI2.WinForms;

namespace Barcoded_Warehouse_Stock_Tracking
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabProducts = new System.Windows.Forms.TabPage();
            this.dgvProducts = new System.Windows.Forms.DataGridView();
            this.btnAdd = new Guna.UI2.WinForms.Guna2Button();
            this.btnPos = new Guna.UI2.WinForms.Guna2Button();
            this.btnReturns = new Guna.UI2.WinForms.Guna2Button();
            this.btnCustomers = new Guna.UI2.WinForms.Guna2Button();
            this.btnReports = new Guna.UI2.WinForms.Guna2Button();
            this.txtPrice = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtName = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtBarcode = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtInitialStock = new Guna.UI2.WinForms.Guna2TextBox();
            this.lblPrice = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblBarcode = new System.Windows.Forms.Label();
            this.lblInitialStock = new System.Windows.Forms.Label();
            this.tabMovements = new System.Windows.Forms.TabPage();
            this.dgvMovements = new System.Windows.Forms.DataGridView();
            this.btnAddMovement = new Guna.UI2.WinForms.Guna2Button();
            this.cmbType = new Guna.UI2.WinForms.Guna2ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.nudQuantity = new System.Windows.Forms.NumericUpDown();
            this.lblQuantity = new System.Windows.Forms.Label();
            this.txtBarcodeMovement = new Guna.UI2.WinForms.Guna2TextBox();
            this.lblBarcodeMovement = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabProducts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.tabMovements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMovements)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantity)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabProducts);
            this.tabControl.Controls.Add(this.tabMovements);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1100, 640);
            this.tabControl.TabIndex = 0;
            // 
            // tabProducts
            // 
            this.tabProducts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.tabProducts.Controls.Add(this.dgvProducts);
            this.tabProducts.Controls.Add(this.btnAdd);
            this.tabProducts.Controls.Add(this.btnPos);
            this.tabProducts.Controls.Add(this.btnReturns);
            this.tabProducts.Controls.Add(this.btnCustomers);
            this.tabProducts.Controls.Add(this.btnReports);
            this.tabProducts.Controls.Add(this.txtInitialStock);
            this.tabProducts.Controls.Add(this.txtPrice);
            this.tabProducts.Controls.Add(this.txtName);
            this.tabProducts.Controls.Add(this.txtBarcode);
            this.tabProducts.Controls.Add(this.lblInitialStock);
            this.tabProducts.Controls.Add(this.lblPrice);
            this.tabProducts.Controls.Add(this.lblName);
            this.tabProducts.Controls.Add(this.lblBarcode);
            this.tabProducts.Location = new System.Drawing.Point(4, 29);
            this.tabProducts.Name = "tabProducts";
            this.tabProducts.Padding = new System.Windows.Forms.Padding(14);
            this.tabProducts.Size = new System.Drawing.Size(1092, 607);
            this.tabProducts.TabIndex = 0;
            this.tabProducts.Text = "📦  Ürünler";
            // 
            // dgvProducts
            // 
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            this.dgvProducts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProducts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvProducts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvProducts.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvProducts.EnableHeadersVisualStyles = false;
            this.dgvProducts.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(55)))), ((int)(((byte)(90)))));
            this.dgvProducts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvProducts.Location = new System.Drawing.Point(18, 262);
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.ReadOnly = true;
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.Size = new System.Drawing.Size(1056, 328);
            this.dgvProducts.TabIndex = 7;
            // 
            // btnAdd
            // 
            this.btnAdd.BorderRadius = 8;
            this.btnAdd.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(174)))), ((int)(((byte)(93)))));
            this.btnAdd.Location = new System.Drawing.Point(390, 18);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(200, 38);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "＋  Ürünü Kaydet";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnPos
            // 
            this.btnPos.BorderRadius = 8;
            this.btnPos.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            this.btnPos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPos.ForeColor = System.Drawing.Color.White;
            this.btnPos.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(50)))), ((int)(((byte)(75)))));
            this.btnPos.Location = new System.Drawing.Point(390, 62);
            this.btnPos.Name = "btnPos";
            this.btnPos.Size = new System.Drawing.Size(200, 38);
            this.btnPos.TabIndex = 8;
            this.btnPos.Text = "🛒  POS / Kasa";
            // 
            // btnReturns
            // 
            this.btnReturns.BorderRadius = 8;
            this.btnReturns.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.btnReturns.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnReturns.ForeColor = System.Drawing.Color.White;
            this.btnReturns.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(100)))), ((int)(((byte)(20)))));
            this.btnReturns.Location = new System.Drawing.Point(390, 106);
            this.btnReturns.Name = "btnReturns";
            this.btnReturns.Size = new System.Drawing.Size(200, 38);
            this.btnReturns.TabIndex = 9;
            this.btnReturns.Text = "↩  İade / İptal";
            // 
            // btnCustomers
            // 
            this.btnCustomers.BorderRadius = 8;
            this.btnCustomers.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnCustomers.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCustomers.ForeColor = System.Drawing.Color.White;
            this.btnCustomers.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(130)))), ((int)(((byte)(200)))));
            this.btnCustomers.Location = new System.Drawing.Point(610, 18);
            this.btnCustomers.Name = "btnCustomers";
            this.btnCustomers.Size = new System.Drawing.Size(200, 38);
            this.btnCustomers.TabIndex = 10;
            this.btnCustomers.Text = "👤  Müşteriler / Cari";
            // 
            // btnReports
            // 
            this.btnReports.BorderRadius = 8;
            this.btnReports.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(160)))));
            this.btnReports.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnReports.ForeColor = System.Drawing.Color.White;
            this.btnReports.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(140)))));
            this.btnReports.Location = new System.Drawing.Point(610, 62);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(200, 38);
            this.btnReports.TabIndex = 11;
            this.btnReports.Text = "📊  Raporlar / Yedek";
            // 
            // txtPrice
            // 
            this.txtPrice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            this.txtPrice.BorderRadius = 8;
            this.txtPrice.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPrice.DefaultText = "";
            this.txtPrice.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.txtPrice.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtPrice.Location = new System.Drawing.Point(110, 120);
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.PasswordChar = '\0';
            this.txtPrice.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.txtPrice.PlaceholderText = "Birim fiyat (TL)";
            this.txtPrice.SelectedText = "";
            this.txtPrice.Size = new System.Drawing.Size(260, 43);
            this.txtPrice.TabIndex = 5;
            // 
            // txtName
            // 
            this.txtName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            this.txtName.BorderRadius = 8;
            this.txtName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtName.DefaultText = "";
            this.txtName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtName.Location = new System.Drawing.Point(110, 70);
            this.txtName.Name = "txtName";
            this.txtName.PasswordChar = '\0';
            this.txtName.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.txtName.PlaceholderText = "Ürün adı";
            this.txtName.SelectedText = "";
            this.txtName.Size = new System.Drawing.Size(260, 43);
            this.txtName.TabIndex = 3;
            // 
            // txtBarcode
            // 
            this.txtBarcode.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            this.txtBarcode.BorderRadius = 8;
            this.txtBarcode.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtBarcode.DefaultText = "";
            this.txtBarcode.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.txtBarcode.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBarcode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtBarcode.Location = new System.Drawing.Point(110, 20);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.PasswordChar = '\0';
            this.txtBarcode.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.txtBarcode.PlaceholderText = "Barkod okutun veya yazın...";
            this.txtBarcode.SelectedText = "";
            this.txtBarcode.Size = new System.Drawing.Size(260, 43);
            this.txtBarcode.TabIndex = 1;
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblPrice.Location = new System.Drawing.Point(18, 114);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(69, 15);
            this.lblPrice.TabIndex = 4;
            this.lblPrice.Text = "Birim Fiyat:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblName.Location = new System.Drawing.Point(18, 70);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(59, 15);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "Ürün Adı:";
            // 
            // lblBarcode
            // 
            this.lblBarcode.AutoSize = true;
            this.lblBarcode.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBarcode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblBarcode.Location = new System.Drawing.Point(18, 26);
            this.lblBarcode.Name = "lblBarcode";
            this.lblBarcode.Size = new System.Drawing.Size(50, 15);
            this.lblBarcode.TabIndex = 0;
            this.lblBarcode.Text = "Barkod:";
            // 
            // txtInitialStock
            // 
            this.txtInitialStock.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(69)))), ((int)(((byte)(96)))));
            this.txtInitialStock.BorderRadius = 8;
            this.txtInitialStock.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtInitialStock.DefaultText = "";
            this.txtInitialStock.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.txtInitialStock.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtInitialStock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtInitialStock.Location = new System.Drawing.Point(110, 170);
            this.txtInitialStock.Name = "txtInitialStock";
            this.txtInitialStock.PasswordChar = '\0';
            this.txtInitialStock.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.txtInitialStock.PlaceholderText = "Açılış Stoğu (İsteğe bağlı)";
            this.txtInitialStock.SelectedText = "";
            this.txtInitialStock.Size = new System.Drawing.Size(260, 43);
            this.txtInitialStock.TabIndex = 6;
            // 
            // lblInitialStock
            // 
            this.lblInitialStock.AutoSize = true;
            this.lblInitialStock.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInitialStock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblInitialStock.Location = new System.Drawing.Point(18, 164);
            this.lblInitialStock.Name = "lblInitialStock";
            this.lblInitialStock.Size = new System.Drawing.Size(73, 15);
            this.lblInitialStock.TabIndex = 11;
            this.lblInitialStock.Text = "Başlan. Stok:";
            // 
            // tabMovements
            // 
            this.tabMovements.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.tabMovements.Controls.Add(this.dgvMovements);
            this.tabMovements.Controls.Add(this.btnAddMovement);
            this.tabMovements.Controls.Add(this.cmbType);
            this.tabMovements.Controls.Add(this.lblType);
            this.tabMovements.Controls.Add(this.nudQuantity);
            this.tabMovements.Controls.Add(this.lblQuantity);
            this.tabMovements.Controls.Add(this.txtBarcodeMovement);
            this.tabMovements.Controls.Add(this.lblBarcodeMovement);
            this.tabMovements.Location = new System.Drawing.Point(4, 29);
            this.tabMovements.Name = "tabMovements";
            this.tabMovements.Padding = new System.Windows.Forms.Padding(14);
            this.tabMovements.Size = new System.Drawing.Size(1092, 607);
            this.tabMovements.TabIndex = 1;
            this.tabMovements.Text = "📋  Stok Hareketleri";
            // 
            // dgvMovements
            // 
            this.dgvMovements.AllowUserToAddRows = false;
            this.dgvMovements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMovements.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            this.dgvMovements.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvMovements.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMovements.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(33)))), ((int)(((byte)(62)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMovements.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvMovements.EnableHeadersVisualStyles = false;
            this.dgvMovements.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(55)))), ((int)(((byte)(90)))));
            this.dgvMovements.Location = new System.Drawing.Point(18, 110);
            this.dgvMovements.Name = "dgvMovements";
            this.dgvMovements.ReadOnly = true;
            this.dgvMovements.RowHeadersVisible = false;
            this.dgvMovements.Size = new System.Drawing.Size(1056, 480);
            this.dgvMovements.TabIndex = 7;
            // 
            // btnAddMovement
            // 
            this.btnAddMovement.BorderRadius = 8;
            this.btnAddMovement.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAddMovement.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddMovement.ForeColor = System.Drawing.Color.White;
            this.btnAddMovement.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(174)))), ((int)(((byte)(93)))));
            this.btnAddMovement.Location = new System.Drawing.Point(720, 18);
            this.btnAddMovement.Name = "btnAddMovement";
            this.btnAddMovement.Size = new System.Drawing.Size(200, 38);
            this.btnAddMovement.TabIndex = 6;
            this.btnAddMovement.Text = "✔  Hareket Kaydet";
            this.btnAddMovement.Click += new System.EventHandler(this.btnAddMovement_Click);
            // 
            // cmbType
            // 
            this.cmbType.BackColor = System.Drawing.Color.Transparent;
            this.cmbType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.cmbType.FocusedColor = System.Drawing.Color.Empty;
            this.cmbType.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.cmbType.ItemHeight = 30;
            this.cmbType.Items.AddRange(new object[] {
            "Giriş",
            "Çıkış"});
            this.cmbType.Location = new System.Drawing.Point(575, 18);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(130, 36);
            this.cmbType.TabIndex = 5;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblType.Location = new System.Drawing.Point(535, 24);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(28, 15);
            this.lblType.TabIndex = 8;
            this.lblType.Text = "Tür:";
            // 
            // nudQuantity
            // 
            this.nudQuantity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.nudQuantity.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudQuantity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.nudQuantity.Location = new System.Drawing.Point(430, 18);
            this.nudQuantity.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudQuantity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudQuantity.Name = "nudQuantity";
            this.nudQuantity.Size = new System.Drawing.Size(90, 25);
            this.nudQuantity.TabIndex = 3;
            this.nudQuantity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblQuantity
            // 
            this.lblQuantity.AutoSize = true;
            this.lblQuantity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblQuantity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblQuantity.Location = new System.Drawing.Point(365, 24);
            this.lblQuantity.Name = "lblQuantity";
            this.lblQuantity.Size = new System.Drawing.Size(47, 15);
            this.lblQuantity.TabIndex = 9;
            this.lblQuantity.Text = "Miktar:";
            // 
            // txtBarcodeMovement
            // 
            this.txtBarcodeMovement.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.txtBarcodeMovement.BorderRadius = 8;
            this.txtBarcodeMovement.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtBarcodeMovement.DefaultText = "";
            this.txtBarcodeMovement.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
            this.txtBarcodeMovement.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBarcodeMovement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtBarcodeMovement.Location = new System.Drawing.Point(110, 20);
            this.txtBarcodeMovement.Name = "txtBarcodeMovement";
            this.txtBarcodeMovement.PasswordChar = '\0';
            this.txtBarcodeMovement.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.txtBarcodeMovement.PlaceholderText = "Ürün barkodu";
            this.txtBarcodeMovement.SelectedText = "";
            this.txtBarcodeMovement.Size = new System.Drawing.Size(240, 43);
            this.txtBarcodeMovement.TabIndex = 1;
            // 
            // lblBarcodeMovement
            // 
            this.lblBarcodeMovement.AutoSize = true;
            this.lblBarcodeMovement.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBarcodeMovement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.lblBarcodeMovement.Location = new System.Drawing.Point(18, 24);
            this.lblBarcodeMovement.Name = "lblBarcodeMovement";
            this.lblBarcodeMovement.Size = new System.Drawing.Size(50, 15);
            this.lblBarcodeMovement.TabIndex = 10;
            this.lblBarcodeMovement.Text = "Barkod:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1100, 640);
            this.Controls.Add(this.tabControl);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Poseidon Yazılım — Depo & Stok Takip Sistemi";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl.ResumeLayout(false);
            this.tabProducts.ResumeLayout(false);
            this.tabProducts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.tabMovements.ResumeLayout(false);
            this.tabMovements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMovements)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabProducts;
        private System.Windows.Forms.DataGridView dgvProducts;
        private Guna2Button btnAdd;
        private Guna2Button btnPos;
        private Guna2Button btnReturns;
        private Guna2Button btnCustomers;
        private Guna2Button btnReports;
        private Guna2TextBox txtPrice;
        private Guna2TextBox txtName;
        private Guna2TextBox txtBarcode;
        private Guna2TextBox txtInitialStock;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblBarcode;
        private System.Windows.Forms.Label lblInitialStock;
        private System.Windows.Forms.TabPage tabMovements;
        private System.Windows.Forms.DataGridView dgvMovements;
        private Guna2Button btnAddMovement;
        private Guna2ComboBox cmbType;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.NumericUpDown nudQuantity;
        private System.Windows.Forms.Label lblQuantity;
        private Guna2TextBox txtBarcodeMovement;
        private System.Windows.Forms.Label lblBarcodeMovement;
    }
}
