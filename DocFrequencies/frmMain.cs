﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using BrightIdeasSoftware;
using System.Collections;

namespace wFrequencies
{
    public partial class FrmMain : Form
    {
        private bool isRunning = false;

        public FrmMain()
        {

            /*  ANNOTATION
             *  The software should be able to read pdf,doc,txt and odf
             *  The software should take all files in the input folder and read them one at a time
             *  The software should prepare lists of words with their frequencie
             *  The software should be awesome
             *  
             *  TODO:
             *  1.  Import doc, docx reader library
             *  2.  Import pdf reader library
             *  3.  Import odf reader library
             *  4.  Create functions for each supporting format
             *  5.  Create open file dialog to show the location of files
             *  6.  List the files filtering the extension
             *  7.  Take a file, read all of its contents
             *  8.  Scan through file and create a list of frequencies for each file
             *  9.  Sum-up the frequency files
             *  10. Voila!
            */

            InitializeComponent();

            btnExport.Visible = false;
            btnFrequenciesToXML.Visible = false;
        }

        public void UpdateStatus(string status)
        {
            lblStatus.Text = status;
        }

        public void UpdateProgress(int ProgressPercentage)
        {

        }
        private void loadFiles()
        {
            this.Enabled = false;
                   
            lblStatus.Text = "Загружаю список файлов";
            // Load all supported files from the chosen dretory 
            this.Enabled = true;

            Utils.fList = new List<xTextFile>();

            foreach (string file in Directory.EnumerateFiles(Utils.WorkDirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".doc") || s.EndsWith(".docx") || s.EndsWith(".odt") || s.EndsWith(".pdf") || s.EndsWith(".txt") || s.EndsWith(".xls") || s.EndsWith(".rtf") || s.EndsWith(".htm") || s.EndsWith(".html"))) {
                Utils.fList.Add(new xTextFile(file));
            }

            olvFiles.SetObjects(Utils.fList);
            lblStatus.Text = "Готов";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Utils.WorkDirPath = Environment.CurrentDirectory + "\\input\\";
            DirectoryInfo dInfo = new DirectoryInfo(Utils.WorkDirPath);
            if (!dInfo.Exists) dInfo.Create();
            txtWorkingDir.Text = Utils.WorkDirPath;
            DbHelper.SetConnection();
            DbHelper.createTables();

            olvFiles.SubItemChecking += delegate (object olvCheckSender, SubItemCheckingEventArgs olvCheckArgs) {
                // Set false all the other categories
                xTextFile rowObject = ((xTextFile)olvCheckArgs.RowObject);
                rowObject.isFiction = false;
                rowObject.isPoetry = false;
                rowObject.isScientific = false;
                rowObject.isSocPol = false;
                rowObject.isReligious = false;

                // After completion it will set the new value
            };

            // Grouping by months=============================================
            OLVColumn clm = ((OLVColumn)olvHistory.Columns[5]);

            clm.GroupKeyGetter = delegate (object rowObject) {
                xTextFile fm = (xTextFile)rowObject;
                return fm.created_at.Substring(0, 10);
            };

            clm.GroupKeyToTitleConverter = delegate (object groupKey) {
                DateTime dt = new DateTime();
                dt = DateTime.ParseExact(groupKey.ToString(), "dd.MM.yyyy", null);
                return groupKey.ToString();
            };
            //=================================================================

            // Set DateTime column as default for sorting to make it beautiful when it shows up for the first time!
            olvHistory.PrimarySortColumn = (olvHistory.GetColumn(5));

            olvHistory.SubItemChecking += delegate (object olvCheckSender, SubItemCheckingEventArgs olvCheckArgs) {
                // Set false all the other categories
                xTextFile rowObject = ((xTextFile)olvCheckArgs.RowObject);
                rowObject.isSelected = !rowObject.isSelected;

                // After completion it will set the new value
            };

            loadFiles();
            loadHistory();



        }



        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Do not allow the user to create new files via the FolderBrowserDialog.
            fbWorkingDir.ShowNewFolderButton = false;
            fbWorkingDir.SelectedPath = txtWorkingDir.Text;

