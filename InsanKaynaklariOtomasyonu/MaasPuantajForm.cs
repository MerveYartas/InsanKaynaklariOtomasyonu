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
    public partial class MaasPuantajForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private ComboBox cbPersonel;
        private NumericUpDown numBrut, numKesinti, numMesai;
        private Label lblNet;
        private Button btnHesapla, btnPDF;
        private DataGridView dgvMaaslar;

        private string dbPath;
        private string connectionString;

        public MaasPuantajForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.BackColor = Color.FromArgb(240, 238, 233);

            SetupHeader();
            ConfigureDatabaseConnection();
            InitializeDatabase();
            InitializeCustomComponents();
            LoadPersonel();
            LoadMaasPuantaj();
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
                Text = "Maaş ve Puantaj",
                ForeColor = Color.FromArgb(85, 80, 70),
                Font = new Font("Segoe UI Semilight", 16, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(20, 7)
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
                Location = new Point(headerPanel.Width - 50, 7),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
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
                Location = new Point(headerPanel.Width - 100, 7),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => { this.Hide(); new MainForm().Show(); };
            btnBack.MouseEnter += (s, e) => btnBack.ForeColor = Color.FromArgb(80, 100, 180);
            btnBack.MouseLeave += (s, e) => btnBack.ForeColor = Color.FromArgb(130, 120, 110);

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(btnBack);
            headerPanel.Controls.Add(btnClose);
            this.Controls.Add(headerPanel);

            headerPanel.Resize += (s, e) =>
            {
                btnClose.Location = new Point(headerPanel.Width - 50, 7);
                btnBack.Location = new Point(headerPanel.Width - 100, 7);
            };
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
            int margin = 20;
            int controlHeight = 25;
            int labelWidth = 100;
            int inputWidth = 180;
            int btnWidth = 100;
            int startY = 60;
            int gapY = 45;

            Label lblPer = new Label() { Text = "Personel:", Location = new Point(margin, startY), AutoSize = true };
            cbPersonel = new ComboBox()
            {
                Location = new Point(margin + labelWidth, startY - 3),
                Width = inputWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblBrut = new Label() { Text = "Brüt Maaş:", Location = new Point(margin, startY + gapY), AutoSize = true };
            numBrut = new NumericUpDown()
            {
                Location = new Point(margin + labelWidth, startY + gapY - 3),
                Width = inputWidth,
                Maximum = 100000,
                DecimalPlaces = 2,
                ThousandsSeparator = true
            };

            Label lblKesinti = new Label() { Text = "Kesinti:", Location = new Point(margin, startY + gapY * 2), AutoSize = true };
            numKesinti = new NumericUpDown()
            {
                Location = new Point(margin + labelWidth, startY + gapY * 2 - 3),
                Width = inputWidth,
                Maximum = 10000,
                DecimalPlaces = 2,
                ThousandsSeparator = true
            };

            Label lblMesai = new Label() { Text = "Fazla Mesai (saat):", Location = new Point(margin, startY + gapY * 3), AutoSize = true };
            numMesai = new NumericUpDown()
            {
                Location = new Point(margin + labelWidth + 30, startY + gapY * 3 - 3),
                Width = inputWidth - 30,
                Maximum = 200,
                DecimalPlaces = 1
            };

            lblNet = new Label()
            {
                Text = "Net Maaş: 0 TL",
                Location = new Point(margin, startY + gapY * 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };

            btnHesapla = new Button()
            {
                Text = "Hesapla",
                Location = new Point(margin + labelWidth + inputWidth + 30, startY),
                Size = new Size(btnWidth, controlHeight + 5)
            };
            btnHesapla.Click += BtnHesapla_Click;


            dgvMaaslar = new DataGridView()
            {
                Location = new Point(margin, startY + gapY * 5),
                Size = new Size(this.ClientSize.Width - 2 * margin, this.ClientSize.Height - (startY + gapY * 5) - margin),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            this.Controls.AddRange(new Control[]
            {
                lblPer, cbPersonel,
                lblBrut, numBrut,
                lblKesinti, numKesinti,
                lblMesai, numMesai,
                lblNet,
                btnHesapla, 
                dgvMaaslar
            });

            this.Resize += (s, e) =>
            {
                dgvMaaslar.Size = new Size(this.ClientSize.Width - 2 * margin, this.ClientSize.Height - dgvMaaslar.Location.Y - margin);
                btnHesapla.Location = new Point(this.ClientSize.Width - btnHesapla.Width - margin, btnHesapla.Location.Y);
            };
        }

        private void BtnHesapla_Click(object sender, EventArgs e)
        {
            if (cbPersonel.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir personel seçiniz.");
                return;
            }

            decimal brut = numBrut.Value;
            decimal kesinti = numKesinti.Value;
            decimal mesaiSaat = numMesai.Value;
            decimal net = brut - kesinti + mesaiSaat * 20;

            lblNet.Text = $"Net Maaş: {net:C}";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                    "INSERT INTO MaasPuantaj (PersonelID, BrutMaas, Kesinti, FazlaMesaiSaat, Tarih) " +
                    "VALUES (@pid, @brut, @kes, @mesai, @tarih)", conn);

                cmd.Parameters.AddWithValue("@pid", cbPersonel.SelectedValue);
                cmd.Parameters.AddWithValue("@brut", brut);
                cmd.Parameters.AddWithValue("@kes", kesinti);
                cmd.Parameters.AddWithValue("@mesai", mesaiSaat);
                cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                cmd.ExecuteNonQuery();
            }

            LoadMaasPuantaj();
        }


        private void LoadPersonel()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteDataAdapter da = new SQLiteDataAdapter(
                    "SELECT ID, Ad || ' ' || Soyad AS AdSoyad FROM Personel", conn);

                DataTable dt = new DataTable();
                da.Fill(dt);
                cbPersonel.DataSource = dt;
                cbPersonel.DisplayMember = "AdSoyad";
                cbPersonel.ValueMember = "ID";
            }
        }

        private void LoadMaasPuantaj()
        {
           
        
        
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT m.PersonelID, COALESCE(p.Ad || ' ' || p.Soyad, 'Bilinmeyen') AS Personel, 
                       m.BrutMaas, m.Kesinti, m.FazlaMesaiSaat, m.NetMaas, m.Tarih
                FROM MaasPuantaj m
                LEFT JOIN Personel p ON p.ID = m.PersonelID
                ORDER BY m.Tarih DESC;
                ";

                SQLiteDataAdapter da = new SQLiteDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvMaaslar.DataSource = dt;
            }
        }
       
        private void InitializeDatabase()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS MaasPuantaj (
                    MassPuantajID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PersonelID INTEGER NOT NULL,
                    BrutMaas DECIMAL(10,2) NOT NULL,
                    Kesinti DECIMAL(10,2) NOT NULL,
                    FazlaMesaiSaat INTEGER NOT NULL,
                    NetMaas DECIMAL(10,2) AS (BrutMaas - Kesinti + FazlaMesaiSaat * 20) STORED,
                    Tarih DATE NOT NULL,
                    FOREIGN KEY (PersonelID) REFERENCES Personel(Id)
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
