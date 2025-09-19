using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static KiCad2Gcode.CrossUnit;

namespace KiCad2Gcode
{

    public class Point2D
    {
        public double x;
        public double y;

        public enum PointType_et
        {
            NORMAL,
            CROSS_X,
            CROSS_T,
            CROSS_V,
            DOUBLED,
            BRIDGE,
            CW,
            CCW,
            ARC
        };
        public PointType_et type;

        public enum STATE_et
        {
            FREE,
            USED,
            ALREADY_USED,
            BAD
        }

        public STATE_et state = STATE_et.FREE;

        public Chunk[] chunk = new Chunk[2];

        public Point2D(double x_, double y_)
        {
            this.x = x_;
            this.y = y_;
            type = PointType_et.NORMAL;
        }

        public Point2D(Point2D p)
        {
            this.x = p.x;
            this.y = p.y;
            type = p.type;
        }

        public void Rotate(double angle)
        {
            /* centre of rotation is point (0,0) */
            double newX = x * Math.Cos(angle) + y * Math.Sin(angle);
            double newY = - x * Math.Sin(angle) + y * Math.Cos(angle);
            x = newX;
            y = newY;

        }

        public Vector ToVector()
        {
            Vector v = new Vector(x, y);
            return v;
        }

        public bool IsSameAs(Point2D pt)
        {
            return (Math.Abs(pt.x - x) < 0.00001 && Math.Abs(pt.y - y) < 0.00001);
        }

        public static Point2D operator +(Point2D a, Vector b)
         => new Point2D(a.x + b.x, a.y + b.y);

        public static Point2D operator -(Point2D a, Vector b)
        => new Point2D(a.x - b.x, a.y - b.y);

        public static Vector operator -(Point2D a, Point2D b)
        => new Vector(a.x - b.x, a.y - b.y);
    }


    public class Vector
    {
        public double x;
        public double y;

        public double Length;

        public Vector(double x_, double y_)
        {
            x = x_;
            y = y_;
            Length = Math.Sqrt(x * x + y * y);
        }

        public Vector(Vector a)
        {
            x = a.x;
            y = a.y;
            Length = a.Length;
        }

        public void Normalize()
        {
            x /= Length; y /= Length; Length = 1;

        }

        public static double AngleBetween(Vector v1, Vector v2)
        {
            if (v1.Length > 0 && v2.Length > 0)
            {
                /*
                double angle = (v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length);
                return Math.Acos(angle);
                */
                double a1 = Math.Atan2(v1.y, v1.x);
                double a2 = Math.Atan2(v2.y, v2.x);
                double a = a2 - a1;

                if (a > Math.PI) { a -= 2 * Math.PI; }
                else if (a < -Math.PI) { a += 2 * Math.PI; }
                return a;
            }
            else
            {
                return 0;
            }
        }
        public static double GetAlpha(Point2D pt1, Point2D pt2)
        {
            double x = pt2.x - pt1.x;
            double y = pt2.y - pt1.y;
            
            double d = Math.Abs(x) + Math.Abs(y);

            if (d == 0) return 0;

            double alpha = 0;

            if(y < 0)
            {
                if(x<0)
                {
                    alpha = 2 - y / d;
                }
                else
                {
                    alpha = 4 + y / d;
                }
            }
            else
            {
                if (x < 0)
                {
                    alpha = 2 - y / d;
                }
                else
                {
                    alpha = y / d;
                }
            }
            return alpha;
        }

        public Vector GetOrtogonal(bool ccw)
        {
            Vector nV = new Vector(this);
            if(ccw )
            {
                nV.x = this.y;
                nV.y = -this.x;
            }
            else
            {
                nV.x = -this.y;
                nV.y = this.x;
            }
            return nV;
            
        }

        public static double GetAlpha(Vector v)
        {
            return GetAlpha(new Point2D(0, 0), new Point2D(v.x, v.y));
        }


        
        public static Vector operator *(Vector a, double m)
        => new Vector(a.x * m, a.y * m);
        public static Vector operator *(double m, Vector a)
        => new Vector(a.x * m, a.y * m);

        public static Vector operator +(Vector a, Vector b)
        => new Vector(a.x + b.x, a.y + b.y);

        public static Vector operator -(Vector a, Vector b)
        => new Vector(a.x - b.x, a.y - b.y);
    };

    public class Chunk
    {
        public Chunk() { }

