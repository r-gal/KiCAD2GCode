using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            CROSS_V
        };
        public PointType_et type;

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
            return (Math.Abs(pt.x - x) < Double.Epsilon && Math.Abs(pt.y - y) < Double.Epsilon);
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

        public Arc()
        {
            type = ChunkType.Arc;
        }

        override public void Rotate(double angle)
        {
            /*start.Rotate(angle);
            end.Rotate(angle);*/
            centre.Rotate(angle);
            startAngle -= angle;
            endAngle -= angle;

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

        public Point2D pt;
        public Arc arc;

        public LinkedListNode<Node> oppNode = null;

        public Node()
        {
            arc = null;
            pt = null;
        }

    }

    public class Figure
    {

        public List<Chunk> chunks = new List<Chunk>();

        public LinkedList<Node> points = new LinkedList<Node>();

        public void Rotate(double angle)
        {
            foreach (Node n in points)
            {
                n.pt.Rotate(angle);
                if(n.arc != null)
                {
                    n.arc.Rotate(angle);
                }
            }
            /*
            foreach (Chunk chunk in chunks)
            {
                chunk.Rotate(angle);
            }*/
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
            /*
            foreach (Chunk chunk in chunks)
            {
                chunk.Move(move);
            }
            */
        }

        public void SplitChunk(LinkedListNode<Node> node, Point2D[] pointArr)
        {
            LinkedListNode < Node > prevNode = node.Previous ?? node.List.Last;

            Point2D sPt = prevNode.Value.pt;
            Point2D ePt = node.Value.pt;

            if (pointArr.Length == 1)
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
                    newNode.pt = node.Value.pt;
                    newNode.oppNode = node.Value.oppNode;
                    node.Value.pt = pointArr[0];
                    if(node.Value.arc != null)
                    {
                        double angle = Math.Atan2(pointArr[0].y - node.Value.arc.centre.y, pointArr[0].x - node.Value.arc.centre.x);
                        newNode.arc = new Arc();
                        newNode.arc.startAngle = angle;
                        newNode.arc.endAngle = node.Value.arc.endAngle;
                        node.Value.arc.endAngle = angle;
                        newNode.arc.radius = node.Value.arc.radius;
                        newNode.arc.centre = node.Value.arc.centre;
                    }
                    LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                    node.List.AddAfter(node, newElement);
                }
            }
            else if (pointArr.Length == 2)
            {
                /* phase 1 - sort merging points */

                Point2D pt1 = null;
                Point2D pt2 = null;

                if (node.Value.arc == null)
                {
                    if( Math.Abs(ePt.x * sPt.x)  > Math.Abs(ePt.x * sPt.x))
                    {
                        if(ePt.x > sPt.x)
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
                        if (ePt.y > sPt.x)
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
                        newNode.pt = node.Value.pt;
                        newNode.oppNode = node.Value.oppNode;
                        node.Value.pt = pt2;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddAfter(node, newElement);
                    }

                    if (sPt.IsSameAs(pt1))
                    {
                        prevNode.Value.pt = pt1;
                    }
                    else
                    {
                        /* regular split */

                        Node newNode = new Node();
                        newNode.pt = node.Value.pt;
                        newNode.oppNode = null;
                        node.Value.pt = pt1;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddAfter(node, newElement);
                    }


                }
                else
                {
                    double angleA = Math.Atan2(pointArr[0].y - node.Value.arc.centre.y, pointArr[0].x - node.Value.arc.centre.x);
                    double angleB = Math.Atan2(pointArr[1].y - node.Value.arc.centre.y, pointArr[1].x - node.Value.arc.centre.x);
                    double angle1 = 0;
                    double angle2 = 0;

                    if (Graph2D.IsAngleBetween(angleA, node.Value.arc.startAngle, angleB) == true)
                    {
                        /* point 1 is first */
                        pt1 = pointArr[1];
                        pt2 = pointArr[0];
                        angle1 = angleA;
                        angle2 = angleB;
                    }
                    else if (Graph2D.IsAngleBetween(angleB, node.Value.arc.startAngle, angleA) == true)
                    {
                        /* point 0 is first */
                        pt1 = pointArr[0];
                        pt2 = pointArr[1];
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
                        newNode.pt = node.Value.pt;
                        newNode.oppNode = node.Value.oppNode;
                        newNode.arc = new Arc();
                        newNode.arc.startAngle = angle2;
                        newNode.arc.endAngle = node.Value.arc.endAngle;
                        node.Value.arc.endAngle = angle2;
                        newNode.arc.radius = node.Value.arc.radius;
                        newNode.arc.centre = node.Value.arc.centre;
                        node.Value.pt = pt2;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddAfter(node, newElement);
                    }

                    if (sPt.IsSameAs(pt1))
                    {
                        prevNode.Value.pt = pt1;
                    }
                    else
                    {
                        /* regular split */

                        Node newNode = new Node();
                        newNode.pt = node.Value.pt;
                        newNode.oppNode = null;
                        newNode.arc = new Arc();
                        newNode.arc.startAngle = angle1;
                        newNode.arc.endAngle = node.Value.arc.endAngle;
                        node.Value.arc.endAngle = angle1;
                        newNode.arc.radius = node.Value.arc.radius;
                        newNode.arc.centre = node.Value.arc.centre;
                        node.Value.pt = pt1;
                        LinkedListNode<Node> newElement = new LinkedListNode<Node>(newNode);
                        node.List.AddAfter(node, newElement);
                    }

                }





            }
        }


    }

    public class Drill
    {
        public Point2D pos;
        public double diameter;
    }


    internal class Graph2D
    {
        public static  bool IsAngleBetween(double a, double v1, double v2)
        {

            while (v1 < v2)
            {
                v1 += 2 * Math.PI;   
            }

            while(a < v2)
            {
               a += 2 * Math.PI; 
            }

            return (a < v1 );

        }
    }
}
