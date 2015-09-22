using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TZPlarium
{
    public class Directory
    {
        public DirectoryInfo DI;
        public List<FileInfo> LF;
        public int Index;

        public Directory()
        {
        }

        public Directory(DirectoryInfo dI, List<FileInfo> lf, int ind)
        {
            DI = dI;
            LF = lf;
            Index = ind;
        }

        public override string ToString()
        {
            return "DirectoryName=\"" + DI.Name + "\"  CreationTime =\"" + DI.CreationTime + "\"  LastWriteTime =\"" + DI.LastWriteTime +
             "\"  LastAccessTime =\"" + DI.LastAccessTime + "\"  Atributes =\"" + DI.Attributes + "\"";
        }
        public List<string> ToListString()
        {
            List<string> ls = new List<string>();
            string s = "";
            for (int i = 0; i < Index + 1; ++i)
            {
                s += "  ";
            }
            foreach (FileInfo a in LF)
            {
                ls.Add(s + "FileName=\"" + a.Name + "\"   CreationTime=\"" + a.CreationTime + "\"  LastWriteTime =\"" + DI.LastWriteTime +
             "\"  LastAccessTime =\"" + DI.LastAccessTime + "\"  Size=\"" + a.Length + 
             "\" Bytes  IsReadOnly=\"" + a.IsReadOnly + "\"  Atributes =\"" + a.Attributes + "\"");
            }
            return ls;
        }
    }
}
