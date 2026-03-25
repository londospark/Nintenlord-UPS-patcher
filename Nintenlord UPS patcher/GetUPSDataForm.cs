using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Nintenlord.Hacking.Core;
using System.Threading.Tasks;

namespace Nintenlord.UPSpatcher
{
    public partial class GetUPSDataForm : Form
    {
        public GetUPSDataForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();
            open.Title = "Select a patch";
            open.Filter = "UPS files|*.ups";
            open.Multiselect = false;
            open.ShowDialog();
            if (open.FileNames.Length > 0)
            {
                textBox1.Text = open.FileName;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show("Patch doesn't exist.");
                return;
            }

            var lines = new List<String>();
            await Task.Run(() =>
            {
                UPSfile UPSFile = new UPSfile(textBox1.Text);
                int[,] details = UPSFile.GetData();
                UPSFile = null;

                lines.Capacity = details.Length + 1;
                lines.Add(textBox2.Lines[0]);
                var offsets = "Offsets";
                var lenghts = "Lenghts";
                lines.Add(offsets);
                var longestOffsetLenght = offsets.Length;
                for (var i = 0; i < details.Length / 2; i++)
                {
                    var line = Convert.ToString(details[i, 0], 16);
                    if (line.Length + 1 > longestOffsetLenght)
                        longestOffsetLenght = line.Length + 1;
                    lines.Add(line);
                }

                for (var i = 1; i < lines.Count; i++)
                    lines[i] += "\t";
                lines[1] += lenghts;
                for (var i = 2; i < lines.Count; i++)
                {
                    lines[i] += Convert.ToString(details[i - 2, 1], 16);
                    lines[i] = lines[i].ToUpper();
                }
            });

            textBox2.Lines = lines.ToArray();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
