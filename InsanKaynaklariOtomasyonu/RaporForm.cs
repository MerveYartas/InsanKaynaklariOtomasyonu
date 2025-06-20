using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace InsanKaynaklariOtomasyonu
{
    public class RaporForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private ComboBox cmbRaporTipi;
        private Button btnRaporOlustur;
        private Label label1;
        private Button btnClose;
        private Button btnBack;

        private string dbPath;
        private string connectionString;

        public RaporForm()
        {
            InitializeComponent();
            ConfigureDatabaseConnection();
        }

        private void ConfigureDatabaseConnection() // veritabanı bağlantısını ayarla
        {
            string appDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data"); // uygulama verileri klasörü
            dbPath = Path.Combine(appDataFolder, "ikdb.sqlite"); // klasör ve veritabanı dosyası
            connectionString = $"Data Source={dbPath};Version=3;"; // SQLite bağlantı dizesi
        }

        private void InitializeComponent()
        {
            this.Text = "Rapor Oluştur";
            this.Size = new Size(500, 250);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel header = new Panel()
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.LightGray
            };
         

            Label title = new Label()
            {
                Text = "Rapor Oluştur",
                Font = new Font("Segoe UI Semilight", 14, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.Black,
                Location = new Point(10, 8)
            };

            btnClose = new Button()
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(40, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightGray,
                ForeColor = Color.Black
            };

            btnBack = new Button()
            {
                Text = "←",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(40, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightGray,
                ForeColor = Color.Black
            };

            btnClose.FlatAppearance.BorderSize = 0;
            btnBack.FlatAppearance.BorderSize = 0;

            btnClose.Click += (s, e) => Application.Exit(); // Formu kapatma butonunun davranışı
            btnBack.Click += (s, e) => { this.Hide(); new MainForm().Show(); }; // Geri butonunun davranışı (istenirse farklı form açılabilir)

            // Başta konumlandırma
            btnClose.Location = new Point(this.Width - btnClose.Width - 10, 5);
            btnBack.Location = new Point(this.Width - btnClose.Width - btnBack.Width - 20, 5);

            // Form yeniden boyutlandığında yeniden konumlandırma
            this.Resize += (s, e) =>
            {
                btnClose.Location = new Point(this.Width - btnClose.Width - 10, 5);
                btnBack.Location = new Point(this.Width - btnClose.Width - btnBack.Width - 20, 5);
            };

            header.Controls.Add(title);
            header.Controls.Add(btnClose);
            header.Controls.Add(btnBack);
            this.Controls.Add(header);

            label1 = new Label()
            {
                Text = "Rapor Tipi Seçin:",
                Location = new Point(30, 70),
                AutoSize = true
            };

            cmbRaporTipi = new ComboBox()
            {
                Location = new Point(150, 65),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRaporTipi.Items.AddRange(new string[] { "Personel", "İzin", "Bordro" });

            btnRaporOlustur = new Button()
            {
                Text = "Raporu İndir",
                Location = new Point(150, 110),
                Width = 250
            };
            btnRaporOlustur.Click += BtnRaporOlustur_Click;

            this.Controls.Add(label1);
            this.Controls.Add(cmbRaporTipi);
            this.Controls.Add(btnRaporOlustur);
        }

        private void BtnRaporOlustur_Click(object sender, EventArgs e)
        {
            if (cmbRaporTipi.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir rapor tipi seçiniz.");
                return;
            }

            string tip = cmbRaporTipi.SelectedItem.ToString().ToLower();
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Metin Dosyası|*.txt",
                Title = "Rapor Kaydet",
                FileName = $"Rapor_{tip}.txt"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string path = saveDialog.FileName;
                OlusturRapor(tip, path);
            }
        }

        private void OlusturRapor(string raporTipi, string dosyaYolu)
        {
            switch (raporTipi)
            {
                case "personel":
                    PerformansRaporuOlustur(dosyaYolu);
                    break;
                case "izin":
                    IzinGecmisiRaporuOlustur(dosyaYolu);
                    break;
                case "bordro":
                    BordroRaporuOlustur(dosyaYolu);
                    break;
                default:
                    MessageBox.Show("Geçersiz rapor tipi!");
                    break;
            }
        }

        private void PerformansRaporuOlustur(string dosyaYolu)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== PERSONEL RAPORU ===");
            sb.AppendLine("Ad Soyad".PadRight(30) + "Görev");
            sb.AppendLine(new string('-', 50));

            using (var con = new SQLiteConnection(connectionString))
            {
                string sql = "SELECT  Ad || ' ' || Soyad AS AdSoyad , Gorev FROM Personel";
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    try
                    {
                        con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string adSoyad = reader["AdSoyad"].ToString();
                                string gorev = reader["Gorev"].ToString();
                                sb.AppendLine(adSoyad.PadRight(30) + gorev);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Personel raporu hatası: " + ex.Message);
                        return;
                    }
                }
            }

            File.WriteAllText(dosyaYolu, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Rapor oluşturuldu: " + dosyaYolu);
        }

        private void IzinGecmisiRaporuOlustur(string dosyaYolu)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== İZİN GEÇMİŞİ RAPORU ===");
            sb.AppendLine("Ad Soyad".PadRight(30) + "İzin Türü".PadRight(20) + "Başlangıç Tarihi".PadRight(20) + "Bitiş Tarihi".PadRight(20) + "Açıklama");
            sb.AppendLine(new string('-', 100));

            using (var con = new SQLiteConnection(connectionString))
            {
                string sql = @"
        SELECT p.Ad || ' ' || p.Soyad AS AdSoyad, 
               it.IzinTuruAdi, i.BaslangicTarihi, i.BitisTarihi, COALESCE(i.Aciklama, 'Yok') AS Aciklama
        FROM Izinler i
        INNER JOIN Personel p ON i.PersonelID = p.Id
        INNER JOIN IzinTurleri it ON i.IzinTuruID = it.IzinTuruID
        ORDER BY i.BaslangicTarihi DESC;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    try
                    {
                        con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string adSoyad = reader["AdSoyad"].ToString();
                                string izinTuru = reader["IzinTuruAdi"].ToString();
                                string baslangicTarihi = Convert.ToDateTime(reader["BaslangicTarihi"]).ToString("dd.MM.yyyy");
                                string bitisTarihi = Convert.ToDateTime(reader["BitisTarihi"]).ToString("dd.MM.yyyy");
                                string aciklama = reader["Aciklama"].ToString();

                                sb.AppendLine(adSoyad.PadRight(30) + izinTuru.PadRight(20) + baslangicTarihi.PadRight(20) + bitisTarihi.PadRight(20) + aciklama);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("İzin raporu hatası: " + ex.Message);
                        return;
                    }
                }
            }

            File.WriteAllText(dosyaYolu, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("İzin geçmişi raporu oluşturuldu: " + dosyaYolu);
        }


        private void BordroRaporuOlustur(string dosyaYolu)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== BORDRO RAPORU ===");
            sb.AppendLine("Ad Soyad".PadRight(30) + "Net Maaş".PadRight(15) + "Kesinti".PadRight(15) + "Fazla Mesai Saat".PadRight(15) + "Tarih");
            sb.AppendLine(new string('-', 80));

            using (var con = new SQLiteConnection(connectionString))
            {
                string sql = @"
        SELECT p.Ad || ' ' || p.Soyad AS AdSoyad, 
               e.NetMaas, e.Kesinti, e.FazlaMesaiSaat, e.Tarih
        FROM MaasPuantaj e
        INNER JOIN Personel p ON e.PersonelID = p.Id
        ORDER BY e.Tarih DESC;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    try
                    {
                        con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string adSoyad = reader["AdSoyad"].ToString();
                                string netMaas = Convert.ToDecimal(reader["NetMaas"]).ToString("N2");
                                string kesinti = Convert.ToDecimal(reader["Kesinti"]).ToString("N2");
                                string mesaiSaat = reader["FazlaMesaiSaat"].ToString();
                                string tarih = Convert.ToDateTime(reader["Tarih"]).ToString("dd.MM.yyyy");

                                sb.AppendLine(adSoyad.PadRight(30) + netMaas.PadRight(15) + kesinti.PadRight(15) + mesaiSaat.PadRight(15) + tarih);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Bordro raporu hatası: " + ex.Message);
                        return;
                    }
                }
            }

            File.WriteAllText(dosyaYolu, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Rapor oluşturuldu: " + dosyaYolu);
        }

    }
}
