using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
                    Figure figA = listA[idxA];
                    Figure figB = listB[idxB];

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

            foreach (Node n in pol1.points)
            {
                if (n.pt.type != Point2D.PointType_et.NORMAL && n.oppNode == null)
                {
                    MainUnit.PrintText("Error\n");
                }
                n.pt.storedType = n.pt.type;
            }

            foreach (Node n in pol2.points)
            {
                if (n.pt.type != Point2D.PointType_et.NORMAL && n.oppNode == null)
                {
                    MainUnit.PrintText("Error\n");
                }
                n.pt.storedType = n.pt.type;
            }

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

                    List<Point2D> points = crossUnit.GetCrosssingPoints(n1, n2,false);

                    
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
                if((n1.Value.pt.type != Point2D.PointType_et.NORMAL) && (n1.Value.pt.state == Point2D.STATE_et.FREE ))
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
                if ((n1.Value.pt.type != Point2D.PointType_et.NORMAL) && (n1.Value.pt.state == Point2D.STATE_et.FREE))
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

        

        private List<AngleData> SortAngles(LinkedListNode<Node> actNode, LinkedListNode<Node> prevNode)
        {

            List<AngleData> list = new List<AngleData>();
            if (actNode.Value.oppNode != null)
            {
                LinkedListNode<Node> oppNode = actNode.Value.oppNode;

                LinkedListNode<Node> nodeA = actNode.Next ?? actNode.List.First;
                LinkedListNode<Node> nodeB = oppNode.Next ?? oppNode.List.First;

                list.Add(new AngleData(actNode.Value, nodeA,false));
                list.Add(new AngleData(oppNode.Value, nodeB, false));
                
                if (prevNode == null)
                {
                    LinkedListNode<Node> oppPrev = oppNode.Previous ?? oppNode.List.First;
                    list.Add(new AngleData(oppNode.Value, oppPrev, true));

                    prevNode = actNode.Previous ?? actNode.List.First;
                }
                list.Add(new AngleData(actNode.Value, prevNode, true));

            }

            bool ready;

            do
            {
                ready = true;
                for(int i = 0; i< list.Count-1;i++)
                {
                    bool swap = false;

                    if (Math.Abs(list[i].angle - list[i + 1].angle) < 0.00001)
                    {
                        if (list[i].wgt > list[i + 1].wgt)
                        {
                            swap = true;
                        }
                    }
                    else if (list[i].angle < list[i+1].angle)
                    {
                        swap = true;
                    }


                    if(swap)
                    {
                        ready = false;
                        AngleData tmp = list[i];
                        list[i] = list[i + 1];
                        list[i + 1] = tmp;
                    }

                }
            } while(ready == false);

            while (list[0].isInput == false)
            {
                AngleData tmp = list[0];
                list.RemoveAt(0);
                list.Add(tmp);
            }
            

            return list;
        }

        private Polygon CreatePolygon(Polygon pol1, Polygon pol2, LinkedListNode<Node> startNode, bool goLeft, bool redraw)
        {
            LinkedListNode<Node> actNode = startNode;
            LinkedListNode<Node> firstNode = startNode;
            LinkedListNode<Node> prevNode = null;
            //Point2D prevPoint = prevNode.Value.pt;
            /*
            if(actNode.Value.pt.type != Point2D.PointType_et.NORMAL)
            {
                LinkedListNode<Node> oppNode = actNode.Value.oppNode.Previous ?? actNode.Value.oppNode.List.Last;
                LinkedListNode<Node> prevNode2 = oppNode.Previous ?? oppNode.List.Last;
                Point2D prevPoint2 = prevNode2.Value.pt;
                
            }*/
            bool initial = true;


            Polygon newPolygon = new Polygon();
            /*
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
                */

            bool tried = false;
            do
            {
                Node n = actNode.Value;


                if (n.pt.state != Point2D.STATE_et.FREE && n.pt != firstNode.Value.pt)
                {
                    /* check if another way from start point is possible, just clear new polygon and start loop one more time. First way is signed 
                     * as already used so only next way can be selected. If another way is also signed as already used then null will be returned*/
                    bool nextTry = false;
                    
                    if(firstNode.Value.pt.type == Point2D.PointType_et.CROSS_T && tried == false)
                    {
                        LinkedListNode<Node> oppNode = firstNode.Value.oppNode;

                        LinkedListNode<Node> nodeA = firstNode.Next ?? actNode.List.First;
                        LinkedListNode<Node> nodeB = oppNode.Next ?? oppNode.List.First;

                        if (nodeA.Value.pt.state == Point2D.STATE_et.FREE) 
                        {
                            /* way is possible, try it */
                            nextTry = true;
                            newPolygon.points.Clear();
                            actNode = firstNode;
                            prevNode = actNode.Previous ?? actNode.List.Last;
                            n = actNode.Value;

                        }
                        else if( nodeB.Value.pt.state == Point2D.STATE_et.FREE)
                        {
                            /* way is possible, try it */
                            nextTry = true;
                            newPolygon.points.Clear();
                            actNode = oppNode;
                            prevNode = oppNode.Previous ?? oppNode.List.Last;
                            n = actNode.Value;
                        }
                        tried = true;

                    }


                    if (nextTry == false)
                    {

                        MainUnit.PrintText("Error, point reused\n");

                        MainUnit.PrintText("Start point at idx " + startNode.Value.idx.ToString() + " pt = " + startNode.Value.pt.x.ToString() + " " + startNode.Value.pt.y.ToString());

                        MainUnit.PrintText(" pol1: \n");
                        PrintPolygonData(pol1);
                        MainUnit.PrintText(" pol2: \n");
                        PrintPolygonData(pol2);

                        MainUnit.PrintText(" newpol: \n");
                        PrintPolygonData(newPolygon);

                        mainUnit.ClearNetList();

                        pol1.GetExtPoints();
                        Figure ft1 = new Figure();
                        ft1.shape = pol1;
                        ft1.net = 0;
                        newPolygon.selected = 1;
                        mainUnit.AddFigure(ft1);

                        pol2.GetExtPoints();
                        Figure ft2 = new Figure();
                        ft2.shape = pol2;
                        ft2.net = 0;
                        newPolygon.selected = 2;
                        mainUnit.AddFigure(ft2);

                        newPolygon.GetExtPoints();
                        Figure ft3 = new Figure();
                        ft3.shape = newPolygon;
                        ft3.net = 0;
                        newPolygon.selected = 3;
                        mainUnit.AddFigure(ft3);

                        mainUnit.RedrawAll();



                        return null;
                    }
                }

                Node newNode = new Node();
                /*
                newNode.pt = new Point2D(n.pt);
                newNode.pt.type = Point2D.PointType_et.NORMAL;
                newNode.arc = n.arc == null ? null : new Arc(n.arc);*/
                newNode.pt = n.pt;
                newNode.arc = n.arc;
                newNode.oppNode = null;
                LinkedListNode<Node> copiedNode = new LinkedListNode<Node>(newNode);

                newPolygon.points.AddLast(copiedNode);

                n.pt.state = Point2D.STATE_et.ALREADY_USED;

                

                if (newPolygon.points.Count > 200000)
                {
                    /*infinite loop*/
                    //newPolygon.GetExtPoints();
                    //return newPolygon;ok,

                    MainUnit.PrintText("Start point at idx " + startNode.Value.idx.ToString() + " pt = " + startNode.Value.pt.x.ToString() + " " + startNode.Value.pt.y.ToString());

                    MainUnit.PrintText(" pol1: \n");
                    PrintPolygonData(pol1);
                    MainUnit.PrintText(" pol2: \n");
                    PrintPolygonData(pol2);

                    return null;
                }
                /*
                if(redraw)
                {
                    mainUnit.drawer.SetCentre(n.pt);
                    mainUnit.drawer.DrawElement(prevPoint, n.pt, n.arc);
                }
                */

                if (actNode.Value.pt.type == Point2D.PointType_et.NORMAL)
                {
                    //prevPoint = actNode.Value.pt;
                    
                    prevNode = actNode;
                    actNode = actNode.Next ?? actNode.List.First;
                }
                else if (initial)
                {
                    


                    List<AngleData> list = SortAngles(actNode, prevNode);

                    //prevPoint = actNode.Value.pt;

                    if (list.Count == 4)
                    {
                        if (list[1].isInput == false && list[2].isInput == false)
                        {
                            actNode = list[1].node;
                        }
                        else if (list[2].isInput == false && list[3].isInput == false)
                        {
                            actNode = list[2].node;
                        }
                        else
                        {
                            /* not explicit choose, try first */
                            if (list[1].node.Value.pt.state == Point2D.STATE_et.FREE)
                            {
                                actNode = list[1].node;
                            }
                            else if (list[3].node.Value.pt.state == Point2D.STATE_et.FREE)
                            {
                                actNode = list[3].node;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }


                    prevNode = actNode.Previous ?? actNode.List.Last;

                    /* sort input and output angles */


                }
                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_T)
                {
                   

                    List<AngleData> list = SortAngles(actNode, prevNode);

                    actNode = list[1].node;

                    prevNode = actNode.Previous ?? actNode.List.Last;



                    

                }
                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_X)
                {
                    prevNode = actNode.Value.oppNode;
                    
                    //prevPoint = actNode.Value.pt;
                    actNode = actNode.Value.oppNode.Next ?? actNode.Value.oppNode.List.First;


                   
                }
                else
                {                 
                    MainUnit.PrintText("Error\n");                    
                }
                initial = false;

                
            }
            while (actNode.Value.pt != firstNode.Value.pt);

            if(actNode != firstNode)
            {
                /* point is this same but different figure, check arcs */
                newPolygon.points.First.Value = actNode.Value;
            }

            if( newPolygon.CheckOrientation() == Graph2D.ORIENTATION_et.UNKNOWN)
            {
                return null;
            }

            newPolygon.GetExtPoints();

            foreach(Node n in newPolygon.points)
            {
                n.pt.state = Point2D.STATE_et.USED;
            }

            newPolygon.CheckConsistency();

            return newPolygon;



        }


        private void PrintPolygonData(Polygon p)
        {
            MainUnit.PrintText(" Polygon data: \n ");
            foreach (Node n in p.points)
            {
                MainUnit.PrintText("Node " + n.idx.ToString() + "State " + n.pt.state.ToString() + " Type " + n.pt.storedType.ToString() + " (" + n.pt.x.ToString() + " " + n.pt.y.ToString() + ")");
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

            foreach(Node n in f1.shape.points)
            {
                n.pt.state = Point2D.STATE_et.FREE;
                n.pt.type = Point2D.PointType_et.NORMAL;
            }
            foreach(Polygon p in f1.holes)
            {
                foreach (Node n in p.points)
                {
                    n.pt.state = Point2D.STATE_et.FREE;
                    n.pt.type = Point2D.PointType_et.NORMAL;
                }
            }

            foreach (Node n in f2.shape.points)
            {
                n.pt.state = Point2D.STATE_et.FREE;
                n.pt.type = Point2D.PointType_et.NORMAL;
            }
            foreach (Polygon p in f2.holes)
            {
                foreach (Node n in p.points)
                {
                    n.pt.state = Point2D.STATE_et.FREE;
                    n.pt.type = Point2D.PointType_et.NORMAL;
                }
            }

            /* find crossing points */

            int crossingPoints = SelectCrossingPoints(f1.shape, f2.shape);
            //return null;

            Figure newFigure = null;

            if (crossingPoints > 0)
            {
               
                f1.shape.Renumerate();
                f2.shape.Renumerate();
               /*
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

                        if(newPol == null)
                        {
                            MainUnit.PrintText("Error\n");
                        }
                        else
                        {
                            newFigure.holes.Add(newPol);

                            foreach (Node n in newPol.points)
                            {
                                n.pt.storedType = n.pt.type;
                            }

                            
                        }

                        
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
                    
                    hole.Renumerate();
                    f2.shape.Renumerate();

                    /*
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

                                if (newPol == null)
                                {
                                    MainUnit.PrintText("Error\n");
                                }
                                else
                                {
                                    newFigure.holes.Add(newPol);
                                }
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

                                if (newPol == null)
                                {
                                    MainUnit.PrintText("Error\n");
                                }
                                else
                                {
                                    newFigure.holes.Add(newPol);
                                }
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

                                    if (newPol == null)
                                    {
                                        MainUnit.PrintText("Error\n");
                                    }
                                    else
                                    {
                                        newFigure.holes.Add(newPol);

                                        foreach (Node n in newPol.points)
                                        {
                                            if (n.pt.type != Point2D.PointType_et.NORMAL && n.oppNode == null)
                                            {
                                                MainUnit.PrintText("Error\n");
                                            }
                                            n.pt.storedType = n.pt.type;
                                        }
                                    }
                                }
                            } while (actNode != null);
                        }
                        else
                        {
                            Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(hole1, hole2);

                            if(pos == Polygon.POLYGONS_POS_et.P1_IN_P2)
                            {
                                newFigure.holes.Add(hole1);
                            }
                            else if (pos == Polygon.POLYGONS_POS_et.P2_IN_P1)
                            {
                                newFigure.holes.Add(hole2);
                            }
                            /* check if holes are fully overlaped is necessary */
                        }
                    }
                }
            }

            if (newFigure != null)
            {
                newFigure.containBoard  = f1.containBoard | f2.containBoard;
                /*
                newFigure.name = f1.name + " || " + f2.name;
                newFigure.net = f1.net;
                */
            }


            return newFigure;
        }
    }
}
