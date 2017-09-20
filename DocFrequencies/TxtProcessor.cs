﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DocFrequencies
{
    class TxtProcessor
    {
        public string GetAllText()
        {
            string allText = "";

            foreach (string filePath in (new Utils()).FindFilesRecursively("*.txt"))
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.GetEncoding(1251), true))
                {
                    reader.Peek(); // you need this!
                    var encoding = reader.CurrentEncoding;
                    allText += File.ReadAllText(filePath, encoding);
                }
            }

            //foreach (string filePath in (new Utils()).FindFilesRecursively("*.txt"))
            //{
            //    // Maybe we would need to figure out the encoding
            //    allText += File.ReadAllText(filePath, Utils.GetEncoding(filePath));
            //}
            return allText;
        }

    }
}
