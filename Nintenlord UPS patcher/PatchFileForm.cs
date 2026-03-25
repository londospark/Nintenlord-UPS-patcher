using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Nintenlord.Hacking.Core;

namespace Nintenlord.UPSpatcher
{
    public partial class PatchFileForm : Form
    {

        public PatchFileForm()
        {
            InitializeComponent();
            this.Resize += new EventHandler(Form1_Resize);
            this.MinimumSize = new Size(this.Width - this.textBox1.Width, this.Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            textBox1.Size = this.Size - this.MinimumSize;
            textBox3.Size = this.Size - this.MinimumSize;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();
            open.Title = "Select a file.";
            open.Filter = "All files|*";
            open.Multiselect = false;
            open.ShowDialog();
            if (open.FileNames.Length > 0)
            {
                textBox1.Text = open.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();
            open.Title = "Select a patch";
            open.Filter = "UPS files|*.ups";
            open.Multiselect = false;
            open.ShowDialog();
            if (open.FileNames.Length > 0)
            {
                textBox3.Text = open.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var upsFile = new UPSfile(textBox3.Text);
            byte[] file = null;

            try
            {
                var br = new BinaryReader(File.Open(textBox1.Text, FileMode.OpenOrCreate));
                file = br.ReadBytes((int)br.BaseStream.Length);
                br.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Error opening file\n" + textBox1.Text);
                return;
            }

            if (!upsFile.ValidPatch)
            {
                MessageBox.Show("The patch is corrupt.");
                return;
            }

            var validToApply = upsFile.ValidToApply(file);

            if (radioButton1.Checked)
            {
                if (!validToApply)
                {
                    MessageBox.Show("The patch doesn't match the file.\nPatching canceled.");
                    return;
                }
            }
            else if (radioButton2.Checked)
            {
                if (!validToApply)
                {
                    if (MessageBox.Show("The patch doesn't match the file.\nPatch anyway?", "Patch?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return;
                }
            }
            else if (radioButton3.Checked)
            {
                if (!validToApply)
                    MessageBox.Show("The patch doesn't match the file.\nPatching anyway.");
            }
            else if (!radioButton4.Checked)
            {
                throw new Exception("What do you want me to do!?!?!?");
            }

            if (checkBox1.Checked)
            {
                var filePath = Path.ChangeExtension(textBox1.Text, ".bak");
                if (File.Exists(filePath))
                {
                    var fileName = Path.GetFileNameWithoutExtension(textBox1.Text);
                    var i = 1;
                    while (File.Exists(Path.GetDirectoryName(textBox1.Text) + fileName + i + ".bak"))
                    {
                        i++;
                    }
                    filePath = Path.GetDirectoryName(textBox1.Text) + fileName + i + ".bak";
                }

                File.Copy(textBox1.Text, filePath, false);
            }

            var newFile = upsFile.Apply(file);

            try
            {
                var bw = new BinaryWriter(File.Open(textBox1.Text, FileMode.Truncate));
                bw.Write(newFile);
                bw.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Error opening file\n" + textBox1.Text);
                return;
            }

            MessageBox.Show("Patching has been done.");
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e) => this.Close();
    }
}
