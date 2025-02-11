﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoshStat
{
    public partial class FrmFrequencies : Form
    {
        private xTextFile _xFile;

        public FrmFrequencies(xTextFile xFile)
        {
            InitializeComponent();

            _xFile = xFile;
            lblCategory.Text += xFile.getCategoryName();
            lblCharactersCount.Text += xFile.charactersCount.ToString();
            Text += " | " + xFile.fileName;
            lblUniqueWords.Text += xFile.uniqueWordsCount.ToString();
            lblWordCount.Text += xFile.wordsCount.ToString();
            olvFrequencies.SetObjects(xFile.frequencies);
        }
        public FrmFrequencies(xTextFile xFile, string word)
        {
            InitializeComponent();

            _xFile = xFile;
            lblCategory.Text += xFile.getCategoryName();
            lblCharactersCount.Text += xFile.charactersCount.ToString();
            Text += " | "+xFile.fileName;
            lblUniqueWords.Text += xFile.uniqueWordsCount.ToString();
            lblWordCount.Text += xFile.wordsCount.ToString();
            olvFrequencies.SetObjects(xFile.frequencies);

            // Select the element
            olvFrequencies.SelectedObject = (xFile.frequencies.First(xObj => xObj.word == word));
            // Scroll to the selected element
            olvFrequencies.EnsureModelVisible((xFile.frequencies.First(xObj => xObj.word == word)));
        }

        private void frmFrequencies_Load(object sender, EventArgs e)
        {
            olvFrequencies.PrimarySortColumn = (olvFrequencies.GetColumn(1));

            olvFrequencies.PrimarySortColumn = olvFrequencies.GetColumn(0);
            olvFrequencies.PrimarySortOrder = SortOrder.Descending;
            olvFrequencies.Sort();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Utils.ExcelExport(olvFrequencies, _xFile.fileName);
        }

        private void olvFrequencies_SelectionChanged(object sender, EventArgs e)
        {
            string lblWordsCountPrefix = "Выделено слов: ";
            string lblWordsPercentagePrefix = "Выделено в процентах: ";
            string lblWordsFrequenciesPrefix = "Сумма выделенных частот: ";

            lblSelectedWordsCount.Text = lblWordsCountPrefix + olvFrequencies.SelectedObjects.Count.ToString();
            float sumPercentage = 0;
            int sumFrequencies = 0;
            foreach (var obj in olvFrequencies.SelectedObjects) {
                xWordFrequencies xwf = (xWordFrequencies)obj;
                sumPercentage += xwf.percentage;
                sumFrequencies += xwf.frequency;
            }
            lblSelectedWordsPercentage.Text = lblWordsPercentagePrefix + sumPercentage.ToString("F") + "%";
            lblSelectedFrequency.Text = lblWordsFrequenciesPrefix + sumFrequencies.ToString();
        }
    }
}
