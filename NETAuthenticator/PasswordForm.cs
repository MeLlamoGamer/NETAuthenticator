using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace NETAuthenticator
{
    public partial class PasswordForm : Form
    {
        public bool IsSettingPassword { get; set; }
        public string EnteredPassword => txtPassword.Text;

        public PasswordForm()
        {
            InitializeComponent();
        }

        private void PasswordForm_Load(object sender, EventArgs e)
        {
            lblMensaje.Text = IsSettingPassword ? "Register a password" : "Insert your password";
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password cannot be empty");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

}
