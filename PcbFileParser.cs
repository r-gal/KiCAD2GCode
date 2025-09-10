using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace KiCad2Gcode
{

    internal class PcbFileElement
    {
        internal int stopIdx; /* position of closing bracket */

        internal string name;
        internal string values;
        internal List<PcbFileElement> children;

        internal PcbFileElement()
        {
            this.children = new List<PcbFileElement>();
        }

        internal PcbFileElement FindElement(string name_)
        {
            foreach (PcbFileElement element in children)
            {
                if (element.name != null && element.name == name_ && element.values != null)
                {
                    return element;
                }
            }
            return null;
        }

        internal double[] ParseParameterNumericArr(string parName, int min, int max)
        {
            PcbFileElement element = FindElement(parName);

            double[] result = null;

            if (element == null)
            {
                return null;
            }
            else
            {
                element.values = element.values.Replace('.', ',');
                string[] valStr = element.values.Split(' ');
                if (valStr.Length >= min && valStr.Length <= max)
                {
                    result = new double[valStr.Length];
                    try
                    {
                        for(int i = 0;i<valStr.Length;i++)
                        {
                            result[i] = double.Parse(valStr[i]);
                        }
                    }
                    catch { return null; }
                }
                else { return null; }
            }
            return result;
        }

        internal double ParseParameterNumeric(string parName)
        {
            PcbFileElement element = FindElement(parName);

            double result = Double.NaN;

            if (element == null)
            {
                return Double.NaN;
            }
            else
            {
                element.values = element.values.Replace('.', ',');
                try
                {
                    result = double.Parse(element.values);
                }
                catch { return Double.NaN; }
            }
            return result;
        }

        internal int ParseNet()
        {

            PcbFileElement element = FindElement("net");
            int result = 0;

            if (element == null)
            {
                return 0;
            }
            else
            {
                string[] valStr = element.values.Split(' ');
                if (valStr.Length > 0 )
                {                     
                    try
                    {
                        
                        result = int.Parse(valStr[0]);
                        
                    }
                    catch { return 0; }
                }
                else { return 0; }
            }
            return result;
        }

        internal bool CheckLayer(string layer)
        {

            PcbFileElement element = FindElement("layers");

            if (element == null)
            {
                element = FindElement("layer");
            }
            if (element == null)
            {
                return false;
            }
            else
            {
                if (element.values.Contains(layer))
                {
                    return true;
                }
                else if (element.values.Contains("*.Cu"))
                {
                    return true;
                }
            }
            return false;
        }
    }
    internal class PcbFileParser
    {
        MainUnit mainUnit;

        PcbFileElement mainElement;
        string fileText;

        string activeLayer = "F.Cu";
        string cutLayer = "Edge.Cuts";


        public PcbFileParser(MainUnit mainUnit_) { mainUnit = mainUnit_; }




        private void DecodeElement(PcbFileElement element)
        {
            if(element.name != null)
            {
                if(element.name == "footprint")
                {
                    mainUnit.PrintText(element.name);
                    mainUnit.PrintText("\n");
                }




            }
            
        }

        private void DecodeFootprint(PcbFileElement footprint)
        {
            double posRot = 0;

            double[] pos = footprint.ParseParameterNumericArr("at", 2, 3);
            if (pos == null) { return; }

            if(pos.Length == 3)
            {
                posRot = pos[2];
            }

            foreach(PcbFileElement e in footprint.children)
            {
                if((e.name != null) && (e.name == "pad"))
                {
                    DecodePad(e, pos[0], -pos[1], -posRot);
                }
            }

        }

 


        private void DecodePad(PcbFileElement pad, double offsetX, double offsetY, double offsetRot)
        {
            if ((pad.name != null) && (pad.name == "pad") && (pad.values != null))
            {

                /* check if correct layer */

                bool layerOk = pad.CheckLayer(activeLayer);
                  
                if(layerOk == false)
                {
                    return;
                }

                double posRot = 0;

                double[] pos = pad.ParseParameterNumericArr("at", 2, 3);
                if (pos == null) { return; }

                if (pos.Length == 3)
                {
                    posRot = pos[2];
                }

                double[] size = pad.ParseParameterNumericArr("size",2,2);
                if (size == null) { return; }

                Point2D posPt = new Point2D(pos[0], -pos[1]);
                posPt.Rotate(offsetRot * Math.PI / 180);                

                posPt.x += offsetX;
                posPt.y += offsetY;




                if (pad.values.Contains("thru_hole"))
                {
                    /*add hole */
                    double drill = pad.ParseParameterNumeric("drill");
                    if (drill == Double.NaN) { return; }

                    Drill d = new Drill();
                    d.diameter = drill;
                    d.pos = posPt;
                    mainUnit.AddDrill(d);
                }

                if(pad.values.Contains("roundrect"))
                {

                    bool[] chamfer = { false, false, false, false };

                    double roundRatio = pad.ParseParameterNumeric("roundrect_rratio");
                    if(roundRatio == Double.NaN) { return; }

                    double chamferRatio = pad.ParseParameterNumeric("chamfer_ratio");
                    if (chamferRatio == Double.NaN) { return; }

                    PcbFileElement element = pad.FindElement("chamfer");
                    if (element != null)
                    {
                        chamfer[0] = element.values.Contains("top_left");
                        chamfer[1] = element.values.Contains("top_right");                        
                        chamfer[2] = element.values.Contains("bottom_right");
                        chamfer[3] = element.values.Contains("bottom_left");
                    }

                    Figure f = new Figure();
                    f.name = "roundrect at " + offsetX.ToString(CultureInfo.InvariantCulture) + " -" + offsetY.ToString(CultureInfo.InvariantCulture);
                    f.net = pad.ParseNet();

                    Arc arc;

                    Node node;
                    LinkedListNode<Node> lln;



                    double[] chamferSize = { 0, 0, 0, 0 };
                    bool[] chamferRounded = {false,false,false,false}; 

                    for(int i=0;i<4;i++)
                    {
                        if (chamfer[i] && (chamferRatio > roundRatio))
                        {
                            chamferSize[i] = chamferRatio;
                        }
                        else if(roundRatio > 0)
                        {
                            chamferSize[i] = roundRatio;
                            chamferRounded[i] = true;
                        }

                    }

                    double xl = size[0] / 2;
                    double yl = size[1] / 2;

                    Point2D[] pts = new Point2D[8];
                    Point2D[] ptsc = new Point2D[4];

                    pts[0] = new Point2D(-xl, yl - chamferSize[0]);
                    pts[1] = new Point2D(-xl + chamferSize[0], yl );
                    pts[2] = new Point2D( xl - chamferSize[1], yl );
                    pts[3] = new Point2D( xl, yl - chamferSize[1]);
                    pts[4] = new Point2D( xl, -yl + chamferSize[2]);
                    pts[5] = new Point2D( xl - chamferSize[2], -yl);
                    pts[6] = new Point2D(-xl + chamferSize[3], -yl);
                    pts[7] = new Point2D(-xl, -yl + chamferSize[3]);

                    ptsc[0] = new Point2D(-xl + chamferSize[0], yl - chamferSize[0]);
                    ptsc[1] = new Point2D( xl - chamferSize[1], yl - chamferSize[1]);
                    ptsc[2] = new Point2D( xl - chamferSize[2], -yl + chamferSize[2]);
                    ptsc[3] = new Point2D(-xl + chamferSize[3], -yl + chamferSize[3]);

                    double[] startAngles = { Math.PI , Math.PI / 2,0, -Math.PI / 2 };
                    double[] endAngles = { Math.PI/2, 0, -Math.PI / 2 , -Math.PI };



                    for (int i=0;i< 4;i++)
                    {
                        node = new Node();
                        node.pt = pts[2*i];
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        if (chamferSize[i] > 0)
                        {
                            node = new Node();
                            node.pt = node.pt = pts[2 * i + 1];
                            lln = new LinkedListNode<Node>(node);                            

                            if (chamferRounded[i])
                            {
                                arc = new Arc();
                                arc.centre = ptsc[i];
                                arc.radius = chamferSize[i];
                                arc.startAngle = startAngles[i];
                                arc.endAngle = endAngles[i];
                                node.arc = arc;
                            }
                            f.shape.points.AddLast(lln);
                        }
                    }



                    f.Rotate(posRot * Math.PI / 180);
                    f.Move(posPt.ToVector());

                    mainUnit.AddFigure(f);

                    //mainUnit.PrintText("PAD TH ROUNDRECT " + "\n");

                }
                else if(pad.values.Contains("rect"))
                {
                    Figure f = new Figure();
                    f.name = "rect at " + offsetX.ToString(CultureInfo.InvariantCulture) + " -" + offsetY.ToString(CultureInfo.InvariantCulture);
                    f.net = pad.ParseNet();

                    Node node;
                    LinkedListNode<Node> lln;

                    Point2D p1 = new Point2D(-size[0] / 2, size[1] / 2);
                    Point2D p2 = new Point2D( size[0] / 2, size[1] / 2);
                    Point2D p3 = new Point2D( size[0] / 2,-size[1] / 2);
                    Point2D p4 = new Point2D(-size[0] / 2,-size[1] / 2);

                    node = new Node();
                    node.pt = p1;
                    lln = new LinkedListNode<Node>(node);
                    f.shape.points.AddLast(lln);
                    node = new Node();
                    node.pt = p2;
                    lln = new LinkedListNode<Node>(node);
                    f.shape.points.AddLast(lln);
                    node = new Node();
                    node.pt = p3;
                    lln = new LinkedListNode<Node>(node);
                    f.shape.points.AddLast(lln);
                    node = new Node();
                    node.pt = p4;
                    lln = new LinkedListNode<Node>(node);
                    f.shape.points.AddLast(lln);
                    
                    f.Rotate(posRot * Math.PI / 180);
                    f.Move(posPt.ToVector());

                    mainUnit.AddFigure(f);

                    //mainUnit.PrintText("PAD TH RECT " + "\n");
                }
                else if (pad.values.Contains("circle"))
                {
                    Figure f = new Figure();
                    Arc arc = new Arc();

                    f.name = "circle at " + offsetX.ToString(CultureInfo.InvariantCulture) + " -" + offsetY.ToString(CultureInfo.InvariantCulture);
                    f.net = pad.ParseNet();

                    Node node;
                    LinkedListNode<Node> lln;

                    Point2D p1 = new Point2D(-size[0] / 2,0);

                    arc.centre = new Point2D(0, 0);
                    arc.startAngle = Math.PI;
                    arc.endAngle = -Math.PI;
                    arc.radius = size[0] / 2;

                    node = new Node();
                    node.pt = p1;
                    node.arc = arc;
                    lln = new LinkedListNode<Node>(node);
                    f.shape.points.AddLast(lln);

                    f.Move(posPt.ToVector());

                    mainUnit.AddFigure(f);

                    //mainUnit.PrintText("PAD TH CIRCLE " + arc.centre.x.ToString() + " " + arc.centre.y.ToString() + " " + arc.radius.ToString() + "\n");
                }
                else if (pad.values.Contains("oval"))
                {
                    Figure f = new Figure();

                    f.name = "oval at " + offsetX.ToString(CultureInfo.InvariantCulture) + " -" + offsetY.ToString(CultureInfo.InvariantCulture);
                    f.net = pad.ParseNet();

                    Arc arc;

                    Node node;
                    LinkedListNode<Node> lln;

                    if (size[0] >= size[1])
                    {
                        /*horizontal */
                        double r = size[1] / 2;
                        double l = size[0] - size[1];

                        Point2D p1 = new Point2D(-l / 2, size[1] / 2);
                        Point2D p2 = new Point2D(l / 2, size[1] / 2);
                        Point2D p3 = new Point2D(l / 2, -size[1] / 2);
                        Point2D p4 = new Point2D(-l / 2, -size[1] / 2);
                        Point2D pc1 = new Point2D(-l / 2, 0);
                        Point2D pc2 = new Point2D(l / 2, 0);

                        arc = new Arc();
                        arc.centre = pc1;
                        arc.startAngle = -Math.PI / 2;
                        arc.endAngle = Math.PI / 2;
                        arc.radius = r;
                        node = new Node();
                        node.pt = p1;
                        node.arc = arc;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        node = new Node();
                        node.pt = p2;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        arc = new Arc();
                        arc.centre = pc2;
                        arc.startAngle = Math.PI / 2;
                        arc.endAngle = -Math.PI / 2;
                        arc.radius = r;
                        node = new Node();
                        node.pt = p3;
                        node.arc = arc;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        node = new Node();
                        node.pt = p4;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);
                    }
                    else
                    {
                        /*vertical*/
                        double r = size[0] / 2;
                        double l = size[1] - size[0];

                        Point2D p1 = new Point2D(r ,  l / 2 );
                        Point2D p2 = new Point2D(r , -l / 2 );
                        Point2D p3 = new Point2D(-r, -l / 2 );
                        Point2D p4 = new Point2D(-r ,  l / 2 );
                        Point2D pc1 = new Point2D( 0, l / 2);
                        Point2D pc2 = new Point2D( 0, - l / 2);

                        arc = new Arc();
                        arc.centre = pc1;
                        arc.startAngle = Math.PI ;
                        arc.endAngle = 0;
                        arc.radius = r;
                        node = new Node();
                        node.pt = p1;
                        node.arc = arc;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        node = new Node();
                        node.pt = p2;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        arc = new Arc();
                        arc.centre = pc2;
                        arc.startAngle = 0;
                        arc.endAngle = -Math.PI;
                        arc.radius = r;
                        node = new Node();
                        node.pt = p3;
                        node.arc = arc;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        node = new Node();
                        node.pt = p4;
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);
                    }



                    f.Rotate(posRot * Math.PI / 180 );
                    f.Move(posPt.ToVector());

                    mainUnit.AddFigure(f);

                    //mainUnit.PrintText("PAD TH OVAL " + "\n");
                }

            }
        }

        private void DecodeVia(PcbFileElement via)
        {

            if ((via.name != null) && (via.name == "via"))
            {
                //mainUnit.PrintText("VIA");
                //mainUnit.PrintText("\n");

                bool layerOk = via.CheckLayer(activeLayer);

                if (layerOk == false)
                {
                    return;
                }

                double[] pos = via.ParseParameterNumericArr("at", 2, 2);
                if (pos == null) { return; }

                double size = via.ParseParameterNumeric("size");
                if (size == Double.NaN) { return; }

                double drill = via.ParseParameterNumeric("drill");
                if (drill == Double.NaN) { return; }

                Figure f = new Figure();
                Arc arc = new Arc();

                f.name = "via at " + pos[0].ToString(CultureInfo.InvariantCulture) + " -" + pos[1].ToString(CultureInfo.InvariantCulture);
                f.net = via.ParseNet();
                Node node;
                LinkedListNode<Node> lln;

                Point2D p1 = new Point2D(-size / 2, 0);

                arc.centre = new Point2D(0, 0);
                arc.startAngle = Math.PI;
                arc.endAngle = -Math.PI;
                arc.radius = size / 2;

                node = new Node();
                node.pt = p1;
                node.arc = arc;
                lln = new LinkedListNode<Node>(node);
                f.shape.points.AddLast(lln);
                f.Move(new Vector(pos[0], -pos[1]));

                mainUnit.AddFigure(f);

                Drill d = new Drill();
                d.diameter = drill ;
                d.pos = new Point2D(pos[0], -pos[1]);
                mainUnit.AddDrill(d);




            }
        }

        private void DecodeSegment(PcbFileElement seg)
        {
            if ((seg.name != null) && (seg.name == "segment"))
            {
                //mainUnit.PrintText("SEGMENT");
                //mainUnit.PrintText("\n");

                /* check layer */

                bool layerOk = seg.CheckLayer(activeLayer);

                if (layerOk == false)
                {
                    return;
                }

                double[] startArr = seg.ParseParameterNumericArr("start", 2, 2);
                if(startArr == null) { return; }
                double[] endArr = seg.ParseParameterNumericArr("end", 2, 2);
                if (endArr == null) { return; }
                double width = seg.ParseParameterNumeric("width");
                if (width == Double.NaN) { return; }

                startArr[1] *= -1;
                endArr[1] *= -1;

                double dirX = endArr[0] - startArr[0];
                double dirY = endArr[1] - startArr[1];

                double angle = Math.Atan2(dirY, dirX);

                Figure f = new Figure();

                f.name = "seg " + startArr[0].ToString(CultureInfo.InvariantCulture) + " -" + startArr[1].ToString(CultureInfo.InvariantCulture) + " <-> " + 
                    endArr[0].ToString(CultureInfo.InvariantCulture) + " -" + endArr[1].ToString(CultureInfo.InvariantCulture);
                f.net = seg.ParseNet();
                Node node;
                LinkedListNode<Node> lln;

                Point2D p1 = new Point2D(0, -width / 2);
                Point2D p2 = new Point2D(0, width / 2);
                Point2D p3 = new Point2D(0, -width / 2);
                Point2D p4 = new Point2D(0, width / 2);

                p1.Rotate(-angle);
                p2.Rotate(-angle);
                p3.Rotate(-angle);
                p4.Rotate(-angle);

                Vector v1 = new Vector(startArr[0], startArr[1]);
                p1 += v1;
                p2 += v1;
                Vector v2 = new Vector(endArr[0], endArr[1]);
                p3 += v2;
                p4 += v2;

                Arc arc1 = new Arc();
                arc1.centre = new Point2D(0,0);
                arc1.radius = width / 2;
                arc1.startAngle = -Math.PI / 2;
                arc1.endAngle = Math.PI / 2;
                arc1.Rotate(-angle);
                arc1.Move(v1);

                Arc arc2 = new Arc();
                arc2.centre = new Point2D(0,0);
                arc2.radius = width / 2;
                arc2.startAngle = Math.PI / 2;
                arc2.endAngle = -Math.PI / 2;
                arc2.Rotate(-angle);
                arc2.Move(v2);

                node = new Node();
                node.pt = p2;
                node.arc = arc1;
                lln = new LinkedListNode<Node>(node);
                f.shape.points.AddLast(lln);

                node = new Node();
                node.pt = p4;
                lln = new LinkedListNode<Node>(node);
                f.shape.points.AddLast(lln);

                node = new Node();
                node.pt = p3;
                node.arc = arc2;
                lln = new LinkedListNode<Node>(node);
                f.shape.points.AddLast(lln);

                node = new Node();
                node.pt = p1;
                lln = new LinkedListNode<Node>(node);
                f.shape.points.AddLast(lln);

                mainUnit.AddFigure(f);

            }
        }

        private void DecodePolygon(PcbFileElement polygon, int net)
        {
            if ((polygon.name != null) && (polygon.name == "filled_polygon"))
            {
                //mainUnit.PrintText("POLYGON");
                //mainUnit.PrintText("\n");

                bool layerOk = polygon.CheckLayer(activeLayer);

                if (layerOk == false)
                {
                    return;
                }

                PcbFileElement pts = polygon.FindElement("pts");

                Figure f = new Figure();
                f.net = net;

                bool firstFetched = false;
                double firstX = 0;
                double firstY = 0   ;

                double prevX = 0;
                double prevY = 0;

                double x = 0;
                double y = 0;

                Line line;

                foreach (PcbFileElement e in pts.children) 
                {
                    
                    if(e.name == "xy")
                    {
                        e.values = e.values.Replace('.', ',');
                        string[] valStr = e.values.Split(' ');
                        if (valStr.Length== 2)
                        {                            
                            try
                            { 
                                x = double.Parse(valStr[0]);
                                y = -double.Parse(valStr[1]);
                            }
                            catch { return ; }
                        }
                        else { return ; }

                        Node node;
                        LinkedListNode<Node> lln;

                        node = new Node();
                        node.pt = new Point2D(x,y);
                        lln = new LinkedListNode<Node>(node);
                        f.shape.points.AddLast(lln);

                        

                        /*
                        if (firstFetched == false)
                        {
                            firstFetched = true;
                            firstX = x;
                            firstY = y;
                            prevX = x; prevY = y;
                        }
                        else
                        {
                            line = new Line();
                            line.start = new Point2D(prevX, prevY);
                            line.end = new Point2D(x,y);
                            f.chunks.Add(line);
                            prevX = x; prevY = y;
                        }*/



                    }
                }
                /*
                line = new Line();
                line.start = new Point2D(x, y);
                line.end = new Point2D(firstX, firstY);
                f.chunks.Add(line);*/

                ZoneUnit zoneUnit = new ZoneUnit(mainUnit);
                zoneUnit.ConvertToValidFigure(f);

                mainUnit.AddZoneFigure(f);

            }
        }

        private void DecodeLine(PcbFileElement el)
        {
            if ((el.name != null) && (el.name == "gr_line"))
            {

                bool layerOk = el.CheckLayer(cutLayer);

                if (layerOk == false)
                {
                    return;
                }

                double[] startArr = el.ParseParameterNumericArr("start", 2, 2);
                if (startArr == null) { return; }
                double[] endArr = el.ParseParameterNumericArr("end", 2, 2);
                if (endArr == null) { return; }
                
                Node node;

                node = new Node();
                node.startPt = new Point2D(startArr[0], -startArr[1]);
                node.pt = new Point2D(endArr[0], -endArr[1]);


                mainUnit.AddCuts(node);
            }
        }

        private void DecodeCircle(PcbFileElement el)
        {
            if ((el.name != null) && (el.name == "gr_circle"))
            {

                bool layerOk = el.CheckLayer(cutLayer);

                if (layerOk == false)
                {
                    return;
                }

                double[] centerArr = el.ParseParameterNumericArr("center", 2, 2);
                if (centerArr == null) { return; }
                double[] endArr = el.ParseParameterNumericArr("end", 2, 2);
                if (endArr == null) { return; }


                Node node;

                node = new Node();
                node.startPt = new Point2D(endArr[0], -endArr[1]);
                node.pt = new Point2D(endArr[0], -endArr[1]);

                Arc arc = new Arc();
                arc.start = new Point2D(endArr[0], -endArr[1]);
                arc.end = new Point2D(endArr[0], -endArr[1]);
                arc.centre = new Point2D(centerArr[0], -centerArr[1]);
                arc.startAngle = 0;
                arc.endAngle = -2 * Math.PI;
                arc.radius = Math.Sqrt(Math.Pow(centerArr[0] - endArr[0], 2) + Math.Pow(centerArr[1] - endArr[1], 2));

                node.arc = arc;

                mainUnit.AddCuts(node);
            }
        }

        private void DecodeMaster(PcbFileElement el)
        {
            int netCnt = 0;

            foreach (PcbFileElement n in el.children)
            {
                if (n.name == "net")
                {
                    netCnt++;
                }
            }
            mainUnit.InitNetList(netCnt);



        }

        private void DecodeZone(PcbFileElement el)
        {
            int net = el.ParseNet();

            mainUnit.InitZone(net);

            foreach (PcbFileElement fp in el.children)
            {
                if (fp.name == "filled_polygon")
                {
                    DecodePolygon(fp,net);
                }
            }



        }

        private void Decode(PcbFileElement top)
        {
            if(top.name != null)
            {
                if(top.name == "footprint")
                {
                    DecodeFootprint(top);
                }
                else if (top.name == "via")
                {
                    DecodeVia(top);
                }
                else if (top.name == "segment")
                {
                    DecodeSegment(top);
                }
                else if (top.name == "gr_line")
                {
                    DecodeLine(top);
                }
                else if (top.name == "gr_circle")
                {
                    DecodeCircle(top);
                }
                else if (top.name == "kicad_pcb")
                {
                    DecodeMaster(top);
                }
                else if (top.name == "zone")
                {
                    DecodeZone(top);
                }


            }
            foreach (PcbFileElement child in top.children) { Decode(child); }



        }

        private PcbFileElement ParseElement(int startIdx_)
        {
            PcbFileElement element = new PcbFileElement();


            bool nameParsed = false;
            bool valuesParsed = false;
            int startIdx = startIdx_;
            for (int i = startIdx_; i < fileText.Length; i++)
            {
                char ch = fileText[i];

                if(ch == ' ' && nameParsed == false)
                {
                    element.name = fileText.Substring(startIdx, i - startIdx);
                    nameParsed = true;
                    startIdx = i + 1;
                }


                if(ch == '(')
                {
                    element.values = fileText.Substring(startIdx, i - startIdx);
                    valuesParsed = true;

                    PcbFileElement newElement = ParseElement(i + 1);
                    //DecodeElement(newElement);
                    element.children.Add(newElement);
                    i = newElement.stopIdx;
                }

                if (ch == ')')
                {
                    if(valuesParsed == false)
                    {
                        element.values = fileText.Substring(startIdx, i - startIdx);
                    }

                    element.stopIdx = i;
                    return element;
                }


            }
            return element;
        }

        public bool Parse(string filename)
        {
            fileText = File.ReadAllText(filename, Encoding.UTF8);

            fileText = fileText.Replace("\r", "");
            fileText = fileText.Replace("\n", "");

            mainElement = null;

            mainElement = ParseElement(0);
            if(mainElement != null)
            {
                Decode(mainElement);
                return true;
            }
            else
            {
                return false;
            }
            


        }
    }
}
