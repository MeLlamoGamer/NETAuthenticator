using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using OtpNet;
using Newtonsoft.Json;
using System.Security.Cryptography;


namespace NETAuthenticator
{
    public partial class Form1 : Form
    {
        string dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NETAuthenticator",
            "accounts.json"
            );

        public class AuthAccount
        {
            public string Name { get; set; }
            public string Secret { get; set; }
        }
        List<AuthAccount> accounts = new List<AuthAccount>();
        public Form1()
        {
            InitializeComponent();
        }
        private void SaveAccounts()
        {
            try
            {
                var dir = Path.GetDirectoryName(dataPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Encriptar los datos con DPAPI
                byte[] encrypted = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(dataPath, encrypted);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cuentas: " + ex.Message);
            }
        }

        private void LoadAccounts()
        {
            try
            {
                if (File.Exists(dataPath))
                {
                    byte[] encrypted = File.ReadAllBytes(dataPath);
                    byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    string json = Encoding.UTF8.GetString(decrypted);

                    accounts = JsonConvert.DeserializeObject<List<AuthAccount>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar cuentas: " + ex.Message);
            }
        }


        private string GenerateTotpCode(string secret)
        {
            try
            {
                var bytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(bytes);
                return totp.ComputeTotp();
            }
            catch
            {
                return "ERROR";
            }
        }
        private void UpdateListView()
        {
            // 🔒 Guardar nombres seleccionados
            var selectedNames = listViewAccounts.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Text)
                .ToList();

            listViewAccounts.BeginUpdate(); // Mejora rendimiento visual
            listViewAccounts.Items.Clear();

            DateTime utcNow = DateTime.UtcNow;
            foreach (var acc in accounts)
            {
                try
                {
                    var bytes = Base32Encoding.ToBytes(acc.Secret);
                    var totp = new Totp(bytes);
                    string code = totp.ComputeTotp(utcNow);
                    int secondsRemaining = 30 - (int)(utcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds % 30);

                    var item = new ListViewItem(acc.Name);
                    item.SubItems.Add(code);
                    item.SubItems.Add(secondsRemaining.ToString());

                    // ✅ Restaurar selección si estaba seleccionado
                    if (selectedNames.Contains(acc.Name))
                        item.Selected = true;

                    listViewAccounts.Items.Add(item);
                }
                catch
                {
                    var item = new ListViewItem(acc.Name);
                    item.SubItems.Add("ERROR");
                    item.SubItems.Add("-");
                    listViewAccounts.Items.Add(item);
                }
            }

            listViewAccounts.EndUpdate();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddAccForm())
            {
                var result = addForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var newAccount = new AuthAccount
                    {
                        Name = addForm.AccountName,
                        Secret = addForm.AccountSecret
                    };

                    accounts.Add(newAccount);
                    UpdateListView();
                    SaveAccounts();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listViewAccounts.View = View.Details;
            listViewAccounts.FullRowSelect = true;
            listViewAccounts.Columns.Add("Name", 99);
            listViewAccounts.Columns.Add("Code", 80);
            listViewAccounts.Columns.Add("Remaining (s)", 80);
            timerUpdate.Interval = 1000;
            timerUpdate.Enabled = true;
            LoadAccounts();
        }

        private void timerUpdate_Tick_1(object sender, EventArgs e)
        {
            UpdateListView();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selectedItem in listViewAccounts.SelectedItems)
            {
                string nameToDelete = selectedItem.Text;
                var acc = accounts.FirstOrDefault(a => a.Name == nameToDelete);
                if (acc != null)
                    accounts.Remove(acc);
            }

            UpdateListView();
            SaveAccounts();
        }

        private void listViewAccounts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnDelete.PerformClick();  // Simula un clic
            }
        }
    }
}
