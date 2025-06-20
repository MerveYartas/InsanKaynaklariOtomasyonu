using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InsanKaynaklariOtomasyonu
{
    public partial class IzinForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private DataGridView dgvIzin;
        private TextBox txtPersonelId, txtIzinTipi, txtAciklama, txtAra;
        private DateTimePicker dtpBaslangic, dtpBitis;
        private Button btnEkle, btnSil, btnGuncelle, btnAra;

        private string dbPath;
        private string connectionString;

        public IzinForm() // yapıcı metot
        {
            ConfigureDatabaseConnection();// önce connectionString'i ayarla
            InitializeDatabase(); // veritabanını başlat, tabloları oluştur
            SetupHeader(); // başlık panelini ayarla
            SetupControls();  // burada LoadComboBoxes() çağrılır, connectionString hazır olur
            LoadData(); // verileri yükle
            this.FormBorderStyle = FormBorderStyle.None; // kenarlıkları kaldır
            this.StartPosition = FormStartPosition.CenterScreen; // formu ortala
            this.Size = new Size(900, 600); // form boyutunu ayarla
            this.BackColor = Color.FromArgb(240, 238, 233); // arka plan rengini ayarla
        }

        private void ConfigureDatabaseConnection() // veritabanı bağlantısını ayarla
        {
            string appDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data"); // uygulama verileri klasörü
            dbPath = Path.Combine(appDataFolder, "ikdb.sqlite"); // klasör ve veritabanı dosyası
            connectionString = $"Data Source={dbPath};Version=3;"; // SQLite bağlantı dizesi
        }
        private void LoadData(string filter = "") // verileri yükle, opsiyonel filtre ile
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString)) // SQLite bağlantısı oluştur
            {
                con.Open(); // bağlantıyı aç
                SQLiteDataAdapter da; // DataAdapter oluştur

                if (string.IsNullOrWhiteSpace(filter)) // eğer filtre boşsa
                {
                    da = new SQLiteDataAdapter(@" 
                    SELECT I.IzinID, P.ID AS PersonelID, P.Ad || ' ' || P.Soyad AS Personel, 
                    IT.IzinTuruID, IT.IzinTuruAdi, 
                    I.BaslangicTarihi, I.BitisTarihi, I.Aciklama
                    FROM Izinler I
                    INNER JOIN Personel P ON I.PersonelID = P.ID
                    INNER JOIN IzinTurleri IT ON I.IzinTuruID = IT.IzinTuruID

                ", con);
                }
                else
                {
                    da = new SQLiteDataAdapter(@"
                    SELECT I.IzinID, P.Id AS PersonelID, P.Ad || ' ' || P.Soyad AS Personel, 
                    IT.IzinTuruID, IT.IzinTuruAdi, I.BaslangicTarihi, I.BitisTarihi, I.Aciklama
                    FROM Izinler I
                    INNER JOIN Personel P ON I.PersonelID = P.Id
                    INNER JOIN IzinTurleri IT ON I.IzinTuruID = IT.IzinTuruID
                    WHERE P.Ad || ' ' || P.Soyad LIKE @filter 
                    OR IT.IzinTuruAdi LIKE @filter
                    ", con);
                    da.SelectCommand.Parameters.AddWithValue("@filter", "%" + filter + "%");
                }

                DataTable dt = new DataTable(); // DataTable oluştur
                da.Fill(dt); // DataAdapter ile verileri doldur
                dgvIzin.DataSource = dt; // DataGridView'e verileri ata

                // ID kolonu gizlenebilir, kullanıcıya gerek yok
                if (dgvIzin.Columns["IzinID"] != null) 
                    dgvIzin.Columns["IzinID"].Visible = false;
            }
        }


        private void SetupHeader() // başlık panelini ayarla
        {
            Panel headerPanel = new Panel() // başlık paneli
            {
                Height = 45, 
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(224, 219, 211)
            };
            headerPanel.MouseDown += HeaderPanel_MouseDown; // başlık paneline fare tıklama olayını ekle

            Label titleLabel = new Label() // başlık etiketi
            {
                Text = "İzin Yönetimi",
                ForeColor = Color.FromArgb(85, 80, 70),
                Font = new Font("Segoe UI Semilight", 16),
                AutoSize = true,
                Location = new Point(20, 7)
            };

            FlowLayoutPanel rightButtonsPanel = new FlowLayoutPanel() // sağdaki butonlar paneli
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Right,
                Width = 100,
                Padding = new Padding(0, 7, 0, 0)
            };

            Button btnClose = new Button() // kapatma butonu
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

            Button btnBack = new Button() // geri butonu
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

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e) // başlık paneline fare tıklama olayını ekle
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private ComboBox cmbPersonel;
        private ComboBox cmbIzinTipi;

        private void SetupControls() // kontrolleri ayarla
        {
            // DataGridView
            dgvIzin = new DataGridView()
            {
                Location = new Point(20, 60),
                Size = new Size(840, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvIzin.CellClick += DgvIzin_CellClick;

            // Labels
            Label lblPersonelId = new Label() { Text = "Personel:", Location = new Point(20, 370), AutoSize = true };
            cmbPersonel = new ComboBox() { Location = new Point(20, 390), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblIzinBaslangic = new Label() { Text = "İzin Başlangıç Tarihi:", Location = new Point(220, 370), AutoSize = true };
            dtpBaslangic = new DateTimePicker() { Location = new Point(220, 390), Width = 180, Format = DateTimePickerFormat.Short };

            Label lblIzinBitis = new Label() { Text = "İzin Bitiş Tarihi:", Location = new Point(420, 370), AutoSize = true };
            dtpBitis = new DateTimePicker() { Location = new Point(420, 390), Width = 180, Format = DateTimePickerFormat.Short };

            Label lblIzinTipi = new Label() { Text = "İzin Tipi:", Location = new Point(20, 430), AutoSize = true };
            cmbIzinTipi = new ComboBox() { Location = new Point(20, 450), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblAciklama = new Label() { Text = "Açıklama:", Location = new Point(220, 430), AutoSize = true };
            txtAciklama = new TextBox() { Location = new Point(220, 450), Width = 380 };

            Label lblAra = new Label() { Text = "Ara (Personel veya İzin Tipi):", Location = new Point(20, 490), AutoSize = true };
            txtAra = new TextBox() { Location = new Point(20, 510), Width = 180 };

            // Buttons
            btnEkle = new Button() { Text = "Ekle", Location = new Point(620, 390), Width = 100 };
            btnSil = new Button() { Text = "Sil", Location = new Point(730, 390), Width = 100 };
            btnGuncelle = new Button() { Text = "Güncelle", Location = new Point(620, 430), Width = 100 };
            btnAra = new Button() { Text = "Ara", Location = new Point(220, 510), Width = 100 };

            btnEkle.Click += BtnEkle_Click;
            btnSil.Click += BtnSil_Click;
            btnGuncelle.Click += BtnGuncelle_Click;
            btnAra.Click += BtnAra_Click;

            this.Controls.AddRange(new Control[]
            {
                dgvIzin,
                lblPersonelId, cmbPersonel,
                lblIzinBaslangic, dtpBaslangic,
                lblIzinBitis, dtpBitis,
                lblIzinTipi, cmbIzinTipi,
                lblAciklama, txtAciklama,
                lblAra, txtAra,
                btnEkle, btnSil, btnGuncelle, btnAra
            });

            LoadComboBoxes(); // combo boxları doldur
        }
        private void LoadComboBoxes() // combo boxları doldur
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();

                // Personel ComboBox
                SQLiteDataAdapter daPersonel = new SQLiteDataAdapter("SELECT Id, Ad || ' ' || Soyad AS AdSoyad FROM Personel;\r\n", con);
                DataTable dtPersonel = new DataTable();
                daPersonel.Fill(dtPersonel);

                cmbPersonel.DataSource = dtPersonel;
                cmbPersonel.DisplayMember = "AdSoyad";    // Kullanıcıya gösterilecek alan
                cmbPersonel.ValueMember = "Id";   // Gerçek değeri (ID)
                cmbPersonel.SelectedIndex = -1;

                // İzin Tipi ComboBox
                SQLiteDataAdapter daIzinTipi = new SQLiteDataAdapter("SELECT IzinTuruID, IzinTuruAdi FROM IzinTurleri", con);
                DataTable dtIzinTipi = new DataTable();
                daIzinTipi.Fill(dtIzinTipi);

                if (dtIzinTipi.Rows.Count == 0) // Eğer izin türleri yoksa, önce ekleyelim
                {
                    InsertDefaultIzinTurleri();
                    daIzinTipi.Fill(dtIzinTipi); // Yeniden yükle
                }

                cmbIzinTipi.DataSource = dtIzinTipi;
                cmbIzinTipi.DisplayMember = "IzinTuruAdi";
                cmbIzinTipi.ValueMember = "IzinTuruID";
                cmbIzinTipi.SelectedIndex = -1;
            }
        }


        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (cmbPersonel.SelectedIndex == -1 || cmbIzinTipi.SelectedIndex == -1)
            {
                MessageBox.Show("Personel ve izin tipi seçiniz!");
                return;
            }

            if (cmbPersonel.SelectedValue == null || cmbIzinTipi.SelectedValue == null)
            {
                MessageBox.Show("Lütfen geçerli bir personel ve izin tipi seçin.");
                return;
            }

            int personelId = Convert.ToInt32(cmbPersonel.SelectedValue);
            int izinTuruId = Convert.ToInt32(cmbIzinTipi.SelectedValue);


            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                    @"INSERT INTO Izinler (PersonelID, IzinTuruID, BaslangicTarihi, BitisTarihi, Aciklama) 
              VALUES (@PersonelID, @IzinTuruID, @BaslangicTarihi, @BitisTarihi, @Aciklama)", con);

                cmd.Parameters.AddWithValue("@PersonelID", personelId);
                cmd.Parameters.AddWithValue("@IzinTuruID", izinTuruId);
                cmd.Parameters.AddWithValue("@BaslangicTarihi", dtpBaslangic.Value.Date);
                cmd.Parameters.AddWithValue("@BitisTarihi", dtpBitis.Value.Date);
                cmd.Parameters.AddWithValue("@Aciklama", txtAciklama.Text.Trim());

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("İzin başarıyla eklendi!");
            LoadData();
            Temizle();
        }


        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgvIzin.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silmek için bir kayıt seçiniz.");
                return;
            }

            int izinId = Convert.ToInt32(dgvIzin.SelectedRows[0].Cells["IzinID"].Value);

            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Izinler WHERE IzinID = @id", con);
                cmd.Parameters.AddWithValue("@id", izinId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("İzin silindi.");
            LoadData();
            Temizle();
        }
        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvIzin.SelectedRows.Count == 0)
            {
                MessageBox.Show("Güncellemek için bir kayıt seçin.");
                return;
            }

            if (cmbPersonel.SelectedValue == null || cmbIzinTipi.SelectedValue == null)
            {
                MessageBox.Show("Lütfen geçerli bir personel ve izin tipi seçin.");
                return;
            }

            int izinId = Convert.ToInt32(dgvIzin.SelectedRows[0].Cells["IzinID"].Value);
            int personelId = Convert.ToInt32(cmbPersonel.SelectedValue);
            int izinTuruId = Convert.ToInt32(cmbIzinTipi.SelectedValue);


            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                  @"UPDATE Izinler SET PersonelID=@PersonelID, IzinTuruID=@IzinTuruID, 
                  BaslangicTarihi=@BaslangicTarihi, BitisTarihi=@BitisTarihi, Aciklama=@Aciklama 
                  WHERE IzinID=@IzinID", con);

                cmd.Parameters.AddWithValue("@PersonelID", personelId);
                cmd.Parameters.AddWithValue("@IzinTuruID", izinTuruId);
                cmd.Parameters.AddWithValue("@BaslangicTarihi", dtpBaslangic.Value.Date);
                cmd.Parameters.AddWithValue("@BitisTarihi", dtpBitis.Value.Date);
                cmd.Parameters.AddWithValue("@Aciklama", txtAciklama.Text.Trim());
                cmd.Parameters.AddWithValue("@IzinID", izinId);


                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Kayıt güncellendi.");
            LoadData();
            Temizle();
        }


        private void BtnAra_Click(object sender, EventArgs e)
        {
            string filtre = txtAra.Text.Trim();
            LoadData(filtre);
        }
        private void DgvIzin_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvIzin.Rows[e.RowIndex];

            int izinId = Convert.ToInt32(row.Cells["IzinID"].Value);
            int personelId = Convert.ToInt32(row.Cells["PersonelID"].Value);
            int izinTuruId = Convert.ToInt32(row.Cells["IzinTuruID"].Value);
            DateTime baslangic = Convert.ToDateTime(row.Cells["BaslangicTarihi"].Value);
            DateTime bitis = Convert.ToDateTime(row.Cells["BitisTarihi"].Value);
            string aciklama = row.Cells["Aciklama"].Value?.ToString();

            cmbPersonel.SelectedValue = personelId;
            cmbIzinTipi.SelectedValue = izinTuruId;
            dtpBaslangic.Value = baslangic;
            dtpBitis.Value = bitis;
            txtAciklama.Text = aciklama;
        }







        private void InitializeDatabase()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS Izinler (
                    IzinID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PersonelID INTEGER NOT NULL,
                    IzinTuruID INTEGER NOT NULL,
                    BaslangicTarihi DATE NOT NULL,
                    BitisTarihi DATE NOT NULL,
                    Aciklama TEXT
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS IzinTurleri (
                    IzinTuruID INTEGER PRIMARY KEY AUTOINCREMENT,
                    IzinTuruAdi TEXT NOT NULL
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InsertDefaultIzinTurleri()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                    INSERT INTO IzinTurleri (IzinTuruAdi) VALUES
                    ('Doğum İzni'),
                    ('Yıllık İzin'),
                    ('Mazeret İzni'),
                    ('Hastalık İzni'),
                    ('Ücretsiz İzin'),
                    ('Eğitim İzni'),
                    ('Evlenme İzni'),
                    ('Babalık İzni');";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void Temizle()
        {
            cmbPersonel.SelectedIndex = -1;
            cmbIzinTipi.SelectedIndex = -1;
            dtpBaslangic.Value = DateTime.Today;
            dtpBitis.Value = DateTime.Today;
            txtAciklama.Clear();
            txtAra.Clear();
            dgvIzin.ClearSelection();
        }


    }
}
