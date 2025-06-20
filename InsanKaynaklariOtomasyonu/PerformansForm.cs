using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InsanKaynaklariOtomasyonu
{
    public partial class PerformansForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private ComboBox cbPersonel, cbYil;
        private NumericUpDown numPuan;
        private TextBox txtAciklama;
        private Button btnKaydet, btnGüncelle, btnSil;
        private DataGridView dgvPerformans;

        private string dbPath;
        private string connectionString;

        private int selectedPerformansId = -1;

        public PerformansForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.BackColor = Color.FromArgb(240, 238, 233);

            ConfigureDatabaseConnection();
            InitializeDatabase();
            SetupHeader();
            InitializeCustomComponents();
            LoadPersonel();
            LoadPerformansGrid();
        }

        private void ConfigureDatabaseConnection()
        {
            string appDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
            if (!Directory.Exists(appDataFolder)) Directory.CreateDirectory(appDataFolder);

            dbPath = Path.Combine(appDataFolder, "ikdb.sqlite");
            connectionString = $"Data Source={dbPath};Version=3;";
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string personelTable = @"CREATE TABLE Personel (Id INTEGER PRIMARY KEY AUTOINCREMENT, Ad TEXT, Soyad TEXT)";
                    string performansTable = @"CREATE TABLE Performans (Id INTEGER PRIMARY KEY AUTOINCREMENT, PersonelId INTEGER, Yil INTEGER, Puan INTEGER, Aciklama TEXT, FOREIGN KEY (PersonelId) REFERENCES Personel(Id))";
                    new SQLiteCommand(personelTable, conn).ExecuteNonQuery();
                    new SQLiteCommand(performansTable, conn).ExecuteNonQuery();
                }
            }
        }

        private void SetupHeader()
        {
            Panel headerPanel = new Panel() { Height = 45, Dock = DockStyle.Top, BackColor = Color.FromArgb(224, 219, 211) };
            headerPanel.MouseDown += HeaderPanel_MouseDown;

            Label titleLabel = new Label() { Text = "Performans Değerlendirme", ForeColor = Color.FromArgb(85, 80, 70), Font = new Font("Segoe UI Semilight", 16), AutoSize = true, Location = new Point(20, 7) };

            FlowLayoutPanel rightButtonsPanel = new FlowLayoutPanel() { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Right, Width = 100, Padding = new Padding(0, 7, 0, 0) };

            Button btnClose = new Button() { Text = "✕", FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12), Size = new Size(35, 30), BackColor = Color.Transparent, ForeColor = Color.FromArgb(130, 120, 110), Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            Button btnBack = new Button() { Text = "←", FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12), Size = new Size(35, 30), BackColor = Color.Transparent, ForeColor = Color.FromArgb(130, 120, 110), Cursor = Cursors.Hand };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => { this.Hide(); new MainForm().Show(); };

            rightButtonsPanel.Controls.Add(btnBack);
            rightButtonsPanel.Controls.Add(btnClose);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(rightButtonsPanel);
            this.Controls.Add(headerPanel);
        }

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void InitializeCustomComponents()
        {
            cbPersonel = new ComboBox() { Location = new Point(120, 65), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbYil = new ComboBox() { Location = new Point(120, 105), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            for (int y = 2020; y <= DateTime.Now.Year; y++) cbYil.Items.Add(y);
            cbYil.SelectedIndex = cbYil.Items.Count - 1;

            numPuan = new NumericUpDown() { Location = new Point(120, 145), Maximum = 100, Minimum = 0, Width = 150 };
            txtAciklama = new TextBox() { Location = new Point(120, 185), Width = 250, Height = 50, Multiline = true };

            btnKaydet = new Button() { Text = "Kaydet", Location = new Point(300, 65), Width = 100 };
            btnKaydet.Click += BtnKaydet_Click;

            btnGüncelle = new Button() { Text = "Güncelle", Location = new Point(300, 105), Width = 100 };
            btnGüncelle.Click += BtnGüncelle_Click;

            btnSil = new Button() { Text = "Sil", Location = new Point(300, 145), Width = 100 };
            btnSil.Click += BtnSil_Click;

            dgvPerformans = new DataGridView() { Location = new Point(30, 250), Size = new Size(820, 300), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvPerformans.SelectionChanged += DgvPerformans_SelectionChanged;

            Controls.AddRange(new Control[]
            {
                new Label() { Text = "Personel:", Location = new Point(30, 70), AutoSize = true },
                cbPersonel,
                new Label() { Text = "Yıl:", Location = new Point(30, 110), AutoSize = true },
                cbYil,
                new Label() { Text = "Puan (0-100):", Location = new Point(30, 150), AutoSize = true },
                numPuan,
                new Label() { Text = "Açıklama:", Location = new Point(30, 190), AutoSize = true },
                txtAciklama,
                btnKaydet, btnGüncelle, btnSil,
                dgvPerformans
            });
        }

        private void LoadPersonel()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT Id, Ad || ' ' || Soyad AS AdSoyad FROM Personel", conn);
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                cbPersonel.DataSource = dt;
                cbPersonel.DisplayMember = "AdSoyad";
                cbPersonel.ValueMember = "Id";
                cbPersonel.SelectedIndex = -1;
            }
        }

        private void LoadPerformansGrid()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var da = new SQLiteDataAdapter(@"
                    SELECT p.Id AS PerformansId, per.Ad || ' ' || per.Soyad AS PersonelAdSoyad, 
                           p.Yil, p.Puan, p.Aciklama
                    FROM Performans p
                    INNER JOIN Personel per ON p.PersonelId = per.Id", conn);
                var dt = new DataTable();
                da.Fill(dt);
                dgvPerformans.DataSource = dt;
            }

            selectedPerformansId = -1;
            ClearInputFields();
        }

        private void ClearInputFields()
        {
            cbPersonel.SelectedIndex = -1;
            cbYil.SelectedIndex = cbYil.Items.Count - 1;
            numPuan.Value = 0;
            txtAciklama.Clear();
        }

        private void DgvPerformans_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPerformans.SelectedRows.Count == 0) return;

            var row = dgvPerformans.SelectedRows[0];
            selectedPerformansId = Convert.ToInt32(row.Cells["PerformansId"].Value);
            cbPersonel.Text = row.Cells["PersonelAdSoyad"].Value.ToString();
            cbYil.SelectedItem = Convert.ToInt32(row.Cells["Yil"].Value);
            numPuan.Value = Convert.ToDecimal(row.Cells["Puan"].Value);
            txtAciklama.Text = row.Cells["Aciklama"].Value.ToString();
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (cbPersonel.SelectedIndex == -1 || cbYil.SelectedIndex == -1)
            {
                MessageBox.Show("Personel ve yıl seçimi zorunludur.");
                return;
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO Performans (PersonelId, Yil, Puan, Aciklama) VALUES (@pid, @yil, @puan, @aciklama)", conn);
                cmd.Parameters.AddWithValue("@pid", cbPersonel.SelectedValue);
                cmd.Parameters.AddWithValue("@yil", cbYil.SelectedItem);
                cmd.Parameters.AddWithValue("@puan", numPuan.Value);
                cmd.Parameters.AddWithValue("@aciklama", txtAciklama.Text);
                cmd.ExecuteNonQuery();
            }

            LoadPerformansGrid();
        }

        private void BtnGüncelle_Click(object sender, EventArgs e)
        {
            if (selectedPerformansId == -1)
            {
                MessageBox.Show("Güncellenecek performans seçilmedi.");
                return;
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand("UPDATE Performans SET PersonelId=@pid, Yil=@yil, Puan=@puan, Aciklama=@aciklama WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@pid", cbPersonel.SelectedValue);
                cmd.Parameters.AddWithValue("@yil", cbYil.SelectedItem);
                cmd.Parameters.AddWithValue("@puan", numPuan.Value);
                cmd.Parameters.AddWithValue("@aciklama", txtAciklama.Text);
                cmd.Parameters.AddWithValue("@id", selectedPerformansId);
                cmd.ExecuteNonQuery();
            }

            LoadPerformansGrid();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (selectedPerformansId == -1)
            {
                MessageBox.Show("Silinecek performans seçilmedi.");
                return;
            }

            if (MessageBox.Show("Silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("DELETE FROM Performans WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", selectedPerformansId);
                    cmd.ExecuteNonQuery();
                }

                LoadPerformansGrid();
            }
        }
    }
}
