using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InsanKaynaklariOtomasyonu
{
    public partial class KullaniciKayitForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnRegister;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblClose;
        private LinkLabel linkToLogin;

        private string dbPath;
        private string connectionString;

        public KullaniciKayitForm()
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
            this.Text = "Kayıt Ol";
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
            lblClose.Click += (s, e) => this.Close();
            lblClose.MouseEnter += (s, e) => lblClose.ForeColor = Color.Red;
            lblClose.MouseLeave += (s, e) => lblClose.ForeColor = Color.FromArgb(80, 100, 180);
            this.Controls.Add(lblClose);

            lblUsername = new Label()
            {
                Text = "Kullanıcı Adı:",
                Location = new Point(30, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
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
                Font = new Font("Segoe UI", 10),
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

            btnRegister = new Button()
            {
                Text = "Kayıt Ol",
                Location = new Point(130, 130),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = Color.FromArgb(60, 80, 150);
            btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = Color.FromArgb(80, 100, 180);
            this.Controls.Add(btnRegister);

            // LinkLabel: LoginForm'a git
            linkToLogin = new LinkLabel()
            {
                Text = "Zaten hesabınız var mı? Giriş yapın",
                Location = new Point(90, 180),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                LinkColor = Color.FromArgb(80, 100, 180),
                ActiveLinkColor = Color.Red,
                Cursor = Cursors.Hand
            };
            linkToLogin.Click += LinkToLogin_Click;
            this.Controls.Add(linkToLogin);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(connectionString))
                {
                    con.Open();

                    string kontrolQuery = "SELECT COUNT(*) FROM Kullanici WHERE KullaniciAdi = @kadi";
                    using (SQLiteCommand kontrolCmd = new SQLiteCommand(kontrolQuery, con))
                    {
                        kontrolCmd.Parameters.AddWithValue("@kadi", username);
                        object result = kontrolCmd.ExecuteScalar();
                        int exists = result != null ? Convert.ToInt32(result) : 0;

                        if (exists > 0)
                        {
                            MessageBox.Show("Bu kullanıcı adı zaten kullanılıyor.");
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO Kullanici (KullaniciAdi, Sifre) VALUES (@kadi, @sifre)";
                    using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@kadi", username);
                        cmd.Parameters.AddWithValue("@sifre", HashPassword(password));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Kayıt başarılı! Giriş yapabilirsiniz.");
                        MainForm mainForm = new MainForm();
                        mainForm.Show();
                        this.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu:\n{ex.Message}");
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

        private void LinkToLogin_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
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

            //Tablo Varmı Diye Kontrol ettiğim kod parçacığı şuanlık kapalı kalması yeterli
            //using (SQLiteConnection con = new SQLiteConnection(connectionString))
            //{
            //    con.Open();
            //    string checkTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='Kullanici';";
            //    using (SQLiteCommand checkCmd = new SQLiteCommand(checkTableQuery, con))
            //    {
            //        var result = checkCmd.ExecuteScalar();
            //        if (result == null)
            //        {
            //            MessageBox.Show("Tablo oluşturulmamış!");
            //        }
            //        else
            //        {
            //            MessageBox.Show("Tablo mevcut!");
            //        }
            //    }
            //}

        }

    }
}