        public Point2D start;
        public Point2D end;
        public enum ChunkType
        {
            Line,
            Arc
        }
        public ChunkType type;

        virtual public void Rotate(double angle) { }
        virtual public void Move(Vector move) { }

        virtual public Chunk Split(Point2D pt) { return null; }
    }

    public class Line:   Chunk
    {
        public Line()
        {
            type = ChunkType.Line;
        }

        override public void Rotate(double angle)
        {
            start.Rotate(angle);
            end.Rotate(angle);
        }

        override public void Move(Vector move)
        {
            start = start + move; 
            end = end + move;

        }
        override public Chunk Split(Point2D pt) 
        {
            Line newLine = new Line();
            newLine.start = pt;
            newLine.end = end;
            this.end = pt;
            return newLine;
        }
    }

    public class Arc : Chunk
    {
        public double startAngle;
        public double endAngle;
        public double radius;

        public Point2D centre;

        public bool ccw;

        public Arc()
        {
            type = ChunkType.Arc;
            ccw = false;
        }

        override public void Rotate(double angle)
        {
            /*start.Rotate(angle);
            end.Rotate(angle);*/
            centre.Rotate(angle);
            startAngle -= angle;
            endAngle -= angle;

            while(startAngle < -Math.PI) { startAngle += 2 * Math.PI;  }
            while (startAngle > Math.PI) { startAngle -= 2 * Math.PI; }
            while (endAngle < -Math.PI) { endAngle += 2 * Math.PI; }
            while (endAngle > Math.PI) { endAngle -= 2 * Math.PI; }

        }

        override public void Move(Vector move)
        {
            /*start = start + move;
            end = end + move;*/
            centre = centre + move;
        }

        override public Chunk Split(Point2D pt)
        {
            return null;
        }
    }



    public class Node
    {
        public bool active = false;

        public Point2D startPt = null; /* used only as temporary value in path unit*/
        public Point2D oldPt = null; /* used only as temporary value in path unit*/
        public Vector vIn, vOut; /* used in path unit */

        public int idx;

        public Point2D pt;
        public Arc arc;

        public LinkedListNode<Node> oppNode = null;



        public Node()
        {
            arc = null;
            pt = null;
            
        }

    }

    public class Polygon : Graph2D
    {
        public LinkedList<Node> points = new LinkedList<Node>();

        int L = 0, U = 1, R = 2, D = 3;

        public Point2D[] extPoint = new Point2D[4];
        public Node[] extNode = new Node[4];

        public int idx;

        public int selected = 0;

        public void Renumerate()
        {
            int cnt = 0;
            foreach (Node n in points)
            {
                n.idx = cnt;
                cnt++;
            }
        }

        public void Rotate(Double angle)
        {
            foreach (Node n in points)
            {
                n.pt.Rotate(angle);
                if (n.arc != null)
                {
                    n.arc.Rotate(angle);
                }
            }
            GetExtPoints();
        }

        public void Move(Vector move)
        {
            foreach (Node n in points)
            {
                n.pt += move;
                if (n.arc != null)
                {
                    n.arc.Move(move);
                }
            }
            GetExtPoints();
        }

        public void GetExtPoints()
        {
            if (points.Count == 0) { return; }
            Point2D prevPt = points.Last.Value.pt;
            LinkedListNode<Node> actNode = points.First;

            for (int i = 0; i < 4; i++)
            {
                extNode[i] = actNode.Value;
                extPoint[i] = actNode.Value.pt;
            }

            do
            {
                Point2D actPt = actNode.Value.pt;

                if (actPt.x < extPoint[L].x) { extPoint[L] = actPt; extNode[L] = actNode.Value; }
                if (actPt.y > extPoint[U].y) { extPoint[U] = actPt; extNode[U] = actNode.Value; }
                if (actPt.x > extPoint[R].x) { extPoint[R] = actPt; extNode[R] = actNode.Value; }
                if (actPt.y < extPoint[D].y) { extPoint[D] = actPt; extNode[D] = actNode.Value; }

                if (actNode.Value.arc != null)
                {
                    Arc arc = actNode.Value.arc;
                    if (arc.centre.x - arc.radius < extPoint[L].x)
                    {
                        if (arc.startAngle < arc.endAngle)
                        {
                            extPoint[L] = new Point2D(arc.centre.x - arc.radius, arc.centre.y);
                            extNode[L] = actNode.Value;
                        }
                    }

                    if (arc.centre.y + arc.radius > extPoint[U].y)
                    {
                        if (Graph2D.IsAngleBetween(Math.PI / 2, arc.startAngle, arc.endAngle, arc.ccw))
                        {
                            extPoint[U] = new Point2D(arc.centre.x, arc.centre.y + arc.radius);
                            extNode[U] = actNode.Value;
                        }
                    }

                    if (arc.centre.x + arc.radius > extPoint[R].x)
                    {
                        if (Graph2D.IsAngleBetween(0, arc.startAngle, arc.endAngle, arc.ccw))
                        {
                            extPoint[R] = new Point2D(arc.centre.x + arc.radius, arc.centre.y);
                            extNode[R] = actNode.Value;
                        }
                    }

                    if (arc.centre.y - arc.radius < extPoint[D].y)
                    {
                        if (Graph2D.IsAngleBetween(-Math.PI / 2, arc.startAngle, arc.endAngle, arc.ccw))
                        {
                            extPoint[D] = new Point2D(arc.centre.x, arc.centre.y - arc.radius);
                            extNode[D] = actNode.Value;
                        }
                    }
                }
                actNode = actNode.Next;
            } while (actNode != null);
        }

