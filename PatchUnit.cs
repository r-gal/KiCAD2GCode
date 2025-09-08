using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KiCad2Gcode
{
    internal class PatchUnit
    {

        MainUnit mainUnit;

        public PatchUnit(MainUnit mainUnit)
        {
            this.mainUnit = mainUnit;
        }

        private double GetMiddleAngle(double angle1, double angle2, bool ccw)
        {
            while(angle1 > Math.PI)
            {
                angle1 -= 2*Math.PI;
            }
            while (angle2  > Math.PI)
            {
                angle2 -= 2 * Math.PI;
            }

            while (angle1 <= -Math.PI)
            {
                angle1 += 2 * Math.PI;
            }
            while (angle2 <= -Math.PI)
            {
                angle2 += 2 * Math.PI;
            }

            if (ccw)
            {
                if (angle2 >= angle1)
                {
                    return (angle1 + angle2) / 2;
                }
                else
                {
                    angle2 += 2 * Math.PI;
                    return (angle1 + angle2) / 2;
                }
            }
            else
            {
                if (angle1 >= angle2)
                {
                    return (angle1 + angle2) / 2;
                }
                else
                {
                    angle1 += 2 * Math.PI;
                    return (angle1 + angle2) / 2;
                }
            }
        }

        private LinkedListNode<Node> SelectNextNode(LinkedListNode<Node> actNode, Polygon orgPolygon, double toolRadius)
        {
            if (actNode.Value.oppNode == null)
            {
                return actNode.Next ?? actNode.List.First;
            }
            else
            {
                LinkedListNode<Node> next1 = actNode.Value.oppNode;
                next1 = next1.Next ?? actNode.List.First;
                LinkedListNode<Node> next2 = actNode.Next ?? actNode.List.First;



                if ((next1.Value.pt.state == Point2D.STATE_et.FREE || next1.Value.pt.state == Point2D.STATE_et.ALREADY_USED)  && (next2.Value.pt.state == Point2D.STATE_et.FREE || next2.Value.pt.state == Point2D.STATE_et.ALREADY_USED))
                {
                    /* one of lines probably have collision somewhere in middle oh them. additional checks is necessary */

                    Point2D actPt = actNode.Value.pt;

                    Point2D nextPt = next1.Value.pt;

                    Point2D middlePoint;

                    if (next1.Value.arc == null)
                    {
                        middlePoint = new Point2D((actPt.x + nextPt.x) / 2, (actPt.y + nextPt.y) / 2);
                    }
                    else
                    {
                        double middleAngle = GetMiddleAngle(next1.Value.arc.startAngle, next1.Value.arc.endAngle, next1.Value.arc.ccw);
                        double x = next1.Value.arc.centre.x + next1.Value.arc.radius * Math.Cos(middleAngle);
                        double y = next1.Value.arc.centre.y + next1.Value.arc.radius * Math.Sin(middleAngle);
                        middlePoint = new Point2D(x, y);
                    }

                    //mainForm.drawer.DrawDot(middlePoint, 3, Color.Red);

                    if( CheckPointCollision(middlePoint,orgPolygon, toolRadius) == false)
                    {
                        return next1;
                    }

                    nextPt = next2.Value.pt;
                    if (next2.Value.arc == null)
                    {
                        middlePoint = new Point2D((actPt.x + nextPt.x) / 2, (actPt.y + nextPt.y) / 2);
                    }
                    else
                    {
                        double middleAngle = GetMiddleAngle(next2.Value.arc.startAngle, next2.Value.arc.endAngle, next2.Value.arc.ccw);
                        double x = next2.Value.arc.centre.x + next2.Value.arc.radius * Math.Cos(middleAngle);
                        double y = next2.Value.arc.centre.y + next2.Value.arc.radius * Math.Sin(middleAngle);
                        middlePoint = new Point2D(x, y);
                    }

                    if (CheckPointCollision(middlePoint, orgPolygon, toolRadius) == false)
                    {
                        return next2;
                    }

                }
                else if (next1.Value.pt.state == Point2D.STATE_et.FREE || next1.Value.pt.state == Point2D.STATE_et.ALREADY_USED)
                {
                    actNode = next1;
                }
                else if (next2.Value.pt.state == Point2D.STATE_et.FREE || next2.Value.pt.state == Point2D.STATE_et.ALREADY_USED)
                {
                    actNode = next2;
                }
                else
                {
                    /* fail*/
                    actNode = null;
                }


                return actNode;

            }
        }

        private Polygon CreatePolygon(LinkedListNode<Node> startNode, Polygon orgPolygon, double toolRadius)
        {
            LinkedListNode<Node> actNode = startNode;
            LinkedListNode<Node> firstNode = startNode;

           // mainForm.PrintText("start at idx = " + firstNode.Value.idx.ToString() + "\n");

            Polygon newPolygon = new Polygon();

            do
            {
                Node n = actNode.Value;
                Node newNode = new Node();
                newNode.pt = n.pt;
                newNode.arc = n.arc;
                newNode.oppNode = null;
                LinkedListNode<Node> copiedNode = new LinkedListNode<Node>(newNode);

                newPolygon.points.AddLast(copiedNode);

                if(actNode.Value.pt.state != Point2D.STATE_et.FREE)
                {
                    return null;
                }

                actNode.Value.pt.state = Point2D.STATE_et.ALREADY_USED;
                //if(actNode.Value.oppNode != null ) { actNode.Value.oppNode.Value.state = Node.STATE_et.ALREADY_USED; }

                if(actNode.Value.idx == 59)
                {
                    int trap = 0;
                }

                actNode = SelectNextNode(actNode, orgPolygon, toolRadius);
                /*
                if(actNode == null)
                {
                    mainForm.PrintText("found null \n");
                    return null;
                }
                else
                {
                    mainForm.PrintText("go to idx = " + actNode.Value.idx.ToString() + "\n");
                }*/
                
            }
            while (actNode.Value.pt != firstNode.Value.pt);

            if(actNode != firstNode && firstNode.Value.oppNode != null && firstNode.Value.oppNode == actNode)
            {
                newPolygon.points.First.Value.arc = actNode.Value.arc;
            }
 
            return newPolygon;
        }

        public List<Polygon> CreatePatch(Polygon polygon, double toolDiameter_)
        {
            double toolRadius = toolDiameter_ / 2;
            List<Polygon> pathList = new List<Polygon>();

            Polygon path = new Polygon();
            /* 1. copy polygon to patch, calc input and output vector */

            Point2D prevPoint = polygon.points.Last.Value.pt;



            Vector prevOut;
            if(polygon.points.Last.Value.arc == null)
            {
                prevOut = polygon.points.Last.Value.pt - polygon.points.Last.Previous.Value.pt;
            }
            else
            {
                Vector v = polygon.points.Last.Value.pt - polygon.points.Last.Value.arc.centre;
                prevOut = v.GetOrtogonal(polygon.points.Last.Value.arc.ccw);
            }

            foreach (Node n in polygon.points)
            {
                Node patchNode = new Node();

                patchNode.startPt = new Point2D(prevPoint);
                patchNode.pt = new Point2D(n.pt);
                prevPoint = patchNode.pt;

                Vector vIn,  vOut;


                if(n.arc != null)
                {
                    /*arc */
                    patchNode.arc = new Arc();
                    patchNode.arc.ccw = n.arc.ccw;
                    patchNode.arc.centre = n.arc.centre;    
                    patchNode.arc.startAngle = n.arc.startAngle;
                    patchNode.arc.endAngle = n.arc.endAngle;
                    patchNode.arc.radius = n.arc.radius;

                    Vector vi = patchNode.startPt - patchNode.arc.centre;
                    Vector vo = patchNode.pt - patchNode.arc.centre;

                    patchNode.vIn = vi.GetOrtogonal(!patchNode.arc.ccw);
                    patchNode.vOut = vo.GetOrtogonal(!patchNode.arc.ccw);

                }
                else
                {
                    patchNode.vIn = patchNode.pt - patchNode.startPt;
                    patchNode.vOut = patchNode.vIn;
                }

                path.points.AddLast(patchNode);
            }

            /* 2. add offsets */

            LinkedListNode<Node> actNode = path.points.First;
            LinkedListNode<Node> prevNode = path.points.Last;

            


            while (actNode != null)
            {
                Point2D newStartPoint;
                Point2D newEndPoint;

                actNode.Value.oldPt = actNode.Value.pt;

                if (actNode.Value.arc == null)
                {
                    /*line */
                    Vector shiftV = actNode.Value.pt - actNode.Value.startPt;
                    shiftV = shiftV.GetOrtogonal(false);
                    shiftV.Normalize();
                    shiftV *= toolRadius;

                    newStartPoint = actNode.Value.startPt + shiftV;
                    newEndPoint = actNode.Value.pt + shiftV;
                }
                else
                {
                    /*arc*/
                    double ccwRatio = actNode.Value.arc.ccw ? -1 : 1;
                    Vector shiftV = actNode.Value.startPt - actNode.Value.arc.centre;
                    shiftV.Normalize();
                    shiftV *= toolRadius;
                    newStartPoint = actNode.Value.startPt + ccwRatio * shiftV;

                    shiftV = actNode.Value.pt - actNode.Value.arc.centre;
                    shiftV.Normalize();
                    shiftV *= toolRadius;
                    newEndPoint = actNode.Value.pt + ccwRatio * shiftV;

                    actNode.Value.arc.radius += ccwRatio * toolRadius;
                    if (actNode.Value.arc.radius <= 0)
                    {
                        /* arc not millable will be fittered so replace by line */
                        actNode.Value.arc = null ;
                    }                   
                        
                }
                actNode.Value.startPt = newStartPoint;
                actNode.Value.pt = newEndPoint;

                prevNode = actNode;
                actNode = actNode.Next;
            }

            /*3. join segments using arcs or dummy segments */

            actNode = path.points.First;
            prevNode = path.points.Last;

            while (actNode != null)
            {

                double angle = Vector.AngleBetween(prevNode.Value.vOut, actNode.Value.vIn);

                if (Math.Abs(angle) < 0.001)
                {

                }
                else if (angle < 0)
                {
                    Node arcNode = new Node();
                    arcNode.arc = new Arc();
                    arcNode.arc.centre = prevNode.Value.oldPt;
                    arcNode.pt = actNode.Value.startPt ;
                    arcNode.startPt = prevNode.Value.pt;
                    arcNode.arc.radius = toolRadius;
                    arcNode.arc.startAngle = Math.Atan2(arcNode.startPt.y - arcNode.arc.centre.y, arcNode.startPt.x - arcNode.arc.centre.x);
                    arcNode.arc.endAngle = Math.Atan2(arcNode.pt.y - arcNode.arc.centre.y, arcNode.pt.x - arcNode.arc.centre.x);
                    path.points.AddBefore(actNode, arcNode);

                }
                else
                {

                    /*just connect points using dummy line. It will be filtered in next phase */
                    Node lineNode = new Node();
                    lineNode.pt = actNode.Value.startPt;
                    lineNode.startPt = prevNode.Value.pt;
                    path.points.AddBefore(actNode, lineNode);

                    prevNode.Value.pt.type = Point2D.PointType_et.CROSS_X;


                }
                prevNode = actNode;
                actNode = actNode.Next;
            }   



            path.points.First.Value.startPt = path.points.Last.Value.pt;
            /* select cross points */

            actNode = path.points.First;
            prevNode = path.points.Last;

            if (false) //while (actNode != null)
            {
                if(prevNode.Value.pt.type == Point2D.PointType_et.CROSS_X)
                {
                    CrossUnit crossUnit = new CrossUnit();

                    List<Point2D> crossPoint = crossUnit.GetCrosssingPoints(prevNode, actNode);

                    if(crossPoint != null)
                    {
                        prevNode.Value.pt = crossPoint[0];
                        if (prevNode.Value.arc != null)
                        {
                            prevNode.Value.arc.endAngle = Math.Atan2(prevNode.Value.pt.y - prevNode.Value.arc.centre.y, prevNode.Value.pt.x - prevNode.Value.arc.centre.x);
                        }

                        actNode.Value.startPt = crossPoint[0];
                        if (actNode.Value.arc != null)
                        {
                            actNode.Value.arc.startAngle = Math.Atan2(actNode.Value.startPt.y - actNode.Value.arc.centre.y, actNode.Value.startPt.x - actNode.Value.arc.centre.x);
                        }
                    }
                    else
                    {
                        /*just connect points using dummy line. It will be filtered in next phase */
                        Node lineNode = new Node();
                        lineNode.pt = actNode.Value.startPt;
                        lineNode.startPt = prevNode.Value.pt;
                        path.points.AddBefore(actNode, lineNode);
                    }

                }
                prevNode = actNode;
                actNode = actNode.Next;
            }

            /* self check and clean-up before next phase */

            actNode = path.points.First;
            prevNode = path.points.Last;

            while (actNode != null)
            {
                if(actNode.Value.startPt != prevNode.Value.pt)
                {
                    actNode.Value.startPt = prevNode.Value.pt;
                }

                actNode.Value.startPt = null;
                actNode.Value.pt.type = Point2D.PointType_et.NORMAL;
                prevNode = actNode;
                actNode = actNode.Next;
            }

            /* find crossing points */

            LinkedListNode<Node> node1 = path.points.First;
            LinkedListNode<Node> node2;

            bool crossesFound = false;

            while(node1 != null)
            {
                node2 = node1.Next;

                while(node2 != null)
                {
                    CrossUnit crossUnit = new CrossUnit();

                    Point2D testPt1 = new Point2D(6.4, 44.7);
                    Point2D testPt2 = new Point2D(6.4, 44.8);

                    if (node1.Value.pt.IsSameAs(testPt1) && node2.Value.pt.IsSameAs(testPt2))
                    {
                        int trap = 0;
                    }


                    List<Point2D> crossPoint = crossUnit.GetCrosssingPoints(node1, node2);


                    if(crossPoint != null)
                    {
                        if(crossPoint.Count == 2)
                        {                            
                            //if (crossPoint[1].type != Point2D.PointType_et.CROSS_X) { crossPoint.RemoveAt(1); } 
                        }
                        if(crossPoint.Count  > 0)
                        {
                            //if (crossPoint[0].type != Point2D.PointType_et.CROSS_X) { crossPoint.RemoveAt(0); }
                        }
                    }

                    if(crossPoint != null && crossPoint.Count > 0)
                    {


                        /* cut f1 */

                        node1 = Figure.SplitChunk2(node1, crossPoint);

                        /* cut f2 */

                        node2 = Figure.SplitChunk2(node2, crossPoint);

                        if (crossPoint.Count == 1)
                        {
                            /* easy case */
                            node1.Value.oppNode = node2;
                            node2.Value.oppNode = node1;

                        }
                        else
                        {
                            if (node1.Value.pt == node2.Value.pt)
                            {
                                node1.Value.oppNode = node2;
                                node2.Value.oppNode = node1;

                                node1.Next.Value.oppNode = node2.Next ?? node2.List.First;
                                node2.Next.Value.oppNode = node1.Next ?? node1.List.First;
                            }
                            else
                            {
                                node1.Value.oppNode = node2.Next ?? node2.List.First;
                                node2.Value.oppNode = node1.Next ?? node1.List.First;

                                node1.Next.Value.oppNode = node2;
                                node2.Next.Value.oppNode = node1;

                            }
                        }



                        crossesFound = true;

                    }
                    node2 = node2.Next;
                }
                node1 = node1.Next;
            }

            if(crossesFound)
            {
                /* path polygon contain crosses, so it need to be fixed. */

                /* Find collisions point - figure */
                foreach(Node n in path.points)
                {
                    bool colission = CheckPointCollision(n.pt,polygon,toolRadius);
                                        
                    if (colission)
                    {
                        n.pt.state = Point2D.STATE_et.BAD;
                        //n.pt.type = Point2D.PointType_et.BAD;
                    }
                }

                path.Renumerate();
                /*
                foreach (Node n in path.points)
                {
                    mainForm.PrintText("Node " + n.idx.ToString() + "State " + n.pt.state.ToString() +  " Type " + n.pt.type.ToString() + " (" + n.pt.x.ToString() + " " + n.pt.y.ToString() + ")");
                    if(n.oppNode!= null)
                    {
                        mainForm.PrintText(" OppIdx = " + n.oppNode.Value.idx.ToString() + " " + n.oppNode.Value.pt.type.ToString());
                    }
                    if(n.arc != null)
                    {
                        mainForm.PrintText(" Arc ");
                    }
                    mainForm.PrintText("\n ");
                }*/
                

                LinkedListNode<Node> startScanNode = path.points.First;
                    bool cont = false;
                do
                {
                    cont = false;
                    LinkedListNode<Node> node = startScanNode;
                    LinkedListNode<Node> firstNode = null;

                    while (node != null)
                    {
                        if (node.Value.pt.state == Point2D.STATE_et.FREE)
                        {
                            firstNode = node;
                            cont = true;
                            break;
                        }

                        node = node.Next;
                    }


                    if(firstNode != null)
                    {
                        Polygon p = CreatePolygon(firstNode,polygon,toolRadius);

                        if(p != null)
                        {
                            foreach(Node n in path.points)
                            {
                                if(n.pt.state == Point2D.STATE_et.ALREADY_USED)
                                {
                                    n.pt.state = Point2D.STATE_et.USED;
                                }
                            }
                            pathList.Add(p);
                        }
                        else
                        {
                            mainUnit.PrintText("Path aborted\n ");
                            foreach (Node n in path.points)
                            {
                                if (n.pt.state == Point2D.STATE_et.ALREADY_USED)
                                {
                                    n.pt.state = Point2D.STATE_et.FREE;
                                }
                            }
                            //firstNode.Value.pt.state = Point2D.STATE_et.BAD;
                            
                        }
                        startScanNode = firstNode.Next;
                    }
                    else if (pathList.Count == 0)
                    {
                        mainUnit.PrintText("Start point not found\n ");
                    }


                } while (cont == true);
                //pathList.Add(path);

            }
            else
            {
                pathList.Add(path);
            }

            return pathList;


        }

        private bool CheckPointCollision(Point2D testedPoint, Polygon orgPolygon, double toolRadius)
        {
            LinkedListNode<Node> orgNode = orgPolygon.points.First;
            LinkedListNode<Node> prevNode = orgPolygon.points.Last;
            

            while (orgNode != null)
            {
                if (orgNode.Value.arc != null)
                {
                    Arc arc = orgNode.Value.arc;

                    Vector vl = testedPoint - arc.centre;


                    if (vl.Length < arc.radius + toolRadius - 0.00001 && vl.Length > arc.radius - toolRadius + 0.00001)
                    {
                        double angle = Math.Atan2(vl.y, vl.x);

                        if (Graph2D.IsAngleBetween(angle, arc.startAngle, arc.endAngle, arc.ccw))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    Vector v = orgNode.Value.pt - prevNode.Value.pt;
                    double length = v.Length;
                    v.Normalize();

                    double sX = prevNode.Value.pt.x;
                    double sY = prevNode.Value.pt.y;

                    double pX = testedPoint.x;
                    double pY = testedPoint.y;

                    double a = pX * v.x - sX * v.x - sY * v.y + pY * v.y;

                    if (a >= 0 && a <= length)
                    {
                        double b = pX * v.y - sX * v.y - pY * v.x + sY * v.x;

                        if (Math.Abs(b) < toolRadius - 0.00001)
                        {
                            return true;
                        }
                    }
                }

                prevNode = orgNode;
                orgNode = orgNode.Next;
            }
            return false;
        }
    }


}
