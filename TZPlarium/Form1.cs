using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Xml;

namespace TZPlarium
{
    public partial class Form1 : Form
    {
        private List<Directory> ListDirectory;
        private ManualResetEvent waitHandle;
        private Thread ScanThread;
        private Thread PrintThread;
        private Thread PrintXmlThread;
        private int IndexForPrint;
        private int IndexForXmlPrint;
        private int MinIndexPrint;
        private int MinIndexXmlPrint;
        private string StartDirectoryPath;
        private string XmlFilePath;
        private object MyLock;
       
        public Form1()
        {
            InitializeComponent();
            MyLock = new object();
            ListDirectory = new List<Directory>();
            ScanThread = new Thread(Scan);
            PrintThread = new Thread(Print);
            PrintXmlThread = new Thread(XmlPrint);
            IndexForPrint = 0;
            IndexForXmlPrint = 0;
            MinIndexPrint = 0;
            MinIndexXmlPrint = 0;
            StartDirectoryPath = "";
            XmlFilePath = "";
            waitHandle = new ManualResetEvent(false);
        }

        private XmlDocument CreateXmlFile(string fullName)
        {
            XmlTextWriter textWritter = new XmlTextWriter(fullName, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("head");
            textWritter.WriteEndElement();
            textWritter.Close();

            return null;
        }

        private void SetDirectoryAtribute(XmlDocument xd, XmlNode xn, Directory dir)
        {
            //По каждой поддиректории и файлу, в XML файле должно быть указано имя, дата создания, дата модификации, дата последнего доступа, атрибуты, 
            //размер (только для файлов; реализация размера для директорий будет засчитана как плюс), 
            //владелец, а также допустимые права (запись, чтение, удаление и т.п.) для текущего пользователя.

            XmlAttribute attribute = xd.CreateAttribute("Name"); 
            attribute.Value = dir.DI.Name; 
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("CreationTime"); 
            attribute.Value = "" + dir.DI.CreationTime; 
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("LastWriteTime");
            attribute.Value = "" + dir.DI.LastWriteTime;
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("LastAccessTime");
            attribute.Value = "" + dir.DI.LastAccessTime;
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("Atributes");
            attribute.Value = "" + dir.DI.Attributes;
            xn.Attributes.Append(attribute);
        }

        private void SetFileAtribute(XmlDocument xd, XmlNode xn, FileInfo fileInfo)
        {
            XmlAttribute attribute = xd.CreateAttribute("Name"); // создаём атрибут
            attribute.Value = fileInfo.Name; // устанавливаем значение атрибута
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("CreationTime"); // создаём атрибут
            attribute.Value = "" + fileInfo.CreationTime; // устанавливаем значение атрибута
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("LastWriteTime");
            attribute.Value = "" + fileInfo.LastWriteTime;
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("LastAccessTime");
            attribute.Value = "" + fileInfo.LastAccessTime;
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("Atributes");
            attribute.Value = "" + fileInfo.Attributes;
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("Size");
            attribute.Value = "" + fileInfo.Length + " Byte";
            xn.Attributes.Append(attribute);

            attribute = xd.CreateAttribute("IsReadOnly");
            attribute.Value = "" + fileInfo.IsReadOnly;
            xn.Attributes.Append(attribute);
        }

        public void Scan()
        {
            Scan(StartDirectoryPath, 0);
        }

        public void Scan(string path, int ind)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            List<FileInfo> ListFI;
            List<DirectoryInfo> ListDI;
            try
            {
                ListFI = directory.GetFiles().ToList();
                ListDI = directory.GetDirectories().ToList();
            }
            catch (System.UnauthorizedAccessException)
            {
                return;
            }

            lock (MyLock)
            {
                ListDirectory.Add(new Directory(directory, ListFI, ind));
            }
            waitHandle.Set();
            directory = null;
            foreach (DirectoryInfo a in ListDI)
            {
                Scan(a.FullName, ind + 1);
            }
        }

        private delegate void AddTextDel(string s);
        private void AddText(string s)
        {
            listBox1.Items.Add(s);
        }

        private delegate void AddListDel(Directory d);
        void AddList(Directory d)
        {
            listBox1.Items.AddRange(d.ToListString().ToArray());
        }

        public void Print()
        {
            string s = "";
            while (true)
            {
                for (; IndexForPrint < ListDirectory.Count; ++IndexForPrint)
                {
                    for (int i = 0; i < ListDirectory[IndexForPrint].Index; ++i)
                    {
                        s += "  ";
                    }
                    if (listBox1.InvokeRequired)
                    {
                        listBox1.Invoke(new AddTextDel(AddText), s + ListDirectory[IndexForPrint].ToString());
                        listBox1.Invoke(new AddListDel(AddList), new Directory[] { ListDirectory[IndexForPrint] });
                    }
                    s = "";
                }
                //Очищение памяти
                MinIndexPrint = IndexForPrint - 1;
                if (MinIndexPrint < MinIndexXmlPrint && MinIndexPrint > 200)
                {
                    lock (MyLock)
                    {
                        ListDirectory.RemoveRange(0, MinIndexPrint);
                        IndexForPrint = 0;
                    }
                }
                if (ScanThread.ThreadState == ThreadState.Stopped && IndexForPrint >= ListDirectory.Count)
                {
                    break;
                }

                if (ScanThread.ThreadState == ThreadState.Running)
                {
                    waitHandle.WaitOne(40);
                }
            }
        }