        public enum POLYGONS_POS_et
        {
            P1_IN_P2,
            P2_IN_P1,
            NONE
        };

        public static POLYGONS_POS_et CheckPolygonsPosition(Polygon pol1, Polygon pol2)
        {
            /* this function assume that polygons have not any crossing points*/

            LinkedListNode<Node> actNode = pol1.points.First;
            while (actNode != null)
            {
                POINT_LOC_et res = CheckPointInPolygon(actNode.Value.pt, pol2);
                if (res == POINT_LOC_et.IN)
                {
                    return POLYGONS_POS_et.P1_IN_P2;
                }
                else if (res == POINT_LOC_et.OUT)
                {
                    break;
                }
                actNode = actNode.Next;
            }

            actNode = pol2.points.First;
            while (actNode != null)
            {
                POINT_LOC_et res = CheckPointInPolygon(actNode.Value.pt, pol1);
                if (res == POINT_LOC_et.IN)
                {
                    return POLYGONS_POS_et.P2_IN_P1;
                }
                else if (res == POINT_LOC_et.OUT)
                {
                    break;
                }
                actNode = actNode.Next;
            }

            return POLYGONS_POS_et.NONE;
        }

        public enum POINT_LOC_et
        {
            IN,
            OUT,
            EDGE
        };

        public static POINT_LOC_et CheckPointInPolygon(Point2D pt, Polygon pol)
        {
            int crosses = 0;

            int state = 0; /* -1: DN, 0 : IDLE, 1 : UP */


            LinkedListNode<Node> actNode = pol.points.First;

            CrossUnit crossUnit = new CrossUnit();

            do
            {

                CROSS_TYPE_et result = crossUnit.CheckFlatCross(pt, actNode);

                //MainUnit.PrintText("Test pt = (" + pt.x.ToString() + " " + pt.y.ToString() + ") vs node " + actNode.Value.idx.ToString() + " result = " + result.ToString() + "\n");

                switch (result)
                {
                    case CROSS_TYPE_et.NORMAL:
                        state = 0;
                        crosses++;
                        break;
                    case CROSS_TYPE_et.DOUBLE:
                        state = 0;
                        crosses += 2;
                        break;
                    case CROSS_TYPE_et.END_DN:
                        if (state == 1)
                        {
                            state = 0;
                            crosses++;
                        }
                        else if (state == -1)
                        {
                            state = 0;
                        }
                        else
                        {
                            state = -1;
                        }
                        break;

                    case CROSS_TYPE_et.END_UP:
                        if (state == -1)
                        {
                            state = 0;
                            crosses++;
                        }
                        else if (state == 1)
                        {
                            state = 0;
                        }
                        else
                        {
                            state = 1;
                        }
                        break;

                    case CROSS_TYPE_et.END2_DN:
                        if (state == 1)
                        {
                            state = 0;
                            crosses += 2;
                        }
                        else if (state == -1)
                        {
                            state = 0;
                            crosses += 1;
                        }
                        else
                        {
                            state = -1;
                            crosses += 1;
                        }
                        break;

                    case CROSS_TYPE_et.END2_UP:
                        if (state == -1)
                        {
                            state = 0;
                            crosses += 2;
                        }
                        else if (state == 1)
                        {
                            state = 0;
                            crosses += 1;
                        }
                        else
                        {
                            state = 1;
                            crosses += 1;
                        }
                        break;
                    case CROSS_TYPE_et.EDGE:
                        return POINT_LOC_et.EDGE;
                }



                actNode = actNode.Next;
            } while (actNode != null);


            if (state == 0)
            {
                if (crosses % 2 == 0)
                {
                    return POINT_LOC_et.OUT;
                }
                else
                {
                    return POINT_LOC_et.IN;
                }
            }
            else
            {

                /* probably point is on edge */
                return POINT_LOC_et.EDGE;
            }
        }



