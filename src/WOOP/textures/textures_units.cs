using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using WOOP;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Collections;

namespace WOOP
{
    public class TexturesSquare : Dictionary<String, TexturesLine>
    {
        public bool loadFromDir(String dir, Color color, bool rotates)
        {
            try
            {
                string[] files = Directory.GetFiles(dir, "*.bmp");

                foreach (String pth in files)
                {
                    String f = Path.GetFileName(pth);
                    int ind = f.LastIndexOf("_fr");
                    f = f.Remove(ind, f.Count() - ind);

                    TexturesLine line = new TexturesLine();
                    if (!line.loadFromFile(pth, rotates, color)) return false;
                    this.Add(f, line);
                }

                logm("Textures loaded from dir " + dir);
            }
            catch
            {
                logm("Error: cannot load textures from dir " + dir);
                return false;
            }

            return true;
        }


        String LogTag { get { return ""; } }
        void logm(String text) { W.core.textLogs.TexturesLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    }
}