        public void XmlPrint()
        {
            XmlDocument XmlDocument = new XmlDocument();
            XmlNode xmlNodeFile;
            XmlNode xmlNodeDirectory = null;
            List<XmlNode> ListNode = new List<XmlNode>();
            List<Directory> listDirectory = new List<Directory>();
            string path = XmlFilePath;
            CreateXmlFile(path);
            XmlDocument.Load(path);
            System.IO.FileStream file = System.IO.File.Create(path);

            int m = 0;
            while (true)
            {
                for (; IndexForXmlPrint < ListDirectory.Count; ++IndexForXmlPrint)
                {
                    //Определение вложености директории
                    while (true)
                    {
                        m = ListNode.Count - 1;
                        if (m >= 0 && listDirectory[m].Index < ListDirectory[IndexForXmlPrint].Index)
                        {
                            xmlNodeDirectory = XmlDocument.CreateElement("Directory");
                            ListNode[m].AppendChild(xmlNodeDirectory);
                            ListNode.Add(xmlNodeDirectory);
                            listDirectory.Add(ListDirectory[IndexForXmlPrint]);
                            break;
                        }
                        else
                        {
                            if (m >= 0 && listDirectory[m].Index == ListDirectory[IndexForXmlPrint].Index)
                            {
                                xmlNodeDirectory = XmlDocument.CreateElement("Directory");
                                ListNode[ListNode.Count - 2].AppendChild(xmlNodeDirectory);
                                ListNode[m] = xmlNodeDirectory;
                                listDirectory[m] = ListDirectory[IndexForXmlPrint];
                                break;
                            }
                            else
                            {
                                if (IndexForXmlPrint == 0)
                                {
                                    xmlNodeDirectory = XmlDocument.CreateElement("Directory");
                                    XmlDocument.DocumentElement.AppendChild(xmlNodeDirectory);
                                    ListNode.Add(xmlNodeDirectory);
                                    listDirectory.Add(ListDirectory[IndexForXmlPrint]);
                                    break;
                                }
                                ListNode.RemoveAt(m);
                                listDirectory.RemoveAt(m);
                            }
                        }
                    }
                    SetDirectoryAtribute(XmlDocument, xmlNodeDirectory, ListDirectory[IndexForXmlPrint]);

                    foreach (FileInfo a in ListDirectory[IndexForXmlPrint].LF)
                    {
                        xmlNodeFile = XmlDocument.CreateElement("File");
                        xmlNodeDirectory.AppendChild(xmlNodeFile);
                        SetFileAtribute(XmlDocument, xmlNodeFile, a);
                    }
                }

                //Очищение памяти
                MinIndexXmlPrint = IndexForPrint - 1;
                if (MinIndexXmlPrint < MinIndexPrint && MinIndexXmlPrint > 200)
                {
                    lock (MyLock)
                    {
                        ListDirectory.RemoveRange(0, MinIndexXmlPrint);
                        IndexForXmlPrint = 0;
                    }
                }

                if (ScanThread.ThreadState == ThreadState.Stopped && IndexForXmlPrint >= ListDirectory.Count)
                {
                    break;
                }
                //Даем потоку сканирования фору
                if (ScanThread.ThreadState == ThreadState.Running)
                {
                    waitHandle.WaitOne(40);
                }
            }
            XmlDocument.Save(file);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ScanThread.ThreadState == ThreadState.Running || PrintThread.ThreadState == ThreadState.Running || PrintXmlThread.ThreadState == ThreadState.Running)
            {
                if (MessageBox.Show("Stop the scan?\nThe data will be lost!", "Attention!", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    ScanThread.Abort();
                    PrintThread.Abort();
                    PrintXmlThread.Abort();
                }
            }
            else
            {
                if (MessageBox.Show("Run the scan?", "Attention!", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    ScanThread = new Thread(Scan);
                    PrintThread = new Thread(Print);
                    PrintXmlThread = new Thread(XmlPrint);
                    ListDirectory = new List<Directory>();
                    IndexForPrint = 0;
                    IndexForXmlPrint = 0;
                    MinIndexPrint = 0;
                    MinIndexXmlPrint = 0;
                    listBox1.Items.Clear();

                    if (StartDirectoryPath != "" && StartDirectoryPath != "")
                    {
                        timer1.Start();
                        ScanThread.Start();
                        PrintThread.Start();
                        PrintXmlThread.Start();
                    }
                    else
                    {
                        MessageBox.Show("Set folder", "Attention!");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            StartDirectoryPath = folderBrowserDialog1.SelectedPath;
            label1.Text = "Selected path " + StartDirectoryPath;

            saveFileDialog1.ShowDialog();
            XmlFilePath = saveFileDialog1.FileName;
            label2.Text = "Create XML file " + XmlFilePath;
        }

        private Color ThreadStateInColor(Thread t)
        {
            if (t.ThreadState == ThreadState.Running)
            {
                return Color.Green;
            }
            if (t.ThreadState == ThreadState.WaitSleepJoin)
            {
                return Color.Yellow;
            }
            return Color.Red;
        }

        //Визуализация состояния потоков
        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.BackColor = ThreadStateInColor(ScanThread);
            label4.BackColor = ThreadStateInColor(PrintThread);
            label5.BackColor = ThreadStateInColor(PrintXmlThread);
            if (ScanThread.ThreadState == ThreadState.Stopped && PrintThread.ThreadState == ThreadState.Stopped && PrintXmlThread.ThreadState == ThreadState.Stopped)
            {
                timer1.Stop();
                MessageBox.Show("scan was completed.", "Attention!", MessageBoxButtons.OK);
            }
        }

        //Вывод полной информации о файле или каталоге
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            InfoForm inF = new InfoForm();
            inF.SetElement("" + listBox1.Items[listBox1.SelectedIndex]);
            inF.ShowDialog();
        }
    }
}
