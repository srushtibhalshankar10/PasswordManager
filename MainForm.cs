using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace PasswordManager
{
    public class MainForm : Form
    {
        private readonly DatabaseHelper _db = new();
        private readonly byte[] _key;
        private int _selectedId = 0;   // 0 = nothing selected

        private readonly TextBox txtSearch = new() { Location = new Point(80, 15), Width = 300 };

        private readonly DataGridView grid = new()
        {
            Location = new Point(20, 50),
            Size = new Size(660, 240),
            ReadOnly = true,
            AllowUserToAddRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private readonly TextBox txtWebsite = new() { Location = new Point(110, 310), Width = 250 };
        private readonly TextBox txtUsername = new() { Location = new Point(110, 345), Width = 250 };
        private readonly TextBox txtPassword = new()
        {
            Location = new Point(110, 380),
            Width = 250,
            UseSystemPasswordChar = true
        };
        private readonly CheckBox chkShow = new() { Text = "Show", Location = new Point(370, 382), AutoSize = true };

        private readonly Button btnAdd = new() { Text = "Add", Location = new Point(110, 420), Width = 80 };
        private readonly Button btnUpdate = new() { Text = "Update", Location = new Point(200, 420), Width = 80 };
        private readonly Button btnDelete = new() { Text = "Delete", Location = new Point(290, 420), Width = 80 };
        private readonly Button btnClear = new() { Text = "Clear", Location = new Point(380, 420), Width = 80 };

        public MainForm(byte[] key)
        {
            _key = key;

            Text = "Password Manager";
            ClientSize = new Size(700, 470);
            StartPosition = FormStartPosition.CenterScreen;

            Controls.Add(new Label { Text = "Search:", Location = new Point(20, 18), AutoSize = true });
            Controls.Add(new Label { Text = "Website:", Location = new Point(20, 313), AutoSize = true });
            Controls.Add(new Label { Text = "Username:", Location = new Point(20, 348), AutoSize = true });
            Controls.Add(new Label { Text = "Password:", Location = new Point(20, 383), AutoSize = true });
            Controls.AddRange(new Control[]
            {
            txtSearch, grid, txtWebsite, txtUsername, txtPassword,
            chkShow, btnAdd, btnUpdate, btnDelete, btnClear
            });

            // Events
            txtSearch.TextChanged += (s, e) => LoadGrid();          // live search
            grid.SelectionChanged += Grid_SelectionChanged;
            chkShow.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkShow.Checked;
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => { grid.ClearSelection(); ClearFields(); };

            LoadGrid();
        }

        // Loads all entries, or only the matches if something is typed in the search box
        private void LoadGrid()
        {
            string term = txtSearch.Text.Trim();
            var list = term == "" ? _db.GetAllEntries() : _db.SearchEntries(term);

            grid.DataSource = list;
            grid.Columns["EncryptedPassword"]!.Visible = false;   // keep encrypted data hidden
            grid.Columns["IV"]!.Visible = false;
            grid.ClearSelection();
            ClearFields();
        }

        // When a row is clicked, show its details (password is decrypted here)
        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            if (grid.SelectedRows[0].DataBoundItem is not PasswordEntry entry) return;

            _selectedId = entry.Id;
            txtWebsite.Text = entry.Website;
            txtUsername.Text = entry.Username;
            txtPassword.Text = CryptoHelper.Decrypt(entry.EncryptedPassword, entry.IV, _key);
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!FieldsAreValid()) return;

            var (cipher, iv) = CryptoHelper.Encrypt(txtPassword.Text, _key);
            _db.AddEntry(txtWebsite.Text.Trim(), txtUsername.Text.Trim(), cipher, iv);

            LoadGrid();
            MessageBox.Show("Entry added.");
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0)
            {
                MessageBox.Show("Select an entry from the list first.");
                return;
            }
            if (!FieldsAreValid()) return;

            var (cipher, iv) = CryptoHelper.Encrypt(txtPassword.Text, _key);   // new IV each time
            _db.UpdateEntry(_selectedId, txtWebsite.Text.Trim(), txtUsername.Text.Trim(), cipher, iv);

            LoadGrid();
            MessageBox.Show("Entry updated.");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedId == 0)
            {
                MessageBox.Show("Select an entry from the list first.");
                return;
            }

            var answer = MessageBox.Show("Delete this entry?", "Confirm",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;

            _db.DeleteEntry(_selectedId);
            LoadGrid();
        }

        private bool FieldsAreValid()
        {
            if (string.IsNullOrWhiteSpace(txtWebsite.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Website, username and password are all required.");
                return false;
            }
            return true;
        }

        private void ClearFields()
        {
            _selectedId = 0;
            txtWebsite.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
        }
    }
}