            DialogResult result = fbWorkingDir.ShowDialog();
            if (result == DialogResult.OK) {
                Utils.WorkDirPath = fbWorkingDir.SelectedPath;
                txtWorkingDir.Text = Utils.WorkDirPath;
                loadFiles();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isRunning) {
                txtWorkingDir.Enabled = false;
                btnBrowse.Enabled = false;
                isRunning = true;
                bgwCounter.RunWorkerAsync();
                btnStart.BackColor = Color.IndianRed;
                btnStart.Text = "Cтоп";
                prbStatus.Visible = true;
            } else {
                if (bgwCounter.IsBusy) bgwCounter.CancelAsync();
                lblStatus.Text = "Отмена"; Update();
                onFinishCounting();
            }
        }

        private string spacer(int count)
        {
            string spaces = "";
            for (int i = 0; i < count; i++) spaces += " ";
            return spaces;
        }

        private void removeSelectedFromOlvFiles()
        {
            foreach (xTextFile xtf in (olvFiles.SelectedObjects)) {
                Utils.fList.Remove(xtf);
            }

            olvFiles.SetObjects(Utils.fList);
        }

        private void ctxRemoveFromtheList_Click(object sender, EventArgs e)
        {
            removeSelectedFromOlvFiles();
        }

        private void loadHistory()
        {
            history = DbHelper.GetHistory();
            olvHistory.SetObjects(history);
        }
        // Tab History
        List<xTextFile> history;
        private void tbcHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbcHistory.SelectedTab == tbcHistory.TabPages[1]) {
                btnExport.Visible = true;
                btnFrequenciesToXML.Visible = true;
                // Tab History has been selected
            } else {
                btnExport.Visible = false;
                btnFrequenciesToXML.Visible = false;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = (olvFiles.SelectedObjects.Count == 0);
        }

        private void olvHistory_DoubleClick(object sender, EventArgs e)
        {
            if (olvHistory.SelectedObject != null) {
                FrmFrequencies frmFreq = new FrmFrequencies((xTextFile)(olvHistory.SelectedObject));
                frmFreq.Show();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Utils.OlvToExcelExport(olvHistory, "List Of Literature " + Utils.GetCurrentDate());
        }

        private void btnFrequenciesToXML_Click(object sender, EventArgs e)
        {
            Utils.FullExcelExport(history, "Full Export " + Utils.GetCurrentDate());
        }

        private void olvFiles_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && olvFiles.SelectedObjects.Count > 0) {
                removeSelectedFromOlvFiles();
            }
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmAbout frmAbout = new FrmAbout();
            frmAbout.ShowDialog();
        }

        private void bgwCounter_DoWork(object sender, DoWorkEventArgs e)
        {

            foreach (xTextFile xFile in Utils.fList) {
                if (bgwCounter.CancellationPending) {
                    bgwCounter.ReportProgress(-1, xFile);
                    return;
                }
                bgwCounter.ReportProgress(-2, xFile);
                string contents = xFile.Processor.GetAllText(xFile.filePath);
                xFile.charactersCount = contents.Length;

                xFile.frequencies = new List<xWordFrequencies>();
                var words = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

                var wordPattern = new Regex(@"\w+");
                xFile.wordsCount = wordPattern.Matches(contents).Count;

                int progress = 0;
                foreach (Match match in wordPattern.Matches(contents)) {
                    if (bgwCounter.CancellationPending) {
                        bgwCounter.ReportProgress(-1, xFile);
                        return;
                    }
                    progress++;
                    int currentCount = 0;
                    words.TryGetValue(match.Value, out currentCount);
                    bgwCounter.ReportProgress(progress, xFile);
                    currentCount++;
                    words[match.Value] = currentCount;
                }

                foreach (var row in words.OrderByDescending(pair => pair.Value)) {
                    xWordFrequencies xwf = new xWordFrequencies();
                    xwf.word = row.Key.ToLower();
                    xwf.word = xwf.word.Substring(0, 1).ToUpper() + xwf.word.Substring(1);
                    xwf.frequency = row.Value;
                    xFile.frequencies.Add(xwf);
                }
                xFile.uniqueWordsCount = xFile.frequencies.Count();
                xFile.SaveFileInfo();

            }
       
            bgwCounter.ReportProgress(-3, null);
        }

        private void bgwCounter_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            xTextFile xFile = (xTextFile)e.UserState;

            if (e.ProgressPercentage == -1) {
                // Is cancelled
                lblStatus.Text = "Отменено пользователем";
            } else if (e.ProgressPercentage == -2) {
                // Still reading
                if (!lblStatus.Text.Equals("Читаю: " + xFile.fileName)) {
                    lblStatus.Text = "Читаю: " + xFile.fileName; Update(); 
                }
            } else if (e.ProgressPercentage == -3) {
                // Has finished
                onFinishCounting();
            } else {
                // Counting
                prbStatus.Maximum = xFile.wordsCount;
                prbStatus.Value = e.ProgressPercentage;
                if ((prbStatus.Maximum != prbStatus.Value) || prbStatus.Maximum == 0) {
                    if (prbStatus.Visible == false) prbStatus.Visible = true;
                    if (!lblStatus.Text.Equals("Обрабатываю: " + xFile.fileName)) { lblStatus.Text = "Обрабатываю: " + xFile.fileName; Update(); }
                } else {
                    // Current file has been finished, let's move to the next
                    prbStatus.Value = 0;
                    prbStatus.Visible = false;
                }
            }
        }

        private void onFinishCounting()
        {
            loadHistory();
            isRunning = false;
            btnStart.BackColor = Color.LightGreen;
            txtWorkingDir.Enabled = true;
            btnBrowse.Enabled = true;
            btnStart.Text = "Старт";
            prbStatus.Visible = false;
            lblStatus.Text = "Работа выполнена";
        }

        private void сброситьБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utils.msgConfirmation("Это действие приведет к полной очистке всей БД приложения, вы уверены?") == DialogResult.Yes) {
                DbHelper.dropTables();
                loadFiles();
                loadHistory();
                lblStatus.Text = "База данных успешно очищена!";
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DocProcessor.Dispose();
        }
    }
}