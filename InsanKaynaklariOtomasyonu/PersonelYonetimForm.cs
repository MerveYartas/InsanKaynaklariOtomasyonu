using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InsanKaynaklariOtomasyonu
{
    public partial class PersonelYonetimiForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private DataGridView dgvPersonel;
        private TextBox txtAd, txtSoyad, txtMaas, txtAra;
        private ComboBox cbGorev, cbDepartman, cbKidem;
        private Button btnEkle, btnSil, btnGuncelle, btnAra;

        private string dbPath;
        private string connectionString;

        public PersonelYonetimiForm()
        {
            SetupHeader();
            SetupControls();
            ConfigureDatabaseConnection();
            InitializeDatabase();
            LoadData();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.BackColor = Color.FromArgb(240, 238, 233);
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
                    da = new SQLiteDataAdapter("SELECT * FROM Personel", con);
                else
                {
                    da = new SQLiteDataAdapter("SELECT * FROM Personel WHERE Ad LIKE @filter OR Soyad LIKE @filter OR Kidem LIKE @filter  OR Gorev LIKE @filter  OR Departman LIKE @filter ", con);
                    da.SelectCommand.Parameters.AddWithValue("@filter", "%" + filter + "%");
                }

                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvPersonel.DataSource = dt;
            }
        }

        private void SetupHeader()
        {
            Panel headerPanel = new Panel()
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(224, 219, 211)
            };
            headerPanel.MouseDown += HeaderPanel_MouseDown;

            Label titleLabel = new Label()
            {
                Text = "Personel Yönetimi",
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
                Padding = new Padding(0, 7, 0, 0)
            };

            Button btnClose = new Button()
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Size = new Size(35, 30),
                BackColor = Color.Transparent,
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
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(130, 120, 110),
                Cursor = Cursors.Hand,
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) =>
            {
                this.Hide();
                new MainForm().Show();
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

        private void SetupControls()
        {
            // DataGridView
            dgvPersonel = new DataGridView()
            {
                Location = new Point(20, 60),
                Size = new Size(840, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvPersonel.CellClick += DgvPersonel_CellClick;

            // Label - Ad
            Label lblAd = new Label() { Text = "Ad:", Location = new Point(20, 370), AutoSize = true };
            // TextBox - Ad
            txtAd = new TextBox() { Location = new Point(20, 390), Width = 180 };

            // Label - Soyad
            Label lblSoyad = new Label() { Text = "Soyad:", Location = new Point(220, 370), AutoSize = true };
            // TextBox - Soyad
            txtSoyad = new TextBox() { Location = new Point(220, 390), Width = 180 };

            // Label - Maaş
            Label lblMaas = new Label() { Text = "Maaş:", Location = new Point(420, 370), AutoSize = true };
            // TextBox - Maaş
            txtMaas = new TextBox() { Location = new Point(420, 390), Width = 180 };
            txtMaas.KeyPress += TxtMaas_KeyPress;

            // Label - Görev
            Label lblGorev = new Label() { Text = "Görev:", Location = new Point(20, 420), AutoSize = true };
            // ComboBox - Görev
            cbGorev = new ComboBox() { Location = new Point(20, 440), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGorev.Items.AddRange(new string[] { "Yazılımcı", "Analist", "Sekreter", "Teknisyen" });

            // Label - Departman
            Label lblDepartman = new Label() { Text = "Departman:", Location = new Point(220, 420), AutoSize = true };
            // ComboBox - Departman
            cbDepartman = new ComboBox() { Location = new Point(220, 440), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbDepartman.Items.AddRange(new string[] { "IT", "İdari", "Bakım" });

            // Label - Kıdem
            Label lblKidem = new Label() { Text = "Kıdem:", Location = new Point(420, 420), AutoSize = true };
            // ComboBox - Kıdem
            cbKidem = new ComboBox() { Location = new Point(420, 440), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cbKidem.Items.AddRange(new string[] { "0-1 yıl", "1-3 yıl", "3-5 yıl", "5+ yıl" });

            // TextBox - Ara
            txtAra = new TextBox() { Location = new Point(20, 490), Width = 180 };
            Label lblAra = new Label() { Text = "Ara:", Location = new Point(20, 470), AutoSize = true };

            // Butonlar
            btnEkle = new Button() { Text = "Ekle", Location = new Point(620, 390), Width = 100 };
            btnSil = new Button() { Text = "Sil", Location = new Point(730, 390), Width = 100 };
            btnGuncelle = new Button() { Text = "Güncelle", Location = new Point(620, 430), Width = 100 };
            btnAra = new Button() { Text = "Ara", Location = new Point(220, 480), Width = 100 };

            btnEkle.Click += BtnEkle_Click;
            btnSil.Click += BtnSil_Click;
            btnGuncelle.Click += BtnGuncelle_Click;
            btnAra.Click += BtnAra_Click;

            this.Controls.AddRange(new Control[]
            {
                dgvPersonel,

                lblAd, txtAd,
                lblSoyad, txtSoyad,
                lblMaas, txtMaas,
                lblGorev, cbGorev,
                lblDepartman, cbDepartman,
                lblKidem, cbKidem,

                lblAra, txtAra,

                btnEkle, btnSil, btnGuncelle, btnAra
            });
        }


        private void TxtMaas_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void BtnEkle_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Personel (Ad, Soyad, Maas, Gorev, Departman, Kidem) VALUES (@Ad, @Soyad, @Maas, @Gorev, @Departman, @Kidem)", con);
                cmd.Parameters.AddWithValue("@Ad", txtAd.Text);
                cmd.Parameters.AddWithValue("@Soyad", txtSoyad.Text);
                cmd.Parameters.AddWithValue("@Maas", txtMaas.Text);
                cmd.Parameters.AddWithValue("@Gorev", cbGorev.Text);
                cmd.Parameters.AddWithValue("@Departman", cbDepartman.Text);
                cmd.Parameters.AddWithValue("@Kidem", cbKidem.Text);
                cmd.ExecuteNonQuery();
            }
            LoadData();
            Temizle();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgvPersonel.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvPersonel.SelectedRows[0].Cells["Id"].Value);
                using (SQLiteConnection con = new SQLiteConnection(connectionString))
                {
                    con.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Personel WHERE Id=@Id", con);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
                LoadData();
                Temizle();
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvPersonel.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Güncellemek için bir personel seçmelisiniz.");
                    return;
                }

                if (!decimal.TryParse(txtMaas.Text, out decimal maas))
                {
                    MessageBox.Show("Lütfen geçerli bir maaş değeri giriniz.");
                    return;
                }

                int id = Convert.ToInt32(dgvPersonel.SelectedRows[0].Cells["Id"].Value);

                using (SQLiteConnection con = new SQLiteConnection(connectionString))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("UPDATE Personel SET Ad=@Ad, Soyad=@Soyad, Maas=@Maas, Gorev=@Gorev, Departman=@Departman, Kidem=@Kidem WHERE Id=@Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Ad", txtAd.Text);
                        cmd.Parameters.AddWithValue("@Soyad", txtSoyad.Text);
                        cmd.Parameters.Add("@Maas", DbType.Double).Value = maas;
                        cmd.Parameters.AddWithValue("@Gorev", cbGorev.Text);
                        cmd.Parameters.AddWithValue("@Departman", cbDepartman.Text);
                        cmd.Parameters.AddWithValue("@Kidem", cbKidem.Text);
                        cmd.Parameters.AddWithValue("@Id", id);

                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Güncelleme başarılı!");
                        }
                        else
                        {
                            MessageBox.Show("Güncelleme başarısız! ID bulunamadı.");
                        }
                    }
                }

                LoadData();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu:\n{ex.Message}");
            }
        }



        private void BtnAra_Click(object sender, EventArgs e)
        {
            LoadData(txtAra.Text.Trim());
        }

        private void DgvPersonel_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvPersonel.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPersonel.SelectedRows[0];
                txtAd.Text = row.Cells["Ad"].Value.ToString();
                txtSoyad.Text = row.Cells["Soyad"].Value.ToString();
                txtMaas.Text = row.Cells["Maas"].Value.ToString();
                cbGorev.Text = row.Cells["Gorev"].Value.ToString();
                cbDepartman.Text = row.Cells["Departman"].Value.ToString();
                cbKidem.Text = row.Cells["Kidem"].Value.ToString();
            }
        }

        private void InitializeDatabase()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS Personel (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Ad TEXT NOT NULL,
                    Soyad TEXT NOT NULL,
                    Maas REAL NOT NULL,
                    Gorev TEXT NOT NULL,
                    Departman TEXT NOT NULL,
                    Kidem TEXT NOT NULL
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void Temizle()
        {
            txtAd.Clear();
            txtSoyad.Clear();
            txtMaas.Clear();
            cbGorev.SelectedIndex = -1;
            cbDepartman.SelectedIndex = -1;
            cbKidem.SelectedIndex = -1;
            dgvPersonel.ClearSelection();
        }

    }
}
