using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ImageViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #if DEBUG
            var first = new Node(20, new Point(50, 40), "Hello");
            var second = new Node(15, new Point(70, 60), "H");
            var third = new Node(5, new Point(10, 10));
            var fourth = new Node(10, new Point(15, 20));
            first.Links.Add(second);
            third.Links.Add(first);
            third.Links.Add(second);
            third.Links.Add(fourth);
            fourth.Links.Add(first);
            circlesViewer1.Items.Add(third);
            circlesViewer1.Items.Add(fourth);
            circlesViewer1.Items.Add(first);
            circlesViewer1.Items.Add(second);
            #endif
        }

        private void circlesViewer1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // circlesViewer1.State = ImageViewer.ViewerState.AddElement;
        }

        private void LinkModeToolStrip_Click(object sender, EventArgs e)
        {
            if (LinkModeToolStrip.Checked)
            {
                circlesViewer1.State = ViewerState.MakeLink;
            }
            else
            {
                circlesViewer1.State = ViewerState.None;
            }
            
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                var root = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(root);

                doc.AppendChild(circlesViewer1.ToXmlNode(doc));
                doc.Save(dialog.FileName);
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            if(openfile.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(openfile.FileName);
                circlesViewer1.LoadFromXml(doc);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var res = MessageBox.Show("Вы хотите сохранить результаты работы?", "Сообщение", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if(res == DialogResult.Yes)
            {
                SaveFileDialog savefile = new SaveFileDialog();
                if(savefile.ShowDialog() == DialogResult.Yes)
                {
                    XmlDocument doc = new XmlDocument();
                    var root = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                    doc.AppendChild(root);

                    doc.AppendChild(circlesViewer1.ToXmlNode(doc));
                    doc.Save(savefile.FileName);
                }
                else
                {
                    e.Cancel = true;
                }
                
            }
        }
    }
}
