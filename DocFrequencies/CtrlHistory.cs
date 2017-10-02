﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Diagnostics;

namespace StrangeWords
{
    public partial class CtrlHistory : UserControl
    {
        public CtrlHistory()
        {
            InitializeComponent();
        }

        public void clearHistory()
        {
            olvHistory.ClearObjects();
        }

        // A flag to prevent olv from updating two times on start because of initializing elements
        private bool isReady = false;

        private void CtrlHistory_Load(object sender, EventArgs e)
        {
            olvHistory.SubItemChecking += delegate (object olvCheckSender, SubItemCheckingEventArgs olvCheckArgs) {
                // Set false all the other categories
                xTextFile rowObject = ((xTextFile)olvCheckArgs.RowObject);
                rowObject.isSelected = !rowObject.isSelected;

                // After completion it will set the new value
            };
            isReady = true;
        }

        public void loadHistory()
        {
            DateTime dt = DateTime.Now;
            TimeSpan ts = new TimeSpan(6, 23, 59, 59);
            dtpFrom.Value = dt.Subtract(ts);
            dtpTo.Value = dt;

            Utils.history = DbHelper.GetHistory(dtpFrom.Value.ToString("yyyy-MM-dd HH:mm:ss"), dtpTo.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (Utils.history != null) {
                Debug.WriteLine(Utils.history.Count);
                olvHistory.SetObjects(Utils.history);
            } else {
                olvHistory.ClearObjects();
            }
        }

        private void btnFullReport_Click(object sender, EventArgs e)
        {
            if (Utils.history == null) return;
            this.Enabled = false;
            FrmTotalDetails frmDetailedHistory = new FrmTotalDetails();
            frmDetailedHistory.Show();
            this.Enabled = true;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (Utils.history == null) return;
            this.Enabled = false;
            Utils.ExcelExport(olvHistory, "История", false);
            this.Enabled = true;
        }

        private void btnTotalFrequencies_Click(object sender, EventArgs e)
        {
            if (Utils.history == null) return;
            this.Enabled = false;
            FrmTotalFrequencies frmTotalFrequencies = new FrmTotalFrequencies();
            frmTotalFrequencies.Show();
            this.Enabled = true;
        }

        private void dtpFrom_ValueChanged(object sender, EventArgs e)
        {
            if (isReady) loadHistory();
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e)
        {
            if (isReady) loadHistory();
        }

        private void olvHistory_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                if (olvHistory.SelectedObject != null) {
                    FrmFrequencies frmFreq = new FrmFrequencies((xTextFile)(olvHistory.SelectedObject));
                    frmFreq.Show();
                }
            }
        }
    }
}
