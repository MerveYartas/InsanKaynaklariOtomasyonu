using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace InsanKaynaklariOtomasyonu
{
    public partial class MainForm : Form
    {
        // Form sürükleme API
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public MainForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(920, 580);
            this.BackColor = Color.FromArgb(240, 238, 233); // Soft krem-bej

            SetupHeader();
            SetupMainPanel();
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
                Text = "İnsan Kaynakları Yönetim Sistemi",
                ForeColor = Color.FromArgb(85, 80, 70),
                Font = new Font("Segoe UI Semilight", 16, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(20, 7)
            };

            // Sağ üst butonlar paneli
            FlowLayoutPanel rightButtonsPanel = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.RightToLeft,  // Sağdan sola sıralama
                Dock = DockStyle.Right,
                Width = 180,  // Yeterince geniş
                Padding = new Padding(0, 7, 0, 0),
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoSize = false
            };

            // ✕ Kapat butonu
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

            // Oturumu Kapat butonu
            Button btnLogout = new Button()
            {
                Text = "Oturumu Kapat",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(224, 219, 211),
                ForeColor = Color.FromArgb(130, 120, 110),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) =>
            {
                LoginForm loginForm = new LoginForm();
                this.Hide();
                loginForm.Show();
            };
            btnLogout.MouseEnter += (s, e) => btnLogout.ForeColor = Color.FromArgb(80, 100, 180);
            btnLogout.MouseLeave += (s, e) => btnLogout.ForeColor = Color.FromArgb(130, 120, 110);

            rightButtonsPanel.Controls.Add(btnClose);
            rightButtonsPanel.Controls.Add(btnLogout);

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

        private void SetupMainPanel()
        {
            Panel mainPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 238, 233),
                Padding = new Padding(40, 60, 40, 40),
            };

            TableLayoutPanel table = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.Transparent,
                AutoSize = true,
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            table.Controls.Add(CreateModuleButton("Personel Yönetimi", BtnPersonelYonetimi_Click), 0, 0);
            table.Controls.Add(CreateModuleButton("Aday Takibi", BtnAdayTakibi_Click), 1, 0);
            table.Controls.Add(CreateModuleButton("İzin Modülü", BtnIzinModulu_Click), 2, 0);

            table.Controls.Add(CreateModuleButton("Performans", BtnPerformans_Click), 0, 1);
            table.Controls.Add(CreateModuleButton("Eğitim / Organizasyon", BtnEgitimOrganizasyon_Click), 1, 1);
            table.Controls.Add(CreateModuleButton("Maaş & Puantaj", BtnMaasPuantaj_Click), 2, 1);

            table.Controls.Add(CreateModuleButton("Raporlar", BtnRaporlar_Click), 1, 2);

            mainPanel.Controls.Add(table);
            this.Controls.Add(mainPanel);
        }

        private Button CreateModuleButton(string text, EventHandler onClick)
        {
            Button btn = new Button()
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(210, 205, 198), // Mat açık kahve-gri
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 80, 70), // Koyu kahve
                Size = new Size(250, 100),
                Margin = new Padding(15),
                Cursor = Cursors.Hand,
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(195, 190, 185); // Hafif koyu tonda hover

            btn.Click += onClick;

            btn.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 15, 15));

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(195, 190, 185);
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(150, 140, 130);
                btn.Font = new Font(btn.Font, FontStyle.Bold);
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(210, 205, 198);
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font(btn.Font, FontStyle.Regular);
            };

            return btn;
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );
        private void BtnPersonelYonetimi_Click(object sender, EventArgs e)
        {
            PersonelYonetimiForm personelYonetimForm = new PersonelYonetimiForm();
            this.Hide();
            personelYonetimForm.FormClosed += (s, args) => this.Close();
            personelYonetimForm.Show();
        }

        private void BtnAdayTakibi_Click(object sender, EventArgs e)
        {
           
                AdayForm adayForm = new AdayForm();
                this.Hide();
                adayForm.FormClosed += (s, args) => this.Close();
                adayForm.Show();
           

        }
        private void BtnIzinModulu_Click(object sender, EventArgs e)
        {
            IzinForm izinForm = new IzinForm();
            this.Hide();
            izinForm.FormClosed += (s, args) => this.Close();
            izinForm.Show();
        }
        private void BtnPerformans_Click(object sender, EventArgs e)
        {
            PerformansForm performansForm = new PerformansForm();
            this.Hide();
            performansForm.FormClosed += (s, args) => this.Close();
            performansForm.Show();
        }
        private void BtnEgitimOrganizasyon_Click(object sender, EventArgs e)
        {
            EgitimOrganizasyonForm egitimOrganizasyonForm = new EgitimOrganizasyonForm();
            this.Hide();
            egitimOrganizasyonForm.FormClosed += (s, args) => this.Close();
            egitimOrganizasyonForm.Show();
        }
        private void BtnMaasPuantaj_Click(object sender, EventArgs e)
        {
            MaasPuantajForm maasPuantajForm = new MaasPuantajForm();
            this.Hide();
            maasPuantajForm.FormClosed += (s, args) => this.Close();
            maasPuantajForm.Show();
        }
        private void BtnRaporlar_Click(object sender, EventArgs e)
        {
            RaporForm raporForm = new RaporForm();
            this.Hide();
            raporForm.FormClosed += (s, args) => this.Close();
            raporForm.Show();
        }

    }
}