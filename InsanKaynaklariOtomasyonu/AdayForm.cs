using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace InsanKaynaklariOtomasyonu
{
    public partial class AdayForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private DataGridView dgvAdaylar;
        private Button btnEkle, btnSil, btnGuncelle, btnCalisanYap;
        private ComboBox cbEgitim, cbTecrube, cbGorev, cbDepartman;
        private NumericUpDown numYas;
        private TextBox txtAd, txtSoyad, txtAra;
        private Button btnFiltre;

        private string dbPath;
        private string connectionString;

        public AdayForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 620);
            this.BackColor = Color.FromArgb(240, 238, 233);

            SetupHeader();
            ConfigureDatabaseConnection();
            InitializeDatabase();
            InitializeCustomComponents();
        }

        private void ConfigureDatabaseConnection()
        {
            string appDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
            dbPath = Path.Combine(appDataFolder, "ikdb.sqlite");
            connectionString = $"Data Source={dbPath};Version=3;";
        }

        private void LoadData(string filter = "")
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteDataAdapter da;

                if (string.IsNullOrWhiteSpace(filter))
                    da = new SQLiteDataAdapter("SELECT * FROM Adaylar", con);
                else
                {
                    da = new SQLiteDataAdapter("SELECT * FROM Adaylar WHERE Ad LIKE @filter OR Soyad LIKE @filter OR Egitim LIKE @filter OR Tecrube LIKE @filter", con);
                    da.SelectCommand.Parameters.AddWithValue("@filter", "%" + filter + "%");
                }

                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvAdaylar.DataSource = dt;
            }
        }

        private void SetupHeader()
        {
            Panel headerPanel = new Panel()
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(224, 219, 211),
                Padding = new Padding(15, 0, 15, 0),
            };
            headerPanel.MouseDown += HeaderPanel_MouseDown;

            Label titleLabel = new Label()
            {
                Text = "Aday Takip Formu",
                ForeColor = Color.FromArgb(85, 80, 70),
                Font = new Font("Segoe UI Semilight", 16),
                AutoSize = true,
                Location = new Point(20, 7)
            };

            FlowLayoutPanel rightButtonsPanel = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Right,
                Width = 100,
                Padding = new Padding(0, 7, 0, 0),
                BackColor = Color.Transparent
            };

            Button btnClose = new Button()
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Size = new Size(35, 30),
                BackColor = Color.FromArgb(224, 219, 211),
                ForeColor = Color.FromArgb(130, 120, 110),
                Cursor = Cursors.Hand,
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            Button btnBack = new Button()
            {
                Text = "←",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Size = new Size(35, 30),
                BackColor = Color.FromArgb(224, 219, 211),
                ForeColor = Color.FromArgb(130, 120, 110),
                Cursor = Cursors.Hand,
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) =>
            {
                this.Hide();
                new MainForm().Show(); // MainForm varsa açılır
            };

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
            Label lblAd = new Label { Text = "Ad:", Location = new Point(20, 60) };
            txtAd = new TextBox { Location = new Point(120, 60), Width = 130 };

            Label lblSoyad = new Label { Text = "Soyad:", Location = new Point(270, 60) };
            txtSoyad = new TextBox { Location = new Point(370, 60), Width = 130 };

            Label lblEgitim = new Label { Text = "Eğitim:", Location = new Point(20, 100) };
            cbEgitim = new ComboBox { Location = new Point(120, 100), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cbEgitim.Items.AddRange(new[] { "Lise", "Ön Lisans", "Lisans", "Yüksek Lisans", "Doktora" });

            Label lblTecrube = new Label { Text = "Tecrübe:", Location = new Point(270, 100) };
            cbTecrube = new ComboBox { Location = new Point(370, 100), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cbTecrube.Items.AddRange(new[] { "0-1 yıl", "1-3 yıl", "3-5 yıl", "5+ yıl" });

            Label lblYas = new Label { Text = "Yaş:", Location = new Point(20, 140) };
            numYas = new NumericUpDown { Location = new Point(120, 140), Width = 130, Minimum = 18, Maximum = 65, Value = 25 };

            Label lblGorev = new Label { Text = "Görev:", Location = new Point(270, 140) };
            cbGorev = new ComboBox { Location = new Point(370, 140), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGorev.Items.AddRange(new string[] { "Yazılımcı", "Analist", "Sekreter", "Teknisyen" });

            Label lblDepartman = new Label { Text = "Departman:", Location = new Point(20, 180) };
            cbDepartman = new ComboBox { Location = new Point(120, 180), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cbDepartman.Items.AddRange(new string[] { "IT", "İdari", "Bakım" });

            Label lblMaas = new Label { Text = "Maaş:", Location = new Point(270, 180) };
            NumericUpDown numMaas = new NumericUpDown { Name = "numMaas", Location = new Point(370, 180), Width = 130, Maximum = 1000000, DecimalPlaces = 2, Increment = 500 };

            btnEkle = new Button { Text = "Ekle", Location = new Point(530, 60), Width = 100 };
            btnGuncelle = new Button { Text = "Güncelle", Location = new Point(530, 100), Width = 100 };
            btnSil = new Button { Text = "Sil", Location = new Point(530, 140), Width = 100 };
            btnCalisanYap = new Button { Text = "Çalışan Yap", Location = new Point(530, 180), Width = 100 };

            dgvAdaylar = new DataGridView
            {
                Location = new Point(20, 230),
                Size = new Size(850, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            txtAra = new TextBox { Location = new Point(20, 550), Width = 200 };
            btnFiltre = new Button { Text = "Filtrele", Location = new Point(230, 548), Width = 100 };

            this.Controls.AddRange(new Control[] {
                lblAd, txtAd,
                lblSoyad, txtSoyad,
                lblEgitim, cbEgitim,
                lblTecrube, cbTecrube,
                lblYas, numYas,
                lblGorev, cbGorev,
                lblDepartman, cbDepartman,
                lblMaas, numMaas,
                btnEkle, btnGuncelle, btnSil, btnCalisanYap,
                dgvAdaylar, txtAra, btnFiltre
            });

            btnEkle.Click += BtnEkle_Click;
            btnGuncelle.Click += BtnGuncelle_Click;
            btnSil.Click += BtnSil_Click;
            btnFiltre.Click += BtnFiltre_Click;
            btnCalisanYap.Click += BtnCalisanYap_Click;
            dgvAdaylar.CellClick += DgvAdaylar_CellClick;


            LoadData();
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Adaylar (Ad, Soyad, Egitim, Tecrube, Yas, Gorev, Departman, Maas) " +
                               "VALUES (@Ad, @Soyad, @Egitim, @Tecrube, @Yas, @Gorev, @Departman, @Maas)";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@Ad", txtAd.Text);
                cmd.Parameters.AddWithValue("@Soyad", txtSoyad.Text);
                cmd.Parameters.AddWithValue("@Egitim", cbEgitim.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Tecrube", cbTecrube.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Yas", Convert.ToInt32(numYas.Value));
                cmd.Parameters.AddWithValue("@Gorev", cbGorev.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Departman", cbDepartman.SelectedItem?.ToString() ?? "");

                // NumericUpDown'u doğrudan bulamıyorsan Controls koleksiyonundan al:
                var numMaas = this.Controls.Find("numMaas", true).FirstOrDefault() as NumericUpDown;
                decimal maasDegeri = numMaas != null ? numMaas.Value : 0;
                cmd.Parameters.AddWithValue("@Maas", maasDegeri);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Aday başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(); // Tabloyu yenile
                Temizle();
            }
        }


        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvAdaylar.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvAdaylar.SelectedRows[0].Cells["Id"].Value);
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Adaylar SET Ad=@Ad, Soyad=@Soyad, Egitim=@Egitim, Tecrube=@Tecrube, Yas=@Yas WHERE Id=@Id";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@Ad", txtAd.Text);
                cmd.Parameters.AddWithValue("@Soyad", txtSoyad.Text);
                cmd.Parameters.AddWithValue("@Egitim", cbEgitim.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Tecrube", cbTecrube.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Yas", (int)numYas.Value);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            LoadData();
            Temizle();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgvAdaylar.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvAdaylar.SelectedRows[0].Cells["Id"].Value);
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Adaylar WHERE Id=@Id";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            LoadData();
            Temizle();
        }

        private void BtnFiltre_Click(object sender, EventArgs e)
        {
            LoadData(txtAra.Text);
        }

        private void DgvAdaylar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvAdaylar.Rows[e.RowIndex];
            txtAd.Text = row.Cells["Ad"].Value.ToString();
            txtSoyad.Text = row.Cells["Soyad"].Value.ToString();
            cbEgitim.SelectedItem = row.Cells["Egitim"].Value.ToString();
            cbTecrube.SelectedItem = row.Cells["Tecrube"].Value.ToString();
            cbDepartman.SelectedItem = row.Cells["Departman"].Value.ToString();
            cbGorev.SelectedItem = row.Cells["Gorev"].Value.ToString();
            numYas.Value = Convert.ToDecimal(row.Cells["Yas"].Value);
        }

        private void Temizle()
        {
            txtAd.Clear();
            txtSoyad.Clear();
            cbEgitim.SelectedIndex = -1;
            cbTecrube.SelectedIndex = -1;
            cbDepartman.SelectedIndex = -1;
            cbGorev.SelectedIndex = -1;
            numYas.Value = 25;
            ((NumericUpDown)this.Controls["numMaas"]).Value = 0;
        }


        private void BtnCalisanYap_Click(object sender, EventArgs e)
        {
            if (dgvAdaylar.SelectedRows.Count == 0) return;

            DataGridViewRow selectedRow = dgvAdaylar.SelectedRows[0];

            int adayId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string ad = selectedRow.Cells["Ad"].Value.ToString();
            string soyad = selectedRow.Cells["Soyad"].Value.ToString();
            string egitim = selectedRow.Cells["Egitim"].Value.ToString();
            string tecrube = selectedRow.Cells["Tecrube"].Value.ToString();

            // Yeni eklenen sütunlar:
            string gorev = selectedRow.Cells["Gorev"].Value.ToString();
            string departman = selectedRow.Cells["Departman"].Value.ToString();
            decimal maas = Convert.ToDecimal(selectedRow.Cells["Maas"].Value);

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Personel tablosuna ekle
                string insertCalisan = "INSERT INTO Personel (Ad, Soyad, Gorev, Departman, Kidem, Maas) " +
                                       "VALUES (@Ad, @Soyad, @Gorev, @Departman, @Kidem, @Maas)";
                SQLiteCommand cmdInsert = new SQLiteCommand(insertCalisan, conn);
                cmdInsert.Parameters.AddWithValue("@Ad", ad);
                cmdInsert.Parameters.AddWithValue("@Soyad", soyad);
                cmdInsert.Parameters.AddWithValue("@Gorev", gorev);
                cmdInsert.Parameters.AddWithValue("@Departman", departman);
                cmdInsert.Parameters.AddWithValue("@Kidem", tecrube); // Tecrübe -> Kıdem olarak atanıyor
                cmdInsert.Parameters.AddWithValue("@Maas", maas);

                cmdInsert.ExecuteNonQuery();

                // Adayı sil
                string deleteAday = "DELETE FROM Adaylar WHERE Id = @Id";
                SQLiteCommand cmdDelete = new SQLiteCommand(deleteAday, conn);
                cmdDelete.Parameters.AddWithValue("@Id", adayId);
                cmdDelete.ExecuteNonQuery();
            }

            LoadData();
            Temizle();
        }
        private void InitializeDatabase()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS Adaylar (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Ad TEXT NOT NULL,
                    Soyad TEXT NOT NULL,
                    Egitim TEXT NOT NULL,
                    Tecrube TEXT NOT NULL,
                    Yas INTEGER NOT NULL,
                    Gorev TEXT NOT NULL, -- Yeni sütun eklendi
                    Departman TEXT NOT NULL,
                    Maas REAL NOT NULL
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
