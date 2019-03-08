using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeckSearcher
{
    public partial class DeckSearcher : Form
    {
        public DeckSearcher()
        {
            InitializeComponent();
            TextBoxPath.Text = Path.Combine(Environment.CurrentDirectory, "decks_save");
        }
        bool stop = false;
        private void  button1_Click(object sender, EventArgs e)
        {

            if (!backgroundWorker1.IsBusy)
            {
                listBox1.Items.Clear();
                progressBar1.Value = 0;
                stop = false;
                duplicatesLabel.Text = "0";
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                stop = true;
            }
        }

        private List<FileInfo> GetFiles(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(directory.GetFiles().Where(x=>x.Extension==".ydk"));
            return files;

        }
        private void GetText(List<FileInfo> files)
        {
            progressBar1.Invoke(new Action(() => {
                progressBar1.Maximum = files.Count;
            }));
            List<string> texts = new List<string>();
           int duplicatesCounter = 0;
            foreach(FileInfo file in files)
            {
                string text = FileTools.ReadFileString(file.FullName);
                
                if (stop)
                {
                    progressBar1.Invoke(new Action(() => {
                        progressBar1.Value = progressBar1.Maximum;
                    }));
                    throw new Exception();
                }
                else
                {
                    progressBar1.Invoke(new Action(() => {
                        progressBar1.Value++;
                        DecksCheckedLabel.Text = progressBar1.Value.ToString();
                    }));
                }
                if(CheckIfTextsContains(text, textBoxID.Text)^filterCheckBox.Checked)
                {
                    if (!CheckIfDuplicate(text, texts))
                    {
                        listBox1.Invoke(new Action(() => {

                            int index =listBox1.Items.Add(file);
                            label3.Text = listBox1.Items.Count.ToString(); listBox1.Sorted = true;
                        }));
                        texts.Add(text);
                    }
                    else
                    {
                        duplicatesCounter++;
                        duplicatesLabel.Invoke(new Action(() => {
                            duplicatesLabel.Text = duplicatesCounter.ToString();
                        }));
                    }
                }
          
            }
            return;
        }
        private bool CheckIfTextsContains(string texts,string value)
        {
            if (value == "")
            {
                return true;
            }
            else
            {
                return texts.Contains(value);
            }
            
                

        }
        private bool CheckIfDuplicate(string text,List<string> texts)
        {

            return texts.Contains(text);

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (int.TryParse(textBoxID.Text, out int parsedValue)||textBoxID.Text=="")
            {
                string path = TextBoxPath.Text;
                try
                {
                GetText(GetFiles(path));
                }
                catch
                {
                    return;
                }
                
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                FileInfo file = (FileInfo)listBox1.Items[index];
                Process.Start("explorer.exe", "/select," + file.FullName);
            }
        }
        private void Export()
        {
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(listBox1.Items.Cast<FileInfo>());
            string targetPath = Path.Combine(TextBoxPath.Text,"Export");
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            foreach(FileInfo file in files)
            {
                string fileName =/*Regex.Replace(*/file.Name.Remove(file.Name.LastIndexOf(".")/*), @"\d+\ ([a-zA-Z])", "$1"*/);
                fileName = Regex.Replace(fileName, @"(\d+\.){1,}\d+", "");
                fileName = Regex.Replace(fileName, @"(\d+\-){2,3}\d+", "");
                fileName = fileName + ".ydk";
                string destFile = Path.Combine(targetPath, fileName);
                File.Copy(file.FullName, destFile, true);
            }
            TextBoxPath.Text = targetPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Export();
        }
    }
}
