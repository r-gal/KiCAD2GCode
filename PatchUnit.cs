using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace KiCad2Gcode
{
    internal class PatchUnit
    {

        Form1 mainForm;

        public PatchUnit(Form1 mainForm)
        {
            this.mainForm = mainForm;
        }

        private Polygon CreatePolygon(LinkedListNode<Node> startNode)
        {
            LinkedListNode<Node> actNode = startNode;
            LinkedListNode<Node> firstNode = startNode;

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


                if (actNode.Value.pt.type == Point2D.PointType_et.NORMAL)
                {
                   // actNode.Value.pt.type = Point2D.PointType_et.USED; /* reset type */
                    actNode = actNode.Next ?? actNode.List.First;
                }                
                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_X)
                {
                   // actNode.Value.pt.type = Point2D.PointType_et.USED; /* reset type */

                    if(actNode.Value.oppNode.Value.pt.type != Point2D.PointType_et.BAD)
                    {
                        actNode = actNode.Value.oppNode;
                    }
                    else
                    {
                        actNode = actNode.Next ?? actNode.List.First;
                        if(actNode.Value.pt.type == Point2D.PointType_et.BAD)
                        {
                            int x = 0;
                        }
                    }
                    
                }
                else
                {
                    /* invalid point */
                    //actNode.Value.pt.type = Point2D.PointType_et.USED; /* reset type */
                    actNode = actNode.Next ?? actNode.List.First;
                }
                
            }
            while (actNode.Value.pt != firstNode.Value.pt);
 
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

            /* add offsets and arcs */

            LinkedListNode<Node> actNode = path.points.First;
            LinkedListNode<Node> prevNode = path.points.Last;

            


            while (actNode != null)
            {
                Point2D newStartPoint;
                Point2D newEndPoint;

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

                double angle = Vector.AngleBetween(prevNode.Value.vOut, actNode.Value.vIn);

                if(Math.Abs(angle) < 0.001)
                {
                    actNode.Value.startPt = prevNode.Value.pt;
                    actNode.Value.pt = newEndPoint;
                }
                else if( angle < 0)
                {
                    Node arcNode = new Node();
                    arcNode.arc = new Arc();
                    arcNode.arc.centre = new Point2D(actNode.Value.startPt);
                    arcNode.pt = newStartPoint;
                    arcNode.startPt = prevNode.Value.pt;
                    arcNode.arc.radius = toolRadius;
                    arcNode.arc.startAngle = Math.Atan2(arcNode.startPt.y - arcNode.arc.centre.y, arcNode.startPt.x - arcNode.arc.centre.x);
                    arcNode.arc.endAngle = Math.Atan2(arcNode.pt.y - arcNode.arc.centre.y, arcNode.pt.x - arcNode.arc.centre.x);
                    path.points.AddBefore(actNode, arcNode);
                    actNode.Value.startPt = newStartPoint;
                    actNode.Value.pt = newEndPoint;
                }
                else
                {
                    actNode.Value.startPt = newStartPoint;
                    actNode.Value.pt = newEndPoint;

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

                    Point2D[] crossPoint = crossUnit.GetCrosssingPoints(prevNode, actNode);

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
                    Point2D[] crossPoint = crossUnit.GetCrosssingPoints(node1, node2);

                    if(crossPoint != null)
                    {
                        if(crossPoint.Length == 1)
                        {
                            if (crossPoint[0].type == Point2D.PointType_et.CROSS_X)
                            {
                                /* cut f1 */

                                Figure.SplitChunk(node1, crossPoint);

                                /* cut f2 */

                                Figure.SplitChunk(node2, crossPoint);

                                node1.Value.oppNode = node2.Next ?? node2.List.First;
                                node2.Value.oppNode = node1.Next ?? node1.List.First;

                                crossesFound = true;
                            }
                        }
                        else
                        {
                            /* is it possible ? */
                        }
                    }
                    node2 = node2.Next;
                }
                node1 = node1.Next;
            }

            if(crossesFound)
            {
                /* path polygon contain crosses, so it need to be fixed. */

                /* Find collisions point - figure */

                foreach (Node n in path.points)
                {
                    bool colission = false;

                    LinkedListNode<Node> orgNode = polygon.points.First;
                    prevNode = polygon.points.Last;


                    while(orgNode != null)
                    {
                        if( orgNode.Value.arc != null)
                        {
                            Arc arc = orgNode.Value.arc;

                            Vector vl = n.pt - arc.centre;


                            if(vl.Length < arc.radius + toolRadius - 0.00001  && vl.Length > arc.radius - toolRadius + 0.00001)
                            {
                                double angle = Math.Atan2(vl.y,vl.x);

                                if(Graph2D.IsAngleBetween(angle, arc.startAngle, arc.endAngle, arc.ccw))
                                {
                                    colission = true;
                                    break;
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

                            double pX = n.pt.x;
                            double pY = n.pt.y;

                            double a = pX * v.x - sX * v.x - sY * v.y + pY * v.y;

                            if(a >= 0 && a <= length)
                            {
                                double b = pX * v.y - sX * v.y - pY * v.x + sY * v.x;

                                if (Math.Abs(b) < toolRadius - 0.00001)
                                {
                                    colission = true;
                                    break;
                                }
                            }
                        }

                        prevNode = orgNode;
                        orgNode = orgNode.Next;
                    }
                    if (colission)
                    {
                        n.pt.type = Point2D.PointType_et.BAD;
                    }
                }


                
                bool cont = false;
                do
                {
                    cont = false;
                    LinkedListNode<Node> node = path.points.First;
                    LinkedListNode<Node> firstNode = null;

                    while (node != null)
                    {
                        if (node.Value.pt.type != Point2D.PointType_et.USED && node.Value.pt.type != Point2D.PointType_et.BAD)
                        {
                            firstNode = node;
                            //cont = true;
                            break;
                        }
                        node = node.Next;
                    }

                    if(firstNode != null)
                    {
                        Polygon p = CreatePolygon(firstNode);

                        if(p != null)
                        {
                            pathList.Add(p);
                        }
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
    }


}
