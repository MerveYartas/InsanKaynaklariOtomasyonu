using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace InsanKaynaklariOtomasyonu
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblClose;
        private LinkLabel linkToRegister;

        private string dbPath;
        private string connectionString;

        public LoginForm()
        {
            ConfigureDatabaseConnection();
            InitializeDatabase();
            InitializeComponents();
        }

        private void ConfigureDatabaseConnection()
        {
            string appDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
            dbPath = Path.Combine(appDataFolder, "ikdb.sqlite");
            connectionString = $"Data Source={dbPath};Version=3;";
        }


        private void InitializeComponents()
        {
            this.Text = "Giriş";
            this.Size = new Size(350, 230);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 243, 247);

            lblClose = new Label()
            {
                Text = "×",
                ForeColor = Color.FromArgb(80, 100, 180),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand,
                AutoSize = true,
                Location = new Point(this.Width - 30, 5),
            };
            lblClose.Click += (s, e) => Application.Exit();
            lblClose.MouseEnter += (s, e) => lblClose.ForeColor = Color.Red;
            lblClose.MouseLeave += (s, e) => lblClose.ForeColor = Color.FromArgb(80, 100, 180);
            this.Controls.Add(lblClose);

            lblUsername = new Label()
            {
                Text = "Kullanıcı Adı:",
                Location = new Point(30, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 50, 50)
            };
            this.Controls.Add(lblUsername);

            txtUsername = new TextBox()
            {
                Location = new Point(130, 35),
                Size = new Size(170, 28),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtUsername);

            lblPassword = new Label()
            {
                Text = "Şifre:",
                Location = new Point(30, 85),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 50, 50)
            };
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox()
            {
                Location = new Point(130, 80),
                Size = new Size(170, 28),
                UseSystemPasswordChar = true,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtPassword);

            btnLogin = new Button()
            {
                Text = "Giriş Yap",
                Location = new Point(130, 130),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(60, 80, 150);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(80, 100, 180);

            this.Controls.Add(btnLogin);

            this.MouseDown += LoginForm_MouseDown;

            linkToRegister = new LinkLabel()
            {
                Text = "Hesabınız yok mu? Kaydolun",
                Location = new Point(90, 180),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                LinkColor = Color.FromArgb(80, 100, 180),
                ActiveLinkColor = Color.Red,
                Cursor = Cursors.Hand
            };
            linkToRegister.Click += LinkToRegister_Click;
            this.Controls.Add(linkToRegister);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Lütfen kullanıcı adı ve şifre girin.");
                    return;
                }

                using (SQLiteConnection con = new SQLiteConnection(connectionString))
                {
                    con.Open();

                    // Şifreyi veritabanından alıyoruz
                    string query = "SELECT Sifre FROM Kullanici WHERE KullaniciAdi = @kadi";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@kadi", username);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string hashedPasswordFromDB = result.ToString();

                            // Girilen şifreyi hashleyip veritabanındakiyle karşılaştırıyoruz
                            if (hashedPasswordFromDB == HashPassword(password))
                            {
                                MessageBox.Show("Giriş başarılı!");
                                this.Hide();
                                MainForm main = new MainForm();
                                main.Show();
                            }
                            else
                            {
                                MessageBox.Show("Kullanıcı adı veya şifre hatalı!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Kullanıcı adı veya şifre hatalı!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata ayrıntıları:\n{ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void LoginForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void LinkToRegister_Click(object sender, EventArgs e)
        {
            KullaniciKayitForm registerForm = new KullaniciKayitForm();
            registerForm.Show();
            this.Hide();
        }

        private void InitializeDatabase()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS Kullanici (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                KullaniciAdi TEXT NOT NULL,
                Sifre TEXT NOT NULL
                );";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
