using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KiCad2Gcode.CrossUnit;

namespace KiCad2Gcode
{
    public class Polygon : Graph2D
    {
        public LinkedList<Node> points = new LinkedList<Node>();

        public List<Figure> innerFigures = null; /* used in field milling unit to hierarhical sort */

        int L = 0, U = 1, R = 2, D = 3;

        public Point2D[] extPoint = new Point2D[4];
        public Node[] extNode = new Node[4];

        public int idx;

        public int selected = 0;

        public ORIENTATION_et orientation = ORIENTATION_et.NOT_CHECKED;

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
            if (orientation != ORIENTATION_et.NOT_CHECKED)
            {
                /* alrady checked */
            }
            else if (points.Count == 0)
            {
                orientation = ORIENTATION_et.UNKNOWN;
            }
            else if (points.Count == 1)
            {
                if (points.First.Value.arc != null)
                {
                    if (points.First.Value.arc.ccw)
                    {
                        orientation = ORIENTATION_et.CCW;
                    }
                    else
                    {
                        orientation = ORIENTATION_et.CW;
                    }
                }
                else
                {
                    orientation = ORIENTATION_et.UNKNOWN;
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
                if (w1 + w2 < 0)
                {
                    orientation = ORIENTATION_et.CCW;
                }
                else if (w1 + w2 > 0)
                {
                    orientation = ORIENTATION_et.CW;
                }
                else
                {
                    orientation = ORIENTATION_et.UNKNOWN;
                }
            }
            else
            {
                /*find point located  most down - right */

                LinkedListNode<Node> selNode = points.First;
                Point2D selPoint = selNode.Value.pt;

                LinkedListNode<Node> actNode = points.First;

                while (actNode != null)
                {
                    Point2D lowestPoint = actNode.Value.pt;

                    if (actNode.Value.arc != null)
                    {
                        if (Graph2D.IsAngleBetween(Math.PI * -0.5, actNode.Value.arc.startAngle, actNode.Value.arc.endAngle, actNode.Value.arc.ccw))
                        {
                            lowestPoint = new Point2D(actNode.Value.arc.centre);
                            lowestPoint.y -= actNode.Value.arc.radius;
                        }
                    }

                    if (lowestPoint.y < selPoint.y)
                    {
                        selNode = actNode;
                        selPoint = lowestPoint;
                    }
                    else if (lowestPoint.y == selPoint.y)
                    {
                        if (lowestPoint.x > selPoint.x)
                        {
                            selNode = actNode;
                            selPoint = lowestPoint;
                        }
                    }
                    actNode = actNode.Next;
                }

                if (selPoint.IsSameAs(selNode.Value.pt))
                {
                    LinkedListNode<Node> prevNode = selNode.Previous ?? selNode.List.Last;
                    LinkedListNode<Node> nextNode = selNode.Next ?? selNode.List.First;
                    AngleData inAngle = new AngleData(selNode.Value, prevNode, true);
                    AngleData outAngle = new AngleData(selNode.Value, nextNode, false);

                    if (inAngle.angle > 3) { inAngle.angle = 0; }
                    if (outAngle.angle > 3) { outAngle.angle = 0; }

                    if (Math.Abs(inAngle.angle - outAngle.angle) < 0.00001)
                    {
                        if (inAngle.wgt > outAngle.wgt)
                        {
                            orientation = ORIENTATION_et.CW;
                        }
                        else
                        {
                            orientation = ORIENTATION_et.CCW;
                        }
                    }
                    else if (inAngle.angle < outAngle.angle)
                    {
                        orientation = ORIENTATION_et.CW;
                    }
                    else
                    {
                        orientation = ORIENTATION_et.CCW;
                    }
                }
                else
                {
                    if (selNode.Value.arc.ccw)
                    {
                        orientation = ORIENTATION_et.CCW;
                    }
                    else
                    {
                        orientation = ORIENTATION_et.CW;
                    }
                }


            }



            return orientation;
        }

        public ORIENTATION_et CheckOrientationOld()
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
                if (w1 + w2 < 0)
                {
                    return ORIENTATION_et.CCW;
                }
                else if (w1 + w2 > 0)
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

