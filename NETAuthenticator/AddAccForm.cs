using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace NETAuthenticator
{
    public partial class AddAccForm : Form
    {
        public string AccountName { get; private set; }
        public string AccountSecret { get; private set; }
        public AddAccForm()
        {
            InitializeComponent();
        }

        private void AddAccForm_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtSecret.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }

            AccountName = txtName.Text.Trim();
            AccountSecret = txtSecret.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ParseOtpAuthUri(string uri)
        {
            try
            {
                if (!uri.StartsWith("otpauth://"))
                {
                    MessageBox.Show("El código QR no es válido.");
                    return;
                }

                Uri uriObj = new Uri(uri);
                string label = Uri.UnescapeDataString(uriObj.AbsolutePath.Trim('/'));

                string query = uriObj.Query.TrimStart('?');
                string[] parts = query.Split('&');
                string secret = null;

                foreach (string part in parts)
                {
                    string[] kv = part.Split('=');
                    if (kv.Length == 2 && kv[0] == "secret")
                    {
                        secret = kv[1];
                        break;
                    }
                }

                if (string.IsNullOrEmpty(secret))
                {
                    MessageBox.Show("No se encontró el secreto en el QR.");
                    return;
                }

                txtName.Text = label;
                txtSecret.Text = secret;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al analizar el código QR: " + ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            using (var scanner = new QRCodeScanner())
            {
                scanner.QrCodeDetected += (qrText) =>
                {
                    ParseOtpAuthUri(qrText);
                };
                scanner.ShowDialog();
            }
        }
    }
}
