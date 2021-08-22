using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;
namespace ImageViewer
{
    public class NodeInfo
    {
        static int UID = 0;
        public int Id { get; set; }
        public List<int> Links;

        public NodeInfo()
        {
            Id = UID++;
            Links = new List<int>();
        }
    }
    [Serializable]
    public class Node
    {
        public NodeInfo Info { get; set; } = new NodeInfo();
        public float Radius { get; set; }
        public Point position;
        public string Text { get; set; }
        public List<Node> Links { get; set; } = new List<Node>();
        public Node(float r, Point pos, string text="")
        {
            Radius = r;
            position = pos;
            Text = text;
        }
        public Node()
        {
            Radius = 5;
            position = new Point(0, 0);
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            var node = doc.CreateElement("Node");
            node.SetAttribute("Id", $"{Info.Id}");
            node.SetAttribute("Radius", $"{Radius}");
            var pos = doc.CreateElement("Position");
            pos.SetAttribute("X", $"{position.X}");
            pos.SetAttribute("Y", $"{position.Y}");
            node.AppendChild(pos);
            node.SetAttribute("Text", $"{Text}");
            if (Links.Count() != 0)
            {
                var linksnode = doc.CreateElement("Links");
                foreach(var l in Links)
                {
                    var linkn = doc.CreateElement("Link");
                    linkn.SetAttribute("Id", $"{l.Info.Id}");
                    linksnode.AppendChild(linkn);
                }
                node.AppendChild(linksnode);
            }
            return node;
        }

        public static Node FromXmlNode(XmlElement root)
        {
            Node result = new Node();
            result.Info.Id = int.Parse(root.GetAttribute("Id"));
            result.Radius = int.Parse(root.GetAttribute("Radius"));
            result.Text = root.GetAttribute("Text");
            foreach (XmlElement element in root.ChildNodes)
            {
                if(element.Name == "Position")
                {
                    result.position.X = int.Parse(element.GetAttribute("X"));
                    result.position.Y = int.Parse(element.GetAttribute("Y"));
                }
                if(element.Name == "Links")
                {
                    foreach(XmlElement link in element.ChildNodes)
                    {
                        var obj = int.Parse(link.GetAttribute("Id"));
                        result.Info.Links.Add(obj);
                    }
                }
            }
            return result;
        }
    }
}
