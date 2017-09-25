﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace wFrequencies
{
    public class RtfProcessor : ITextProcessor
    {
        // Make it into a singleton
        private static readonly ITextProcessor _instance = new RtfProcessor();
        public static ITextProcessor GetInstance() { return _instance; }
        private RtfProcessor() { }

        public string GetAllText()
        {
            var form = Form.ActiveForm as frmMain;
            form.lblStatus.Text = "Читаю Rtf ... ";
            form.Refresh();

            string allText = "";

            RichTextBox rtb = new RichTextBox();

            foreach (xTextFile file in Utils.fList.Where((x) => x.fileName.EndsWith("rtf"))) {
                rtb.Rtf = File.ReadAllText(file.filePath);
                allText += rtb.Text;
            }


            return allText;
        }
    }
}
