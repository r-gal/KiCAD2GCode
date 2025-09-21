using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static KiCad2Gcode.CrossUnit;
using static KiCad2Gcode.Polygon;

namespace KiCad2Gcode
{


    internal class Merger
    {
        MainUnit mainUnit;

        List<Figure> listA = null;
        List<Figure> listB = null;

        int idxA;
        int idxB;

        public Merger(MainUnit mainUnit)
        {
            this.mainUnit = mainUnit;
        }

        public bool Init(List<Figure> listA, List<Figure> listB)
        {
            this.listA = listA;
            this.listB = listB;

            idxA = 0;

            if (listA == listB)
            {
                if(listA.Count < 2)
                {
                    return false;
                }
                idxB = 1;
            }
            else
            {
                if (listA.Count < 1 || listB.Count < 1)
                {
                    return false;
                }
                idxB = 0;
            }
            return true;
        }

        public bool Step(int phase)
        {
            bool result;
            bool merged = false;


            foreach (Figure f in listA)
            {
                f.shape.selected = 0;
            }

            if(listA != listB)
            {
                foreach (Figure f in listB)
                {
                    f.shape.selected = 0;
                }
            }

            if ((listA == listB) && (idxB == idxA)) { idxB++; }

            if (listA != null && idxA < listA.Count && idxB < listB.Count && ((listA != listB) || (idxA != idxB)))
            {

                merged = false;

                listA[idxA].shape.selected = 1;
                listB[idxB].shape.selected = 2;

                Figure mergedFigure = null;

                if (phase != 2 || listA[idxA].touched == true || listB[idxB].touched == true)
                {
                    mergedFigure = Merge(listA[idxA], listB[idxB]);
                }


                if (mergedFigure != null)
                {

                    //PrintText("Found " + idxA.ToString() + " vs " + idxB.ToString() + "size = " + netList[idxNet].figures.Count.ToString() + "\n");

                    listA[idxA] = mergedFigure;
                    listB.RemoveAt(idxB);


                    merged = true;

                    if (phase >= 1)
                    {
                        mergedFigure.touched = true;
                    }
                }
                else
                {

                    //PrintText("skip\n");
                    idxB++;
                    if ((listA == listB) && (idxB == idxA)) { idxB++; }
                }

                if (merged == true)
                {
                    if (listA == listB)
                    {
                        idxB = idxA + 1;
                    }
                    else
                    {
                        idxB = 0;
                    }

                }

                if (idxB >= listB.Count)
                {
                    idxA++;
                    if (listA == listB)
                    {
                        idxB = idxA + 1;
                    }
                    else
                    {
                        idxB = 0;
                    }
                }


                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }


        

        private int SelectCrossingPoints(Polygon pol1, Polygon pol2)
        {
            int pointCnt = 0;



            int n1Idx = 0;
            int n2Idx = 0;


            Point2D lastFoundPoint = null;
            Point2D lastFoundPoint2 = null;
            int lastIdx1 = 0;
            int lastIdx2 = 0;

            LinkedListNode<Node> n1 = pol1.points.First;

            while (n1 != null)
            {
                LinkedListNode<Node> n2 = pol2.points.First;
                n2Idx = 0;

                while (n2 != null)
                {
                    CrossUnit crossUnit = new CrossUnit();

                    if(n1Idx == 0 && n2Idx == 3)
                    {
                        int trap = 0; 
                    }
                    if (n1Idx == 1 && n2Idx == 0)
                    {
                        int trap = 0;
                    }

                    List<Point2D> points = crossUnit.GetCrosssingPoints(n1, n2);

                    
                    if (points != null)
                    {
                        //mainUnit.PrintText("Find Point 1 at " + points[0].x.ToString() + "," + points[0].y.ToString() + " type is " + points[0].type.ToString() + "\n");
                        pointCnt++;
                        lastFoundPoint = points[0];
                        lastIdx1 = n1Idx;
                        lastIdx2 = n2Idx;
                        if (points.Count == 2)
                        {
                            //mainUnit.PrintText("Find Point 2 at " + points[1].x.ToString() + "," + points[1].y.ToString() + " type is " + points[1].type.ToString() + "\n");
                            pointCnt++;
                            lastFoundPoint2 = points[1];
                        }
                        else
                        {
                            lastFoundPoint2 = null;
                        }

                        /* cut f1 */

                        n1 = Figure.SplitChunk(n1, points);

                        /* cut f2 */

                        n2 = Figure.SplitChunk(n2, points);


                        if (points.Count == 1)
                        {
                            /* easy case */
                            n1.Value.oppNode = n2;
                            n2.Value.oppNode = n1;
/*
                            mainUnit.PrintText("Set oppNode for f1: " + n1.Value.oppNode.Value.pt.x.ToString() + "," + n1.Value.oppNode.Value.pt.y.ToString() + "\n");
                            mainUnit.PrintText("Set oppNode for f2: " + n2.Value.oppNode.Value.pt.x.ToString() + "," + n2.Value.oppNode.Value.pt.y.ToString() + "\n");
*/
                        }
                        else
                        {
                            if (n1.Value.pt == n2.Value.pt)
                            {
                                n1.Value.oppNode = n2;
                                n2.Value.oppNode = n1;

                                n1.Next.Value.oppNode = n2.Next ?? n2.List.First;
                                n2.Next.Value.oppNode = n1.Next ?? n1.List.First;
                            }
                            else
                            {
                                n1.Value.oppNode = n2.Next ?? n2.List.First;
                                n2.Value.oppNode = n1.Next ?? n1.List.First;

                                n1.Next.Value.oppNode = n2;
                                n2.Next.Value.oppNode = n1;

                            }
/*
                            mainUnit.PrintText("Set oppNode for f1: " + n1.Value.oppNode.Value.pt.x.ToString() + "," + n1.Value.oppNode.Value.pt.y.ToString() + "\n");
                            mainUnit.PrintText("Set oppNode for f2: " + n2.Value.oppNode.Value.pt.x.ToString() + "," + n2.Value.oppNode.Value.pt.y.ToString() + "\n");

                            mainUnit.PrintText("Set oppNode 2 for f1: " + n1.Next.Value.oppNode.Value.pt.x.ToString() + "," + n1.Next.Value.oppNode.Value.pt.y.ToString() + "\n");
                            mainUnit.PrintText("Set oppNode 2 for f2: " + n2.Next.Value.oppNode.Value.pt.x.ToString() + "," + n2.Next.Value.oppNode.Value.pt.y.ToString() + "\n");
*/

                        }


                    }
                    else
                    {
                        int x = 0;/*skip */
                    }
         

                    n2 = n2.Next;
                    n2Idx++;
                }
                n1 = n1.Next;
                n1Idx++;
            }

            return pointCnt;
        }

        private LinkedListNode<Node> GetNodeForUnion(Polygon pol1, Polygon pol2)
        {
            if (pol1.extNode[0].pt.x < pol2.extPoint[0].x) { return pol1.points.Find(pol1.extNode[0]); }
            if (pol2.extNode[0].pt.x < pol1.extPoint[0].x) { return pol2.points.Find(pol2.extNode[0]); }
            if (pol1.extNode[1].pt.y > pol2.extPoint[1].y) { return pol1.points.Find(pol1.extNode[1]); }
            if (pol2.extNode[1].pt.y > pol1.extPoint[1].y) { return pol2.points.Find(pol2.extNode[1]); }
            if (pol1.extNode[2].pt.x > pol2.extPoint[2].x) { return pol1.points.Find(pol1.extNode[2]); }
            if (pol2.extNode[2].pt.x > pol1.extPoint[2].x) { return pol2.points.Find(pol2.extNode[2]); }
            if (pol1.extNode[3].pt.y < pol2.extPoint[3].y) { return pol1.points.Find(pol1.extNode[3]); }
            if (pol2.extNode[3].pt.y < pol1.extPoint[3].y) { return pol2.points.Find(pol2.extNode[3]); }

            if (pol1.extNode[0].pt.x == pol2.extPoint[0].x) { return pol1.points.Find(pol1.extNode[0]); }
            if (pol2.extNode[0].pt.x == pol1.extPoint[0].x) { return pol2.points.Find(pol2.extNode[0]); }
            if (pol1.extNode[1].pt.y == pol2.extPoint[1].y) { return pol1.points.Find(pol1.extNode[1]); }
            if (pol2.extNode[1].pt.y == pol1.extPoint[1].y) { return pol2.points.Find(pol2.extNode[1]); }
            if (pol1.extNode[2].pt.x == pol2.extPoint[2].x) { return pol1.points.Find(pol1.extNode[2]); }
            if (pol2.extNode[2].pt.x == pol1.extPoint[2].x) { return pol2.points.Find(pol2.extNode[2]); }
            if (pol1.extNode[3].pt.y == pol2.extPoint[3].y) { return pol1.points.Find(pol1.extNode[3]); }
            if (pol2.extNode[3].pt.y == pol1.extPoint[3].y) { return pol2.points.Find(pol2.extNode[3]); }

            LinkedListNode<Node> n = pol1.points.First;
            while(n != null)
            {
                POINT_LOC_et result = CheckPointInPolygon(n.Value.pt, pol2);
                if(result == POINT_LOC_et.OUT) { return n; }
                n = n.Next;
            }

            n = pol2.points.First;
            while (n != null)
            {
                POINT_LOC_et result = CheckPointInPolygon(n.Value.pt, pol1);
                if (result == POINT_LOC_et.OUT) { return n; }
                n = n.Next;
            }

            n = pol1.points.First;
            while (n != null)
            {
                POINT_LOC_et result = CheckPointInPolygon(n.Value.pt, pol2);
                if (result != POINT_LOC_et.IN) { return n; }
                n = n.Next;
            }

            n = pol2.points.First;
            while (n != null)
            {
                POINT_LOC_et result = CheckPointInPolygon(n.Value.pt, pol1);
                if (result != POINT_LOC_et.IN) { return n; }
                n = n.Next;
            }

            return null;
        }

        private LinkedListNode<Node> GetNodeForHole(Polygon pol1)
        {
            LinkedListNode<Node> n1 = pol1.points.First;

            while (n1 != null)
            {
                if(n1.Value.pt.type != Point2D.PointType_et.NORMAL)
                {
                    return n1;
                }
                n1 = n1.Next;
            }
            return null;
        }

        private LinkedListNode<Node> GetNodeForHoleCutting(Polygon hole, Polygon pol)
        {
            LinkedListNode<Node> n1 = hole.points.First;

            while (n1 != null)
            {
                if (n1.Value.pt.type != Point2D.PointType_et.NORMAL)
                {
                    return n1;
                }
                n1 = n1.Next;
            }
            return null;

        }


        private LinkedListNode<Node> GetNode(Polygon selectPol, Polygon areaPol, bool inside)
        {
            LinkedListNode<Node> node = null;

            LinkedListNode<Node> actNode = selectPol.points.First;
            while (actNode != null)
            {
                if (actNode.Value.pt.type == Point2D.PointType_et.NORMAL)
                {
                    Polygon.POINT_LOC_et pointIsInFigure = Polygon.CheckPointInPolygon(actNode.Value.pt, areaPol);

                    if (pointIsInFigure == Polygon.POINT_LOC_et.IN && inside == true)
                    {
                        return actNode;
                    }
                    else if (pointIsInFigure == Polygon.POINT_LOC_et.OUT && inside == false)
                    {
                        return actNode;
                    }
                }
                    
                actNode = actNode.Next;
            }
            /* cannot find any solid point, try to find new point on segment splits */

            actNode = selectPol.points.First;
            LinkedListNode<Node> prevNode = selectPol.points.Last;

            Point2D prevPt = prevNode.Value.pt;
            Point2D actPt = actNode.Value.pt;
            while (actNode != null)
            {
                Point2D testPoint = null;
                double divAngle = 0;
                if (actNode.Value.arc == null)
                {
                    testPoint = new Point2D(prevPt.x + (actPt.x - prevPt.x) / 2, prevPt.y + (actPt.y - prevPt.y) / 2);
                }
                else
                {

                    if( actNode.Value.arc.endAngle > actNode.Value.arc.startAngle)
                    {
                        divAngle = (actNode.Value.arc.endAngle + actNode.Value.arc.startAngle) / 2 - Math.PI;
                    }
                    else
                    {
                        divAngle = (actNode.Value.arc.endAngle + actNode.Value.arc.startAngle) / 2;
                    }
                    testPoint = new Point2D(actNode.Value.arc.radius, 0);
                    testPoint.Rotate(divAngle);
                    testPoint.x += actNode.Value.arc.centre.x;
                    testPoint.y += actNode.Value.arc.centre.y;
                }

                bool pointOk = false;
                Polygon.POINT_LOC_et pointIsInFigure = Polygon.CheckPointInPolygon(actNode.Value.pt, areaPol);
                if (pointIsInFigure == Polygon.POINT_LOC_et.IN && inside == true)
                {
                    pointOk = true;
                }
                else if (pointIsInFigure == Polygon.POINT_LOC_et.OUT && inside == false)
                {
                    pointOk = true;
                }

                if(pointOk)
                {
                    Node newNode = new Node();
                    newNode.pt = actPt;
                    actNode.Value.pt = testPoint;

                    newNode.oppNode = actNode.Value.oppNode;
                    actNode.Value.oppNode = null;
                    
                    if (actNode.Value.arc != null)
                    {
                        newNode.arc = new Arc();
                        newNode.arc.radius = actNode.Value.arc.radius;
                        newNode.arc.endAngle = actNode.Value.arc.endAngle;
                        actNode.Value.arc.endAngle = divAngle;
                        newNode.arc.startAngle = divAngle;
                    }
                    LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                    actNode.List.AddAfter(actNode, newElement);
                    return actNode;
                }
                actNode = actNode.Next;
            }

            return node;
        }


        private LinkedListNode<Node> GetNodeOutside(Polygon selectPol, Polygon areaPol)
        {
            return GetNode(selectPol, areaPol, false);
        }

        private LinkedListNode<Node> GetNodeInside(Polygon selectPol, Polygon areaPol)
        {
            return GetNode(selectPol, areaPol, true);            
        }

        private Polygon CreatePolygon(Polygon pol1, Polygon pol2, LinkedListNode<Node> startNode, bool goLeft, bool redraw)
        {
            LinkedListNode<Node> actNode = startNode;
            LinkedListNode<Node> firstNode = startNode;
            LinkedListNode<Node> prevNode = startNode.Previous ?? startNode.List.Last;
            Point2D prevPoint = prevNode.Value.pt;
            /*
            if(actNode.Value.pt.type != Point2D.PointType_et.NORMAL)
            {
                LinkedListNode<Node> oppNode = actNode.Value.oppNode.Previous ?? actNode.Value.oppNode.List.Last;
                LinkedListNode<Node> prevNode2 = oppNode.Previous ?? oppNode.List.Last;
                Point2D prevPoint2 = prevNode2.Value.pt;
                
            }*/
            bool initial = true;


            Polygon newPolygon = new Polygon();
            if (redraw)
            {
                mainUnit.drawer.DrawDot(startNode.Value.pt, 2, Color.LightGreen);
                mainUnit.drawer.SetCentre(startNode.Value.pt);

                foreach (Node nt in pol1.points)
                {
                    if(nt.pt.type == Point2D.PointType_et.CROSS_X)
                    {
                        mainUnit.drawer.DrawDot(nt.pt, 2, Color.Pink);
                    }
                    else if (nt.pt.type == Point2D.PointType_et.CROSS_T)
                    {
                        mainUnit.drawer.DrawDot(nt.pt, 2, Color.Violet);
                    }
                    else
                    {
                        mainUnit.drawer.DrawDot(nt.pt, 2, Color.Black);
                    }
                }
            }
                

            do
            {
                Node n = actNode.Value;
                Node newNode = new Node();
                newNode.pt = n.pt;
                newNode.arc = n.arc;
                newNode.oppNode = null;
                LinkedListNode<Node> copiedNode = new LinkedListNode<Node>(newNode);

                newPolygon.points.AddLast(copiedNode);

                if (newPolygon.points.Count > 200000)
                {
                    /*infinite loop*/
                    //newPolygon.GetExtPoints();
                    //return newPolygon;
                    return null;
                }
                if(redraw)
                {
                    mainUnit.drawer.SetCentre(n.pt);
                    mainUnit.drawer.DrawElement(prevPoint, n.pt, n.arc);
                }
                

                if (actNode.Value.pt.type == Point2D.PointType_et.NORMAL)
                {
                    prevPoint = actNode.Value.pt;
                    actNode = actNode.Next ?? actNode.List.First;
                }

                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_T || initial)
                {
                    actNode.Value.pt.type = Point2D.PointType_et.NORMAL; /* reset type */
                    LinkedListNode<Node> nodeA = actNode.Next ?? actNode.List.First;
                    LinkedListNode<Node> nodeB = actNode.Value.oppNode.Next ?? actNode.Value.oppNode.List.First;

                    /* choose correct node */

                    /* angle is for compare only so alfa function is enough */

                    Point2D actPoint = actNode.Value.pt;

                    Point2D nextPoint1 = nodeA.Value.pt;
                    Point2D nextPoint2 = nodeB.Value.pt;

                    double inputAngle = Vector.GetAlpha(actPoint, prevPoint);

                    double out1Angle = 0;
                    double out2Angle = 0;

                    double wgt1 = 0;
                    double wgt2 = 0;


                    if (nodeA.Value.arc == null)
                    {
                        out1Angle = Vector.GetAlpha(actPoint, nextPoint1);
                        wgt1 = 0;// Double.MaxValue;
                    }
                    else
                    {
                        Vector v = nodeA.Value.arc.centre - actPoint;
                        Vector vt;
                        if(nodeA.Value.arc.ccw == false)
                        {
                            vt = new Vector(-v.y, v.x);
                            wgt1 = 1/nodeA.Value.arc.radius;
                        }
                        else
                        {
                            vt = new Vector(v.y, -v.x);
                            wgt1 = -1/nodeA.Value.arc.radius;
                        }
                        out1Angle = Vector.GetAlpha(vt);
                        
                    }

                    if (nodeB.Value.arc == null)
                    {
                        out2Angle = Vector.GetAlpha(actPoint, nextPoint2);

                        wgt2 = 0;// Double.MaxValue;
                    }
                    else
                    {
                        Vector v = nodeB.Value.arc.centre - actPoint;
                        Vector vt;
                        if (nodeB.Value.arc.ccw == false)
                        {
                            vt = new Vector(-v.y, v.x);
                            wgt2 = 1/nodeB.Value.arc.radius;
                        }
                        else
                        {
                            vt = new Vector(v.y, -v.x);
                            wgt2 = -1/nodeB.Value.arc.radius;
                        }
                        out2Angle = Vector.GetAlpha(vt);
                        
                    }


                    /*
                    mainUnit.PrintText("Test in " + actPoint.x.ToString() + "," + actPoint.y.ToString() + "\n");
                    mainUnit.PrintText("NODE1 " + nextPoint1.x.ToString() + "," + nextPoint1.y.ToString() + "\n");
                    mainUnit.PrintText("NODE2 " + nextPoint2.x.ToString() + "," + nextPoint2.y.ToString() + "\n");

                    mainUnit.PrintText("Alpha : IN=" + inputAngle.ToString() + " OUT1=" + out1Angle.ToString() + " OUT2=" + out2Angle.ToString() + "\n");
                    */
                    prevPoint = actNode.Value.pt;

                    if (Math.Abs(out1Angle - out2Angle) < 0.00001)
                    {

                        if (wgt1 < wgt2)
                        {
                            /*out1 is 2 */
                            actNode = nodeA;
                        }
                        else
                        {
                            /*out2 is 2 */
                            actNode = nodeB;
                        }
                    }
                    else if(inputAngle == out1Angle)
                    {
                        actNode = nodeB;
                    }
                    else if (inputAngle == out2Angle)
                    {
                        actNode = nodeA;
                    }
                    else if (inputAngle > out1Angle)
                    {
                        if (inputAngle > out2Angle)
                        {
                            /* prev is 3 */
                            if (out1Angle > out2Angle)
                            {
                                /*out1 is 2 */
                                actNode = nodeA;
                            }
                            else
                            {
                                /*out2 is 2 */
                                actNode = nodeB;
                            }
                        }
                        else
                        {
                            /* prev is 2 , out1 is 1*/
                            actNode = nodeA;
                        }
                    }
                    else if (inputAngle > out2Angle)
                    {
                        /* prev is 2 , out2 is 1*/
                        actNode = nodeB;
                    }
                    else
                    {
                        /* prev is 1 */
                        if (out1Angle > out2Angle)
                        {
                            /*out1 is 3 */
                            actNode = nodeA;
                        }
                        else
                        {
                            /*out2 is 3 */
                            actNode = nodeB;
                        }
                    }

                    //mainUnit.PrintText("Go to  " + actNode.Value.pt.x.ToString() + "," + actNode.Value.pt.y.ToString() + "\n");

                }
                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_X)
                {
                    actNode.Value.pt.type = Point2D.PointType_et.NORMAL; /* reset type */
                    prevPoint = actNode.Value.pt;
                    actNode = actNode.Value.oppNode.Next ?? actNode.Value.oppNode.List.First;



                }
                initial = false;

                
            }
            while (actNode.Value.pt != firstNode.Value.pt);

