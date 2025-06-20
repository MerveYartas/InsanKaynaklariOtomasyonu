using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InsanKaynaklariOtomasyonu
{
    public partial class EgitimOrganizasyonForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private TextBox txtBaslik, txtEgitmen;
        private DateTimePicker dtTarih;
        private CheckedListBox clbKatilimcilar;
        private Button btnEkle,btnSil,btnGüncelle, btnListele;
        private DataGridView dgvEgitimler;

        private string dbPath;
        private string connectionString;

        public EgitimOrganizasyonForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.BackColor = Color.FromArgb(240, 238, 233);

            SetupHeader(); // Header bar
            ConfigureDatabaseConnection();
            InitializeDatabase(); // Veritabanını başlat
            InitializeCustomComponents();

            this.Load += EgitimOrganizasyonForm_Load; // Form load eventi
        }

        private void ConfigureDatabaseConnection() // veritabanı bağlantısını ayarla
        {
            string appDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data"); // uygulama verileri klasörü
            dbPath = Path.Combine(appDataFolder, "ikdb.sqlite"); // klasör ve veritabanı dosyası
            connectionString = $"Data Source={dbPath};Version=3;"; // SQLite bağlantı dizesi
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
                Text = "Eğitim / Organizasyon",
                ForeColor = Color.FromArgb(85, 80, 70),
                Font = new Font("Segoe UI Semilight", 16, FontStyle.Regular),
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
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.DarkRed;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(130, 120, 110);

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
                new MainForm().Show(); // Geri dönüş formunu belirt
            };
            btnBack.MouseEnter += (s, e) => btnBack.ForeColor = Color.FromArgb(80, 100, 180);
            btnBack.MouseLeave += (s, e) => btnBack.ForeColor = Color.FromArgb(130, 120, 110);

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
            int headerHeight = 45;
            int margin = 20;

            this.Text = "Eğitim ve Organizasyon";
            this.Size = new Size(900, 600);

            Panel contentPanel = new Panel()
            {
                Location = new Point(0, headerHeight),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - headerHeight),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(contentPanel);

            Label lblBaslik = new Label() { Text = "Başlık:", Location = new Point(margin, 30), AutoSize = true };
            txtBaslik = new TextBox() { Location = new Point(margin + 90, 25), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblEgitmen = new Label() { Text = "Eğitmen:", Location = new Point(margin, 70), AutoSize = true };
            txtEgitmen = new TextBox() { Location = new Point(margin + 90, 65), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblTarih = new Label() { Text = "Tarih:", Location = new Point(margin, 110), AutoSize = true };
            dtTarih = new DateTimePicker() { Location = new Point(margin + 90, 105), Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label lblKatilim = new Label() { Text = "Katılımcılar:", Location = new Point(margin, 150), AutoSize = true };
            clbKatilimcilar = new CheckedListBox() { Location = new Point(margin + 90, 145), Width = 200, Height = 100, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            btnEkle = new Button() { Text = "Eğitim Ekle", Location = new Point(margin + 320, 25), Size = new Size(120, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnSil = new Button() { Text = "Eğitim Sil", Location = new Point(margin + 320, 65), Size = new Size(120, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnGüncelle = new Button() { Text = "Eğitim Güncelle", Location = new Point(margin + 320, 105), Size = new Size(120, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnListele = new Button() { Text = "Etkinlikleri Listele", Location = new Point(margin + 320, 145), Size = new Size(120, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            dgvEgitimler = new DataGridView()
            {
                Location = new Point(margin, 270),
                Size = new Size(contentPanel.Width - 2 * margin, contentPanel.Height - 280),
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };

            contentPanel.Controls.AddRange(new Control[] {
                lblBaslik, txtBaslik,
                lblEgitmen, txtEgitmen,
                lblTarih, dtTarih,
                lblKatilim, clbKatilimcilar,
                btnEkle,btnGüncelle,btnSil, btnListele,
                dgvEgitimler
            });

            // Buton eventlerini bağla
            btnEkle.Click += BtnEkle_Click;
            btnListele.Click += BtnListele_Click;
            btnSil.Click += BtnSil_Click;
            btnGüncelle.Click += BtnGuncelle_Click;

            this.Resize += (s, e) =>
            {
                dgvEgitimler.Size = new Size(contentPanel.Width - 2 * margin, contentPanel.Height - dgvEgitimler.Location.Y - margin);
            };
        }

        private void BtnSil_Click1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void EgitimOrganizasyonForm_Load(object sender, EventArgs e)
        {
            dgvEgitimler.SelectionChanged += DgvEgitimler_SelectionChanged;
            LoadPersonelList();
        }

        private void LoadPersonelList()
        {
            try
            {
                clbKatilimcilar.Items.Clear();

                using (SQLiteConnection conn= new SQLiteConnection(connectionString))
                {
                    string sql = "SELECT ID, Ad || ' ' || Soyad AS AdSoyad FROM Personel;\r\n";
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    conn.Open();
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string adSoyad = reader["AdSoyad"].ToString();
                        clbKatilimcilar.Items.Add(adSoyad);
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Katılımcılar yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            string baslik = txtBaslik.Text.Trim();
            string egitmen = txtEgitmen.Text.Trim();
            DateTime tarih = dtTarih.Value;

            if (string.IsNullOrEmpty(baslik) || string.IsNullOrEmpty(egitmen))
            {
                MessageBox.Show("Lütfen Başlık ve Eğitmen alanlarını doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (clbKatilimcilar.CheckedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir katılımcı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string katilimcilar = string.Join(", ", clbKatilimcilar.CheckedItems.Cast<string>());

            try
            {
                using (SQLiteConnection conn= new SQLiteConnection(connectionString))
                {
                    string insertQuery = "INSERT INTO EgitimOrganizasyon (Baslik, Egitmen, Tarih, Katilimcilar) VALUES (@Baslik, @Egitmen, @Tarih, @Katilimcilar)";
                    SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@Baslik", baslik);
                    cmd.Parameters.AddWithValue("@Egitmen", egitmen);
                    cmd.Parameters.AddWithValue("@Tarih", tarih);
                    cmd.Parameters.AddWithValue("@Katilimcilar", katilimcilar);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (rows > 0)
                    {
                        MessageBox.Show("Eğitim başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Temizle();
                    }
                    else
                    {
                        MessageBox.Show("Eğitim eklenirken sorun oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message);
            }
        }
        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgvEgitimler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silinecek bir kayıt seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int egitimId = Convert.ToInt32(dgvEgitimler.SelectedRows[0].Cells["Id"].Value);

            DialogResult result = MessageBox.Show("Bu kaydı silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    string deleteQuery = "DELETE FROM EgitimOrganizasyon WHERE Id = @Id";
                    SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);
                    cmd.Parameters.AddWithValue("@Id", egitimId);

                    conn.Open();
                    int affectedRows = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (affectedRows > 0)
                    {
                        MessageBox.Show("Eğitim başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadEgitimler();
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme hatası: " + ex.Message);
            }
        }
        private void LoadEgitimler()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    string selectQuery = "SELECT Id, Baslik, Egitmen, Tarih, Katilimcilar FROM EgitimOrganizasyon ORDER BY Tarih DESC";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(selectQuery, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvEgitimler.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler listelenirken hata oluştu: " + ex.Message);
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvEgitimler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellenecek bir kayıt seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int egitimId = Convert.ToInt32(dgvEgitimler.SelectedRows[0].Cells["Id"].Value);
            string baslik = txtBaslik.Text.Trim();
            string egitmen = txtEgitmen.Text.Trim();
            DateTime tarih = dtTarih.Value;
            string katilimcilar = string.Join(", ", clbKatilimcilar.CheckedItems.Cast<string>());

            if (string.IsNullOrEmpty(baslik) || string.IsNullOrEmpty(egitmen))
            {
                MessageBox.Show("Başlık ve Eğitmen alanları boş bırakılamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    string updateQuery = @"
                UPDATE EgitimOrganizasyon
                SET Baslik = @Baslik, Egitmen = @Egitmen, Tarih = @Tarih, Katilimcilar = @Katilimcilar
                WHERE Id = @Id";

                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);
                    cmd.Parameters.AddWithValue("@Baslik", baslik);
                    cmd.Parameters.AddWithValue("@Egitmen", egitmen);
                    cmd.Parameters.AddWithValue("@Tarih", tarih);
                    cmd.Parameters.AddWithValue("@Katilimcilar", katilimcilar);
                    cmd.Parameters.AddWithValue("@Id", egitimId);

                    conn.Open();
                    int affectedRows = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (affectedRows > 0)
                    {
                        MessageBox.Show("Eğitim başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadEgitimler();
                    }
                    else
                    {
                        MessageBox.Show("Güncelleme başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme hatası: " + ex.Message);
            }
        }


        private void BtnListele_Click(object sender, EventArgs e)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    string selectQuery = "SELECT Id, Baslik, Egitmen, Tarih, Katilimcilar FROM EgitimOrganizasyon ORDER BY Tarih DESC";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(selectQuery, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvEgitimler.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler listelenirken hata oluştu: " + ex.Message);
            }
        }

        private void DgvEgitimler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEgitimler.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvEgitimler.SelectedRows[0];

            int egitimId = Convert.ToInt32(row.Cells["Id"].Value);
            string baslik = row.Cells["Baslik"].Value.ToString();
            string egitmen = row.Cells["Egitmen"].Value.ToString();
            DateTime tarih = Convert.ToDateTime(row.Cells["Tarih"].Value);
            string katilimcilar = row.Cells["Katilimcilar"].Value?.ToString();

            txtBaslik.Text = baslik;
            txtEgitmen.Text = egitmen;
            dtTarih.Value = tarih;

            // ✅ Katılımcıları işaretle
            foreach (int i in Enumerable.Range(0, clbKatilimcilar.Items.Count))
            {
                string itemText = clbKatilimcilar.Items[i].ToString();
                clbKatilimcilar.SetItemChecked(i, katilimcilar?.Split(',').Select(x => x.Trim()).Contains(itemText) ?? false);
            }
        }


        private void InitializeDatabase()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS EgitimOrganizasyon (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Baslik TEXT NOT NULL,
                    Egitmen TEXT NOT NULL,
                    Tarih DATE NOT NULL,
                    Katilimcilar TEXT NOT NULL
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Temizle()
        {
            txtBaslik.Clear();
            txtEgitmen.Clear();
            dtTarih.Value = DateTime.Now;
            for (int i = 0; i < clbKatilimcilar.Items.Count; i++)
            {
                clbKatilimcilar.SetItemChecked(i, false);
            }
        }
    }
}
