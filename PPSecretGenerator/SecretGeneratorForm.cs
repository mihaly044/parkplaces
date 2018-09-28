using PPNetLib.Prototypes;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PPNetLib.Extensions;

namespace PPSecretGenerator
{
    public partial class SecretGeneratorForm : Form
    {
        public SecretGeneratorForm()
        {
            InitializeComponent();
        }

        private void openSecretKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog() { Filter = "Secret key files (*.ppsk)|*.ppsk" } )
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var data = File.ReadAllBytes(dlg.FileName);
                        using (var stream = new MemoryStream(data))
                        {
                            var secret = Serializer.Deserialize<EncryptionKey>(stream);
                            txtPassword.Text = secret.Password;
                            txtSalt.Text = secret.Salt.ToHexString();
                        }
                    } catch(Exception)
                    {
                        MessageBox.Show("Could not parse input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void saveSecretKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog() { Filter = "Secret key files (*.ppsk)|*.ppsk" })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    if(txtPassword.Text.Length < 3 || txtPassword.Text.Length > 32)
                    {
                        MessageBox.Show("The passphrase must be between 3 and 32 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(txtSalt.Text.Length != 32)
                    {
                        MessageBox.Show("The salt must be exactly 16 bytes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (var stream = new FileStream(dlg.FileName, FileMode.CreateNew))
                        {
                            try
                            {
                                Serializer.Serialize(stream, new EncryptionKey()
                                {
                                    Password = txtPassword.Text,
                                    Salt = txtSalt.Text.ToUpper().ToHexByteArray()
                                });
                                stream.Close();
                                MessageBox.Show($"File {dlg.FileName} successfully saved.\nBe advised that losing this secret key file might revoke your access from the server.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show($"An unexpected error has happened: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }

        private void btnRandomize_Click(object sender, EventArgs e)
        {
            var random = new Random();
            var str = new StringBuilder();
            var salt = new byte[16];
            random.NextBytes(salt);

            for (var i = 0; i < salt.Length; i++)
                str.AppendFormat("{0:X2}", salt[i]);

            txtSalt.Text = str.ToString();
        }

        private void txtSalt_KeyPress(object sender, KeyPressEventArgs e)
        {
            var temp = "012345679ABCDEF";
            if (temp.IndexOf(e.KeyChar) == -1)
            {
                e.Handled = true;
            }
        }
    }
}