            if(actNode != firstNode)
            {
                /* point is this same but different figure, check arcs */
                newPolygon.points.First.Value = actNode.Value;
            }

            newPolygon.GetExtPoints();

            return newPolygon;



        }


        private void PrintPolygonData(Polygon p)
        {
            MainUnit.PrintText(" Polygon data: \n ");
            foreach (Node n in p.points)
            {
                MainUnit.PrintText("Node " + n.idx.ToString() + "State " + n.pt.state.ToString() + " Type " + n.pt.type.ToString() + " (" + n.pt.x.ToString() + " " + n.pt.y.ToString() + ")");
                if (n.oppNode != null)
                {
                    MainUnit.PrintText(" OppIdx = " + n.oppNode.Value.idx.ToString() + " " + n.oppNode.Value.pt.type.ToString());
                }
                if (n.arc != null)
                {
                    MainUnit.PrintText(" Arc " + (n.arc.ccw ? "CCW" : "CW"));
                }
                MainUnit.PrintText("\n ");
            }


        }
       

        public Figure Merge(Figure f1, Figure f2)
        {
            bool cont = false;

            bool merged = false;

            /*early check */

            if (f1.shape.extPoint[0].x > f2.shape.extPoint[2].x) { return null; }
            if (f2.shape.extPoint[0].x > f1.shape.extPoint[2].x) { return null; }

            if (f1.shape.extPoint[3].y > f2.shape.extPoint[1].y) { return null; }
            if (f2.shape.extPoint[3].y > f1.shape.extPoint[1].y) { return null; }

            /*
            f1.shape.Renumerate();
            f2.shape.Renumerate();

            MainUnit.PrintText(f1.name + "\n");
            PrintPolygonData(f1.shape);
            MainUnit.PrintText(f2.name + "\n");
            PrintPolygonData(f2.shape);
            */
            

            /* find crossing points */

            int crossingPoints = SelectCrossingPoints(f1.shape, f2.shape);
            //return null;

            Figure newFigure = null;

            if (crossingPoints > 0)
            {
               /*
                f1.shape.Renumerate();
                f2.shape.Renumerate();

                MainUnit.PrintText(f1.name + "\n");
                PrintPolygonData(f1.shape);
                MainUnit.PrintText(f2.name + "\n");
                PrintPolygonData(f2.shape);
                */

                /*choose start point */
                f1.shape.GetExtPoints();
                f2.shape.GetExtPoints();

                //LinkedListNode<Node> actNode = GetNodeOutside(f1.shape, f2.shape);
                LinkedListNode<Node> actNode = GetNodeForUnion(f1.shape, f2.shape);

                if (actNode == null)
                {
                    /*something weird */
                    MainUnit.PrintText("Start point not found\n");
                    return null;
                }
                //MainUnit.PrintText("Start point at idx " + actNode.Value.idx.ToString() + " pt = " + actNode.Value.pt.x.ToString() + " " + actNode.Value.pt.y.ToString());



                /* sanity check */

                foreach (Node n in f1.shape.points)
                {
                    if(n.pt.type != Point2D.PointType_et.NORMAL && n.oppNode == null)
                    {
                        MainUnit.PrintText("Error\n");
                    }
                }

                foreach (Node n in f2.shape.points)
                {
                    if (n.pt.type != Point2D.PointType_et.NORMAL && n.oppNode == null)
                    {
                        MainUnit.PrintText("Error\n");
                    }
                }

                Polygon newPol = CreatePolygon(f1.shape, f2.shape, actNode, true,false);
                if(newPol == null) { return null; }

                newFigure = new Figure();
                newFigure.shape = newPol;

                /* search holes in merged polygon*/

                do
                {
                    actNode = GetNodeForHole(f1.shape);

                    if (actNode != null)
                    {
                        //MainUnit.PrintText("Hole start at  " + actNode.Value.pt.x.ToString() + "," + actNode.Value.pt.y.ToString() + "\n");
                        newPol = CreatePolygon(f1.shape, f2.shape, actNode, true,false);

                        newFigure.holes.Add(newPol);
                    }
                } while (actNode != null);

                merged = true;
                cont = true;
            }
            else
            {
                /* shape 1 and shape 2 have not any crossing points. It is necessary to check if one shape not includes another one.
                 * In this case outer polygon mut be returnd as result and origin holes must be proceeded. */

                Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(f1.shape, f2.shape);

                if(pos == Polygon.POLYGONS_POS_et.P1_IN_P2)
                {
                    newFigure = new Figure();
                    newFigure.shape = f2.shape;
                    cont = true;
                }
                else if(pos == Polygon.POLYGONS_POS_et.P2_IN_P1)
                {
                    newFigure = new Figure();
                    newFigure.shape = f1.shape;
                    cont = true;
                }
                else
                {
                    return null;
                }

            }

            if(cont)
            {
                /* cut holes in origin polygons */
                LinkedListNode<Node> actNode;
                Polygon newPol;


                foreach (Polygon hole in f1.holes)
                {

                    //hole.selected = 1;
                    //f2.shape.selected = 2;
                    //mainUnit.RedrawAll();
                    /*
                    hole.Renumerate();
                    f2.shape.Renumerate();

                    
                    PrintPolygonData(hole);
                    MainUnit.PrintText(f2.name + "\n");
                    PrintPolygonData(f2.shape);*/

                    crossingPoints = SelectCrossingPoints(hole, f2.shape);
                    /*
                    hole.Renumerate();
                    f2.shape.Renumerate();

                    
                    PrintPolygonData(hole);
                    MainUnit.PrintText(f2.name + "\n");
                    PrintPolygonData(f2.shape);

                    mainUnit.RedrawAll();*/

                    if (crossingPoints > 1)
                    {
                        do
                        {
                            actNode = GetNodeForHoleCutting(hole, f2.shape);
                            if (actNode != null)
                            {
                                newPol = CreatePolygon(hole, f2.shape, actNode, true,false);

                                newFigure.holes.Add(newPol);
                            }
                        } while (actNode != null);
                        merged = true;
                    }
                    else
                    {
                        /* this mean that hole is fully covered or fully uncovered or polygon is fully within hole, additional check is necessary */
                        Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(hole, f2.shape);

                        if(pos == Polygon.POLYGONS_POS_et.NONE)
                        {
                            /* hole is fully uncovered */
                            newFigure.holes.Add(hole);
                        }
                        else if(pos == Polygon.POLYGONS_POS_et.P2_IN_P1)
                        {
                            /*polygon is fully within hole */

                            hole.Renumerate();
                            f2.shape.Renumerate();

                            //mainUnit.PrintText(hole.name + "\n");
                            //PrintPolygonData(hole);
                            //mainUnit.PrintText(f2.name + "\n");
                            //PrintPolygonData(f2.shape);

                            return null;
                        }
                    }

                    //hole.selected = 0;
                    //f2.shape.selected = 0;
                    //mainUnit.RedrawAll();
                }
                
                foreach (Polygon hole in f2.holes)
                {
                    crossingPoints = SelectCrossingPoints(hole, f1.shape);
                    if (crossingPoints > 1)
                    {
                        do
                        {
                            actNode = GetNodeForHoleCutting(hole, f1.shape);

                            if (actNode != null)
                            { 
                                newPol = CreatePolygon(hole, f1.shape, actNode, true, false);

                                newFigure.holes.Add(newPol);
                            }
                        } while (actNode != null);
                        merged = true;
                    }
                    else
                    {
                        /* this mean that hole is fully covered or fully uncovered or polygon is fully within hole, additional check is necessary */
                        Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(hole, f1.shape);


                        if (pos == Polygon.POLYGONS_POS_et.NONE)
                        {
                            /* hole is fully uncovered */
                            newFigure.holes.Add(hole);
                        }
                        else if (pos == Polygon.POLYGONS_POS_et.P2_IN_P1)
                        {
                            /*polygon is fully within hole */
                            return null;
                        }
                    }


                }

                /* search for common parts of holes */


                /* merge holes */
                foreach (Polygon hole1 in f1.holes)
                {
                    foreach (Polygon hole2 in f2.holes)
                    {
                        crossingPoints = SelectCrossingPoints(hole1, hole2);
                        if (crossingPoints > 1)
                        {
                            do
                            {
                                actNode = GetNodeForHoleCutting(hole2, hole2);
                                if (actNode != null)
                                {         
                                    newPol = CreatePolygon(hole2, hole2, actNode, true, false);

                                    newFigure.holes.Add(newPol);
                                }
                            } while (actNode != null);
                        }
                        else
                        {
                            Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(hole1, hole2);

                            if(pos == Polygon.POLYGONS_POS_et.P1_IN_P2)
                            {
                                newFigure.holes.Add(hole2);
                            }
                            else if (pos == Polygon.POLYGONS_POS_et.P2_IN_P1)
                            {
                                newFigure.holes.Add(hole1);
                            }
                            /* check if holes are fully overlaped is necessary */
                        }
                    }
                }
            }
            if(newFigure != null)
            {
                newFigure.name = f1.name + " || " + f2.name;
                newFigure.net = f1.net;
            }


            return newFigure;
        }
    }
}
