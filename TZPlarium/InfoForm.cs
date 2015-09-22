using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TZPlarium
{
    public partial class InfoForm : Form
    {
        List<Label> LL;
        public InfoForm()
        {
            InitializeComponent();
            LL = new List<Label>(); 
        }
        public void SetElement(string s)
        {
            int i = 22;
            string[] c = { "  " };
            List<string> ls=s.Split(c,StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string a in ls)
            {
                LL.Add(new Label());
                LL[LL.Count - 1].AutoSize = true;
                LL[LL.Count - 1].Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                LL[LL.Count - 1].Location = new System.Drawing.Point(15, 22+i);
                LL[LL.Count - 1].Name = "label1";
                LL[LL.Count - 1].Text = a;
                Controls.Add(LL[LL.Count-1]);
                i += 20;
                if (LL[LL.Count - 1].Width >= this.Width-20)
                {
                    this.Width = LL[LL.Count - 1].Width + 50;
                }
            }
        }
    }
}
