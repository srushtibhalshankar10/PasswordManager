using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace PasswordManager
{
    public class LoginForm : Form
    {
        private readonly DatabaseHelper _db = new();
        private readonly bool _firstRun;
        private int _attempts = 0;

        private readonly TextBox txtPassword = new() { UseSystemPasswordChar = true, Width = 280 };
        private readonly TextBox txtConfirm = new() { UseSystemPasswordChar = true, Width = 280 };
        private readonly Label lblConfirm = new() { Text = "Confirm password:", AutoSize = true };
        private readonly Button btnOk = new() { Width = 120 };

        public LoginForm()
        {
            Text = "Password Manager - Login";
            ClientSize = new Size(330, 200);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            _firstRun = !_db.MasterExists();

            var lblPassword = new Label
            {
                Text = _firstRun ? "Create master password:" : "Master password:",
                Location = new Point(25, 20),
                AutoSize = true
            };
            txtPassword.Location = new Point(25, 45);

            lblConfirm.Location = new Point(25, 80);
            txtConfirm.Location = new Point(25, 105);
            lblConfirm.Visible = _firstRun;
            txtConfirm.Visible = _firstRun;

            btnOk.Text = _firstRun ? "Create" : "Login";
            btnOk.Location = new Point(25, 150);
            btnOk.Click += BtnOk_Click;
            AcceptButton = btnOk;   // Enter key triggers the button

            Controls.AddRange(new Control[] { lblPassword, txtPassword, lblConfirm, txtConfirm, btnOk });
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            string pw = txtPassword.Text;

            if (_firstRun)
            {
                if (pw.Length < 8)
                {
                    MessageBox.Show("Use at least 8 characters.");
                    return;
                }
                if (pw != txtConfirm.Text)
                {
                    MessageBox.Show("Passwords do not match.");
                    return;
                }

                byte[] salt = CryptoHelper.GenerateSalt();
                byte[] keySalt = CryptoHelper.GenerateSalt();
                _db.SaveMaster(CryptoHelper.HashMasterPassword(pw, salt),
                               Convert.ToBase64String(salt),
                               Convert.ToBase64String(keySalt));

                OpenMain(CryptoHelper.DeriveBytes(pw, keySalt));
            }
            else
            {
                var master = _db.GetMaster()!.Value;
                byte[] salt = Convert.FromBase64String(master.Salt);

                if (CryptoHelper.VerifyMasterPassword(pw, master.Hash, salt))
                {
                    byte[] keySalt = Convert.FromBase64String(master.KeySalt);
                    OpenMain(CryptoHelper.DeriveBytes(pw, keySalt));
                }
                else
                {
                    _attempts++;
                    if (_attempts >= 3)
                    {
                        MessageBox.Show("Too many failed attempts. Closing.");
                        Close();
                        return;
                    }
                    MessageBox.Show($"Wrong password ({_attempts}/3).");
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }

        private void OpenMain(byte[] key)
        {
            var main = new MainForm(key);
            main.FormClosed += (s, e) => Close();   // closing main window exits the app
            Hide();
            main.Show();
        }
    }
}