        public ORIENTATION_et CheckOrientation()
        {
            if (points.Count == 0)
            {
                return ORIENTATION_et.UNKNOWN;
            }
            else if (points.Count == 1)
            {
                if (points.First.Value.arc != null)
                {
                    if (points.First.Value.arc.ccw)
                    {
                        return ORIENTATION_et.CCW;
                    }
                    else
                    {
                        return ORIENTATION_et.CW;
                    }
                }
                else
                {
                    return ORIENTATION_et.UNKNOWN;
                }
            }
            else if (points.Count == 2)
            {
                double w1 = 0;
                double w2 = 0;

                if (points.First.Value.arc != null)
                {
                    double r1 = points.First.Value.arc.radius;
                    if (points.First.Value.arc.ccw) r1 = -r1;
                    w1 = 1 / r1;
                }
                if (points.Last.Value.arc != null)
                {
                    double r2 = points.Last.Value.arc.radius;
                    if (points.Last.Value.arc.ccw) r2 = -r2;
                    w2 = 1 / r2;
                }
                if(w1 + w2 < 0)
                {
                    return ORIENTATION_et.CCW;
                }
                else if(w1 + w2 > 0 )
                {
                    return ORIENTATION_et.CW;
                }
                else
                {
                    return ORIENTATION_et.UNKNOWN;
                }
            }
            else
            {
                /* regular case */

                /*find point located  most down - right */

                LinkedListNode<Node> selNode = points.First;

                LinkedListNode<Node> actNode = points.First;

                while(actNode != null)
                {
                    if(actNode.Value.pt.y < selNode.Value.pt.y)
                    {
                        selNode = actNode;
                    }
                    else if(actNode.Value.pt.y == selNode.Value.pt.y)
                    {
                        if (actNode.Value.pt.x > selNode.Value.pt.x)
                        {
                            selNode = actNode;
                        }
                    }
                    actNode = actNode.Next;
                }

                LinkedListNode<Node> prevNode = selNode.Previous ?? points.Last;
                LinkedListNode<Node> nextNode = selNode.Next ?? points.First;

                return Graph2D.CheckTriangleOrientation(prevNode.Value.pt, selNode.Value.pt, nextNode.Value.pt);
            }
        }

        public void SetOrientation(ORIENTATION_et orientation)
        {
            ORIENTATION_et actOrientation = CheckOrientation();
            if (actOrientation == orientation)
            {
                return;
            }
            else if (actOrientation != ORIENTATION_et.UNKNOWN && orientation != ORIENTATION_et.UNKNOWN)
            {
                /* change orientation */


                Node prevNode = points.Last.Value;

                foreach (Node n in points)
                {
                    n.startPt = prevNode.pt;
                    prevNode = n;
                }                

                foreach (Node n in points)
                {
                    n.pt = n.startPt;
                    n.startPt = null;

                    if (n.arc != null)
                    {
                        Double tmpAngle = n.arc.startAngle;
                        n.arc.startAngle = n.arc.endAngle;
                        n.arc.endAngle = tmpAngle;

                        n.arc.ccw = !n.arc.ccw;
                    }
                }
                LinkedList<Node> newList = new LinkedList<Node>();

                while(points.Count> 0)
                {
                    Node nn = points.Last();
                    points.RemoveLast();
                    newList.AddLast(nn);


                };

                points = newList;
            }
        }