                while (actNode != null)
                {
                    if (actNode.Value.pt.y < selNode.Value.pt.y)
                    {
                        selNode = actNode;
                    }
                    else if (actNode.Value.pt.y == selNode.Value.pt.y)
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

        private bool ComparePoints(Point2D pt1, Point2D pt2)
        {
            if (Math.Abs(pt1.x - pt2.x) > 0.001)
            {
                return false;
            }

            if (Math.Abs(pt1.y - pt2.y) > 0.001)
            {
                return false;
            }
            return true;
        }

        public bool CheckConsistency()
        {
            if (CheckOrientation() == ORIENTATION_et.UNKNOWN)
            {
                return false;
            }



            Point2D prevPoint = points.Last.Value.pt;

            foreach (Node n in points)
            {
                if (n.arc != null)
                {
                    Point2D sPt = new Point2D(n.arc.centre.x + n.arc.radius * Math.Cos(n.arc.startAngle), n.arc.centre.y + n.arc.radius * Math.Sin(n.arc.startAngle));
                    Point2D ePt = new Point2D(n.arc.centre.x + n.arc.radius * Math.Cos(n.arc.endAngle), n.arc.centre.y + n.arc.radius * Math.Sin(n.arc.endAngle));

                    if (ComparePoints(sPt, prevPoint) == false)
                    {
                        return false;
                    }

                    if (ComparePoints(ePt, n.pt) == false)
                    {
                        return false;
                    }

                }
                prevPoint = n.pt;

            }
            return true;

        }

        public void SetOrientation(ORIENTATION_et orientation_)
        {
            ORIENTATION_et actOrientation = CheckOrientation();
            if (actOrientation == orientation_)
            {
                return;
            }
            else if (actOrientation != ORIENTATION_et.UNKNOWN && orientation_ != ORIENTATION_et.UNKNOWN)
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

                while (points.Count > 0)
                {
                    Node nn = points.Last();
                    points.RemoveLast();
                    newList.AddLast(nn);


                };

                points = newList;
                orientation = orientation_;
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

                    List<Point2D> crossPoint = crossUnit.GetCrosssingPoints(node1, node2, false);

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

            if (crossesFound)
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

        internal void FilterShortSegments()
        {
            if(points == null || points.Count < 3)
            {
                return;
            }

            bool cont = false;

            do
            {
                cont = false;
                LinkedListNode<Node> actNode = points.First;
                Point2D startPt = points.Last.Value.pt;

                while (actNode != null && points.Count > 2)
                {
                    Vector v = actNode.Value.pt - startPt;

                    if (v.Length < 0.01) /* really small segment */
                    {
                        LinkedListNode<Node> remNode = actNode;

                        actNode = remNode.Next ?? points.First;

                        if(actNode.Value.arc != null)
                        {
                            Vector vOld =   actNode.Value.pt - remNode.Value.pt;
                            double oldLength = vOld.Length;
                            vOld.Normalize();

                            Vector vStartOld = actNode.Value.arc.centre - remNode.Value.pt;

                            double a = vStartOld.y * vOld.y + vStartOld.x * vOld.x;
                            double h = vStartOld.y * vOld.x - vStartOld.x * vOld.y;

                            Point2D sP = startPt;
                            Point2D eP = actNode.Value.pt;

                            Vector vNew = eP - sP;

                            double ratio = vNew.Length / oldLength;

                            double aNew = ratio * a;
                            double hNew = ratio * h;

                            vNew.Normalize();

                            Vector vNewOrt = new Vector(-vNew.y, vNew.x);

                            Vector vNewCp = a * vNew + h * vNewOrt;
                            Point2D newCentre = sP + vNewCp;


                            actNode.Value.arc.radius = vNewCp.Length; 
                            actNode.Value.arc.centre = newCentre;
                            actNode.Value.arc.startAngle = Math.Atan2(sP.y - newCentre.y, sP.x - newCentre.x);
                            actNode.Value.arc.endAngle = Math.Atan2(eP.y - newCentre.y, eP.x - newCentre.x );


                        }

                        points.Remove(remNode);

                        cont = true;
                    }

                    startPt = actNode.Value.pt;
                    actNode = actNode.Next;
                }
            }
            while (cont);
        }
    }
}
