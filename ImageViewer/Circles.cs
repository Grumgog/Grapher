using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ImageViewer
{
    public enum ViewerState { None, AddElement, MoveElement, MoveArea, MakeLink }; 
    public partial class CirclesViewer : UserControl
    {
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<Node> Items { get; set; } = new List<Node>();
        public Point BasePosition = new Point(0, 0);
        public Point SaveBasePosition = new Point(0, 0);
        public Point MousePoint { get; set; }
        public bool DragAndDropMoveFlag = false;
        public Size ViewArea { get; set; }
        public bool InitFlag { get; set; } = true;
        public Node MovingCircle;
        private ViewerState _state;

        private Node link = null;

        public ViewerState State
        {
            get 
            {
                return _state;
            } 
            set
            {
                _state = value;
                if(_state == ViewerState.MakeLink)
                {
                    Cursor = System.Windows.Forms.Cursors.UpArrow;
                }
                else
                {
                    Cursor = System.Windows.Forms.Cursors.Arrow;
                }
            }
        }

       
        public CirclesViewer()
        {
            InitializeComponent();
            MenuItem addMenuItem = new MenuItem("Добавить элемент", new EventHandler(AddElement));
            MenuItem DeleteMenuItem = new MenuItem("Удалить", new EventHandler(DeleteElement));
            this.ContextMenu = new ContextMenu(new MenuItem[] { addMenuItem, DeleteMenuItem });
            this.DoubleBuffered = true;

        }

        private void AddElement(object sender, EventArgs e)
        {
            Items.Add(new Node(15, new Point(
                MousePoint.X - BasePosition.X, 
                MousePoint.Y - BasePosition.Y)));
            Redraw();
        }

        private void DeleteElement(object sender, EventArgs e)
        {
            var removed = FastInBorder(Items, MousePoint);
            Items.Remove(removed);
            foreach(var item in Items)
            {
                foreach(var link in item.Links)
                {
                    item.Links.RemoveAll((el)=>el == removed);
                    break;
                }
            }
            Redraw();
        }

        private void Circles_Load(object sender, EventArgs e)
        {

        }

        private void Redraw()
        {
            this.CreateGraphics().Clear(Color.WhiteSmoke); // color?
            OnPaint(new PaintEventArgs(this.CreateGraphics(), new Rectangle(0, 0, this.Size.Width, this.Size.Height)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // draw link
            foreach(Node item in Items)
            {
                DrawLinks(e.Graphics, item);
            }
            //draw items
            foreach(Node item in Items)
            {
                DrawItem(e.Graphics, item);
            }
            DrawDebug(e.Graphics);
        }

        private void DrawLinks(Graphics g, Node item)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var position = new PointF(item.position.X - item.Radius, item.position.Y - item.Radius);
            position.X += BasePosition.X;
            position.Y += BasePosition.Y;

            position.X += item.Radius;
            position.Y += item.Radius;
            foreach (var link in item.Links)
            {
                var pos = link.position;
                pos.X += BasePosition.X;
                pos.Y += BasePosition.Y;
                g.DrawLine(new Pen(Color.Black, 3), position, pos);
            }
        }

        private void DrawDebug(Graphics g)
        {   
            #if DEBUG  
            Brush b = new SolidBrush(Color.Red);
            g.DrawString($"BasePosition: ({BasePosition.X},{BasePosition.Y})\n" +
                $"MousePoint: ({MousePoint.X},{MousePoint.Y})\n" +
                $"MouseLocalPostion: ({MousePosition.X - BasePosition.X},{MousePosition.Y - BasePosition.Y})\n",
                this.Font, b, new PointF(0, 0));
            #endif
        }

        private void DrawItem(Graphics g, Node item)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var position = new PointF(item.position.X - item.Radius, item.position.Y - item.Radius);
            position.X += BasePosition.X;
            position.Y += BasePosition.Y;
            var rect = new RectangleF(position, new SizeF(item.Radius*2, item.Radius*2));
            

            position.X += item.Radius;
            position.Y += item.Radius;
            
            g.FillEllipse(new SolidBrush(Color.Yellow), rect);
            g.DrawEllipse(new Pen(Color.Black, 3), rect);
            position.X -= item.Radius + g.MeasureString(item.Text, Font).Width / 2;
            position.Y -= item.Radius;
            g.DrawString(item.Text, this.Font, new SolidBrush(Color.Red), position);
            
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            if(se.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                VerticalScroll.Value = se.NewValue;
                PerformLayout();
            }
        }


        /// Drag and drop Item
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.Button == MouseButtons.Middle)
            {
                DragAndDropMoveFlag = true;
                MousePoint = e.Location;
                SaveBasePosition = BasePosition;
            }      
            if(e.Button == MouseButtons.Left && State == ViewerState.None)
            {
                MovingCircle = FastInBorder(Items, e.Location);      
            }
            if(e.Button == MouseButtons.Right)
            {
                MousePoint = e.Location;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left && State == ViewerState.MakeLink)
            {
                if(link == null)
                {
                    link = FastInBorder(Items, e.Location);
                }
                else if(link != null)
                {
                    // do work
                    var slink = FastInBorder(Items, e.Location);
                    if(slink != null && link != null)
                    {
                        link.Links.Add(slink);
                        slink.Links.Add(link);
                    }
                    //
                    link = null;
                    //State = ViewerState.None;
                    Redraw();
                }
            }
        }

        private Node FastInBorder(List<Node> items, Point position)
        {
            //// not right
            foreach(var item in items)
            {
                var X = item.position.X - position.X + BasePosition.X;
                var Y = item.position.Y - position.Y + BasePosition.Y;
                var itemq = X * X + Y * Y;
                if (itemq <= (item.Radius * item.Radius))
                    return item;
            }
            return null;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if(e.Button == MouseButtons.Middle)
            {
                DragAndDropMoveFlag = false;
                SaveBasePosition = BasePosition;
            }  
            if(e.Button == MouseButtons.Left)
            {
                MovingCircle = null;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (DragAndDropMoveFlag)
            {
                BasePosition.X = SaveBasePosition.X - (MousePoint.X - e.Location.X);
                BasePosition.Y = SaveBasePosition.Y - (MousePoint.Y - e.Location.Y);
                Refresh();

            }
            if (MovingCircle != null)
            {
                MovingCircle.position.X = e.Location.X - BasePosition.X;
                MovingCircle.position.Y = e.Location.Y - BasePosition.Y;
                Refresh();
            }
           
        }
        /// End Drag and Drop Item
        public XmlNode ToXmlNode(XmlDocument doc)
        {
            var Enode = doc.CreateElement("ViewerEnvironment");
            var BP = doc.CreateElement("BasePosition");
            BP.SetAttribute("X", $"{BasePosition.X}");
            BP.SetAttribute("Y", $"{BasePosition.Y}");
            Enode.AppendChild(BP);
            var Dnode = doc.CreateElement("Data");
            foreach(var node in Items)
            {
                Dnode.AppendChild(node.ToXmlNode(doc));
            }
            var root = doc.CreateElement("Viewer");
            root.AppendChild(Enode);
            root.AppendChild(Dnode);
            return root;
        }

        public void LoadFromXml(XmlNode doc)
        {
            Items.Clear();
            BasePosition = new Point(0, 0);
            foreach(XmlNode cnode in doc.LastChild)
            {
                if(cnode.Name == "ViewerEnvironment")
                {
                    foreach(XmlElement VEnode in cnode.ChildNodes)
                    {
                        if(VEnode.Name == "BasePosition")
                        {
                            BasePosition.X = int.Parse(VEnode.GetAttribute("X"));
                            BasePosition.Y = int.Parse(VEnode.GetAttribute("Y"));
                        }
                    }   
                }
                if(cnode.Name == "Data")
                {
                    foreach(XmlElement NodeData in cnode.ChildNodes)
                    {
                        Items.Add(Node.FromXmlNode(NodeData));
                    }
                    // rebuild links!
                    foreach(var item in Items)
                    {
                        if (item.Info.Links.Count != 0)
                        {
                            foreach(int id in item.Info.Links)
                            {
                                item.Links.Add(Items.Find((el) => el.Info.Id == id));
                            }
                        }
                    }
                }
            }
            Redraw();
        }
    }
}