        public Polygon GetValidPolygon()
        {
            if (points.Count < 3)
            {
                return this;
            }

            LinkedListNode<Node> node1 = points.First;
            LinkedListNode<Node> node2 = points.Last;
            /* select cross points */
            bool crossesFound = false;

            while (node1 != null)
            {
                node2 = node1.Next;

                while (node2 != null)
                {
                    CrossUnit crossUnit = new CrossUnit();

                    List<Point2D> crossPoint = crossUnit.GetCrosssingPoints(node1, node2);

                    if (crossPoint != null && crossPoint.Count > 0)
                    {


                        /* cut f1 */

                        node1 = Figure.SplitChunk(node1, crossPoint);

                        /* cut f2 */

                        node2 = Figure.SplitChunk(node2, crossPoint);

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
                Polygon np = new Polygon();

                LinkedListNode<Node> startNode = points.First;
                LinkedListNode<Node> actNode = startNode;

                do
                {
                    np.points.AddLast(actNode.Value);

                    if (actNode.Value.pt.type == Point2D.PointType_et.NORMAL)
                    {
                        actNode = actNode.Next ?? actNode.List.First;
                    }
                    else
                    {                        
                        actNode = actNode.Value.oppNode;
                        actNode = actNode.Next ?? actNode.List.First;
                    }

                } while (startNode.Value.pt != actNode.Value.pt);

                return np;

            }
            else
            {
                return this;
            }
        }
    }

    public class Figure
    {

        //public List<Chunk> chunks = new List<Chunk>();

        public Polygon shape = new Polygon();

        public List<Polygon> holes = new List<Polygon>();

        public bool touched = false;

        public string name;

        public int net = -1;

        public int idx = 0;

        public void Rotate(double angle)
        {
            shape.Rotate(angle);
            foreach (Polygon h in holes)
            {
                h.Rotate(angle);
            }
        }
        public void Move(Vector move)
        {
            shape.Move(move);
            foreach (Polygon h in holes)
            {
                h.Move(move);
            }
        }

        public static LinkedListNode<Node> SplitChunk(LinkedListNode<Node> node, List<Point2D> pointArr)
        {
            LinkedListNode<Node> prevNode = node.Previous ?? node.List.Last;


            Point2D sPt = prevNode.Value.pt;
            Point2D ePt = node.Value.pt;

            int fail = 0;

            if (pointArr.Count < 1)
            {
                return node;
            }

            if (pointArr[0].type == Point2D.PointType_et.NORMAL)
            {
                fail++;
            }
            if (pointArr.Count == 2 && pointArr[1].type == Point2D.PointType_et.NORMAL)
            {
                fail++;
            }

            if (pointArr.Count == 1)
            {
                /*easy case */
                if (sPt.IsSameAs(pointArr[0]))
                {
                    prevNode.Value.pt = pointArr[0];

                }
                else if (ePt.IsSameAs(pointArr[0]))
                {
                    node.Value.pt = pointArr[0];

                }
                else
                {
                    /* regular split */

                    Node newNode = new Node();
                    newNode.pt = pointArr[0];
                    newNode.oppNode = null;

                    if (node.Value.arc != null)
                    {
                        double angle = Math.Atan2(pointArr[0].y - node.Value.arc.centre.y, pointArr[0].x - node.Value.arc.centre.x);
                        newNode.arc = new Arc();
                        newNode.arc.startAngle = node.Value.arc.startAngle;
                        newNode.arc.endAngle = angle;
                        node.Value.arc.startAngle = angle;
                        newNode.arc.radius = node.Value.arc.radius;
                        newNode.arc.centre = node.Value.arc.centre;
                        newNode.arc.ccw = node.Value.arc.ccw;
                    }
                    LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                    node.List.AddBefore(node, newElement);
                    node =  newElement;
                }
            }
            else if (pointArr.Count == 2)
            {
                /* phase 1 - sort merging points */

                Point2D pt1 = null;
                Point2D pt2 = null;

                if (node.Value.arc == null)
                {
                    if (Math.Abs(ePt.x - sPt.x) > Math.Abs(ePt.y - sPt.y))
                    {
                        if (ePt.x > sPt.x)
                        {
                            if (pointArr[0].x < pointArr[1].x)
                            {
                                /* point 0 is first */
                                pt1 = pointArr[0];
                                pt2 = pointArr[1];
                            }
                            else
                            {
                                /* point 1 is first */
                                pt1 = pointArr[1];
                                pt2 = pointArr[0];
                            }
                        }
                        else
                        {
                            if (pointArr[0].x < pointArr[1].x)
                            {
                                /* point 1 is first */
                                pt1 = pointArr[1];
                                pt2 = pointArr[0];
                            }
                            else
                            {
                                /* point 0 is first */
                                pt1 = pointArr[0];
                                pt2 = pointArr[1];
                            }
                        }
                    }
                    else
                    {
                        if (ePt.y > sPt.y)
                        {
                            if (pointArr[0].y < pointArr[1].y)
                            {
                                /* point 0 is first */
                                pt1 = pointArr[0];
                                pt2 = pointArr[1];
                            }
                            else
                            {
                                /* point 1 is first */
                                pt1 = pointArr[1];
                                pt2 = pointArr[0];
                            }
                        }
                        else
                        {
                            if (pointArr[0].y < pointArr[1].y)
                            {
                                /* point 1 is first */
                                pt1 = pointArr[1];
                                pt2 = pointArr[0];
                            }
                            else
                            {
                                /* point 0 is first */
                                pt1 = pointArr[0];
                                pt2 = pointArr[1];
                            }
                        }
                    }

                    if (ePt.IsSameAs(pt2))
                    {
                        node.Value.pt = pt2;

                    }
                    else
                    {
                        /* regular split */

                        Node newNode = new Node();
                        newNode.pt = pt2;
                        newNode.oppNode = null;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddBefore(node, newElement);

                        node = newElement;
                    }

                    if (sPt.IsSameAs(pt1))
                    {
                        prevNode.Value.pt = pt1;
                    }
                    else
                    {
                        /* regular split */

                        Node newNode = new Node();
                        newNode.pt = pt1;
                        newNode.oppNode = null;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddBefore(node, newElement);

                        node = newElement;
                    }

                    


                }
                else
                {
                    double angleA = Math.Atan2(pointArr[0].y - node.Value.arc.centre.y, pointArr[0].x - node.Value.arc.centre.x);
                    double angleB = Math.Atan2(pointArr[1].y - node.Value.arc.centre.y, pointArr[1].x - node.Value.arc.centre.x);
                    double angle1 = 0;
                    double angle2 = 0;

                    if (Graph2D.IsAngleBetween(angleA, node.Value.arc.startAngle, angleB, node.Value.arc.ccw) == true || ePt.IsSameAs(pointArr[1]))
                    {
                        /* point 1 is first */
                        pt1 = pointArr[0];
                        pt2 = pointArr[1];
                        angle1 = angleA;
                        angle2 = angleB;
                    }
                    else if (Graph2D.IsAngleBetween(angleB, node.Value.arc.startAngle, angleA, node.Value.arc.ccw) == true || ePt.IsSameAs(pointArr[0]))
                    {
                        /* point 0 is first */
                        pt1 = pointArr[1];
                        pt2 = pointArr[0];
                        angle1 = angleB;
                        angle2 = angleA;
                    }
                    

                    if (ePt.IsSameAs(pt2))
                    {
                        node.Value.pt = pt2;

                    }
                    else
                    {
                        /* regular split */

                        Node newNode = new Node();
                        newNode.pt = pt2;
                        newNode.oppNode = null;
                        newNode.arc = new Arc();
                        newNode.arc.startAngle = node.Value.arc.startAngle;
                        newNode.arc.endAngle = angle2;
                        node.Value.arc.startAngle = angle2;
                        newNode.arc.radius = node.Value.arc.radius;
                        newNode.arc.centre = node.Value.arc.centre;
                        newNode.arc.ccw = node.Value.arc.ccw;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddBefore(node, newElement);

                        node = newElement;
                    }

                    if (sPt.IsSameAs(pt1))
                    {
                        prevNode.Value.pt = pt1;
                    }
                    else
                    {
                        /* regular split */

                        Node newNode = new Node();
                        newNode.pt = pt1;
                        newNode.oppNode = null;
                        newNode.arc = new Arc();
                        newNode.arc.startAngle = node.Value.arc.startAngle;
                        newNode.arc.endAngle = angle1;
                        node.Value.arc.startAngle = angle1;
                        newNode.arc.radius = node.Value.arc.radius;
                        newNode.arc.centre = node.Value.arc.centre;
                        newNode.arc.ccw = node.Value.arc.ccw;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddBefore(node, newElement);

                        node = newElement;
                    }

                }





            }
            return node;
        }

    }

    public class Net
    {
        public List<Figure> figures = new List<Figure>();
        public int net;

        public void Renumerate()
        {
            int cnt = 0;
            foreach(Figure figure in figures)
            {
                figure.idx = cnt;
                cnt++;
            }
        }

    }

    public class Drill
    {
        public Point2D pos;
        public double diameter;
    }

    public class DrillList
    {
        public DrillData drillData;
        public List<Point2D> pts;
    }


    public class Graph2D
    {
        public enum ORIENTATION_et
        {
            UNKNOWN,
            CW,
            CCW
        }

        public static bool IsAngleBetween(double a_, double v1_, double v2_, bool ccw)
        {
            double v1, v2;
            if (ccw)
            {
                v1 = v2_;
                v2 = v1_;
            }
            else
            {
                v1 = v1_;
                v2 = v2_;
            }
            double a = a_;

            double v1_deg = 180 * v1 / Math.PI;
            double v2_deg = 180 * v2 / Math.PI;
            double a_deg = 180 * a / Math.PI;

            while (v1 > (v2 + 2 * Math.PI))
            {
                v1 -= 2 * Math.PI;
            }

            while (a > (v2 + 2 * Math.PI))
            {
                a -= 2 * Math.PI;
            }

            while (v1 < v2)
            {
                v1 += 2 * Math.PI;
            }

            while (a < v2)
            {
                a += 2 * Math.PI;
            }

            return (a < v1);

        }

        public static bool IsValueBetween(double val, double v1, double v2)
        {
            if (v1 < v2)
            {
                return (val > v1 && val <= v2);
            }
            else if (v1 > v2)
            {
                return (val >= v2 && val < v1);
            }
            else
            {
                return (val == v1);
            }
        }

        public static bool IsPointOnLine(Point2D pt, Point2D sPt, Point2D ePt)
        {
            if(pt.IsSameAs(ePt))
            {
                return true;
            }

            if (pt.IsSameAs(sPt))
            {
                return false;
            }


            double diffX = Math.Abs(ePt.x - sPt.x);
            double diffY = Math.Abs(ePt.y - sPt.y);

            if(diffX + diffY == 0)
            {
                return false;
            }

            if(diffX > diffY)
            {
                return IsValueBetween(pt.x, sPt.x, ePt.x);
            }
            else
            {
                return IsValueBetween(pt.y, sPt.y, ePt.y);
            }
        }

        public static bool IsPointOnArc(Point2D pt, Point2D sPt, Point2D ePt, Arc arc)
        {
            if (pt.IsSameAs(ePt))
            {
                return true;
            }

            if (pt.IsSameAs(sPt))
            {
                return false;
            }

            double angle = Math.Atan2(pt.y - arc.centre.y, pt.x - arc.centre.x);      
            return Graph2D.IsAngleBetween(angle, arc.startAngle, arc.endAngle,arc.ccw);
        }

        public static ORIENTATION_et CheckTriangleOrientation(Point2D pt1, Point2D pt2, Point2D pt3)
        {
            double xA = pt2.x - pt1.x;
            double yA = pt2.y - pt1.y;
            double xB = pt3.x - pt1.x;
            double yB = pt3.y - pt1.y;

            double i = xA*yB - xB*yA;



            if (i > 0)
            {
                return ORIENTATION_et.CCW;
            }
            else if(i < 0 )
            {
                return ORIENTATION_et.CW;
            }    
            else
            {
                return ORIENTATION_et.UNKNOWN;
            }
        }

        public static Polygon CreateBezier(Polygon pIn)
        {
            LinkedListNode<Node> actNode = pIn.points.First;
            Point2D A = actNode.Value.pt;
            actNode = actNode.Next;
            Point2D B = actNode.Value.pt;
            actNode = actNode.Next;
            Point2D C = actNode.Value.pt;
            actNode = actNode.Next;
            Point2D D = actNode.Value.pt;

            Polygon p = new Polygon();

            double steps = 20;
            double step = 0.05;


            for(int i = 0; i<= steps; i++)
            {
                double t = i*step;

                double tA = Math.Pow(1 - t, 3);
                double tB = Math.Pow(1 - t, 2) * t;
                double tC = Math.Pow(t,2) * (1-t);
                double tD = Math.Pow(t, 3);

                double x = A.x * tA + 3 * B.x * tB + 3 * C.x * tC + D.x * tD;
                double y = A.y * tA + 3 * B.y * tB + 3 * C.y * tC + D.y * tD;

                Node n = new Node();
                n.pt = new Point2D(x, y);

                p.points.AddLast(n);
            }

            return p;



        }
    }
}
