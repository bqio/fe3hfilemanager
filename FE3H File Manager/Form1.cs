using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;

namespace FE3H_File_Manager
{
    public partial class Form1 : Form
    {
        List<string> database = new List<string>();
        string dataPath = "";
        string tmpPath = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void CheckUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateDataBase();
        }

        private void UpdateDataBase()
        {
            string url = "https://docs.google.com/spreadsheets/d/18bCCrsHwyAU-JSlpvaulVos3j8dtPBr0mDB-vLWib54/export?format=xlsx&id=18bCCrsHwyAU-JSlpvaulVos3j8dtPBr0mDB-vLWib54";
            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(url, "db.xlsx");

            DateTime dt = File.GetLastWriteTime("db.xlsx");
            toolStripStatusLabel1.Text = "Last update: " + dt;

            MessageBox.Show("Database successfully updated.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadDataBase();
        }

        private void LoadDataBase()
        {
            using(var stream = File.Open("db.xlsx", FileMode.Open, FileAccess.Read))
            {
                using(var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    DataTable table = new DataTable();

                    table.Columns.Add("Index");
                    table.Columns.Add("Path");
                    table.Columns.Add("Description");

                    for (int i = 1; i < result.Tables[1].Rows.Count; i++)
                    {
                        DataRow dr = table.NewRow();

                        dr["Index"] = result.Tables[1].Rows[i].ItemArray[0];
                        dr["Path"] = result.Tables[1].Rows[i].ItemArray[2];
                        dr["Description"] = result.Tables[1].Rows[i].ItemArray[3];

                        table.Rows.Add(dr);
                    }
                    
                    dataGridView1.DataSource = table;
                    dataGridView1.Columns[1].Width = 600;

                    dataGridView1.Update();
                    WindowState = FormWindowState.Maximized;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("db.xlsx"))
            {
                DateTime dt = File.GetLastWriteTime("db.xlsx");
                toolStripStatusLabel1.Text = "Last update: " + dt;

                LoadDataBase();
            } else
            {
                UpdateDataBase();
            }
        }

        private void TestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDataBase();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "DATA0.bin (*.bin)|*.bin";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    dataPath = openFileDialog1.FileName;
                    tmpPath = folderBrowserDialog1.SelectedPath;
                }
                else
                {
                    MessageBox.Show("You must specify a dist folder for the files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("You must specify the file DATA0.bin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataPath.Length == 0 || tmpPath.Length == 0)
            {
                MessageBox.Show("Select the DATA0.bin file and the dist directory for the files in the open menu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int index = dataGridView1.CurrentCell.RowIndex;
            int offset = index * 32;
            string path = dataGridView1.Rows[index].Cells[1].Value.ToString();

            if (path.Length == 0)
            {
                MessageBox.Show("File path not specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var metaReader = new BinaryReader(File.OpenRead(dataPath)))
            {
                metaReader.BaseStream.Position = offset;

                long foffset = metaReader.ReadInt64();
                long funcompressedSize = metaReader.ReadInt64();
                long fcompressedSize = metaReader.ReadInt64();
                long fisCompressed = metaReader.ReadInt64();

                string dataDir = Path.GetDirectoryName(dataPath);

                using (var dataReader = new BinaryReader(File.OpenRead(dataDir + "\\DATA1.bin")))
                {
                    dataReader.BaseStream.Position = foffset;

                    if (fisCompressed == 1)
                    {
                        if (fcompressedSize > 0)
                        {
                            if (!Directory.Exists(tmpPath + "\\" + Path.GetDirectoryName(path)))
                            {
                                Directory.CreateDirectory(tmpPath + "\\" + Path.GetDirectoryName(path));
                            }

                            File.WriteAllBytes(tmpPath + "\\" + path, dataReader.ReadBytes((int)funcompressedSize));

                            MessageBox.Show("File structure successfully created.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    } else
                    {
                        if (funcompressedSize > 0)
                        {
                            if (!Directory.Exists(tmpPath + "\\" + Path.GetDirectoryName(path)))
                            {
                                Directory.CreateDirectory(tmpPath + "\\" + Path.GetDirectoryName(path));
                            }

                            File.WriteAllBytes(tmpPath + "\\" + path, dataReader.ReadBytes((int)funcompressedSize));

                            MessageBox.Show("File structure successfully created.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void GithubToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/bqio/fe3hfilemanager");
        }

        private async void AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to create a structure from all available paths?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                if (dataPath.Length == 0 || tmpPath.Length == 0)
                {
                    MessageBox.Show("Select the DATA0.bin file and the dist directory for the files in the open menu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string dataDir = Path.GetDirectoryName(dataPath);

                menu.Enabled = false;
                dataGridView1.Enabled = false;

                using (var metaReader = new BinaryReader(File.OpenRead(dataPath)))
                {
                    using (var dataReader = new BinaryReader(File.OpenRead(dataDir + "\\DATA1.bin")))
                    {
                        for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                        {
                            string path = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            toolStripStatusLabel2.Text = path;

                            if (path.Length != 0)
                            {
                                int offset = i * 32;

                                metaReader.BaseStream.Position = offset;

                                long foffset = metaReader.ReadInt64();
                                long funcompressedSize = metaReader.ReadInt64();
                                long fcompressedSize = metaReader.ReadInt64();
                                long fisCompressed = metaReader.ReadInt64();

                                dataReader.BaseStream.Position = foffset;

                                if (fisCompressed == 1)
                                {
                                    if (fcompressedSize > 0)
                                    {
                                        if (!Directory.Exists(tmpPath + "\\" + Path.GetDirectoryName(path)))
                                        {
                                            Directory.CreateDirectory(tmpPath + "\\" + Path.GetDirectoryName(path));
                                        }

                                        await Task.Run(async () => {
                                            File.WriteAllBytes(tmpPath + "\\" + path, dataReader.ReadBytes((int)fcompressedSize));
                                        });
                                    }
                                }
                                else
                                {
                                    if (funcompressedSize > 0)
                                    {
                                        if (!Directory.Exists(tmpPath + "\\" + Path.GetDirectoryName(path)))
                                        {
                                            Directory.CreateDirectory(tmpPath + "\\" + Path.GetDirectoryName(path));
                                        }

                                        await Task.Run(async () => {
                                            File.WriteAllBytes(tmpPath + "\\" + path, dataReader.ReadBytes((int)funcompressedSize));
                                        });
                                    }
                                }
                            }
                        }

                        MessageBox.Show("Structure successfully created.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        menu.Enabled = true;
                        dataGridView1.Enabled = true;
                    }
                }
            }
        }

        private void NewCustomRangeScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataPath.Length == 0 || tmpPath.Length == 0)
            {
                MessageBox.Show("Select the DATA0.bin file and the dist directory for the files in the open menu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            crs crs = new crs();
            crs.Owner = this;
            crs.Show();
        }

        public async void CreateCustomRangeScript(string pathScript, int rangeStart, int rangeEnd)
        {
            for (int i = rangeStart; i < rangeEnd; i++)
            {
                int offset = i * 32;
                string path = pathScript.Replace("$", i.ToString());

                using (var metaReader = new BinaryReader(File.OpenRead(dataPath)))
                {
                    metaReader.BaseStream.Position = offset;

                    long foffset = metaReader.ReadInt64();
                    long funcompressedSize = metaReader.ReadInt64();
                    long fcompressedSize = metaReader.ReadInt64();
                    long fisCompressed = metaReader.ReadInt64();

                    string dataDir = Path.GetDirectoryName(dataPath);

                    using (var dataReader = new BinaryReader(File.OpenRead(dataDir + "\\DATA1.bin")))
                    {
                        dataReader.BaseStream.Position = foffset;

                        if (fisCompressed == 1)
                        {
                            if (fcompressedSize > 0)
                            {
                                if (!Directory.Exists(tmpPath + "\\" + Path.GetDirectoryName(path)))
                                {
                                    Directory.CreateDirectory(tmpPath + "\\" + Path.GetDirectoryName(path));
                                }

                                await Task.Run(async () => {
                                    File.WriteAllBytes(tmpPath + "\\" + path, dataReader.ReadBytes((int)fcompressedSize));
                                });
                            }
                        }
                        else
                        {
                            if (funcompressedSize > 0)
                            {
                                if (!Directory.Exists(tmpPath + "\\" + Path.GetDirectoryName(path)))
                                {
                                    Directory.CreateDirectory(tmpPath + "\\" + Path.GetDirectoryName(path));
                                }

                                await Task.Run(async () => {
                                    File.WriteAllBytes(tmpPath + "\\" + path, dataReader.ReadBytes((int)funcompressedSize));
                                });
                            }
                        }
                    }
                    toolStripStatusLabel2.Text = path;
                }
            }
            MessageBox.Show("Structure successfully created.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            toolStripStatusLabel2.Text = "";
        }
    }
}
