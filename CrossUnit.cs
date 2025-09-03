using System;
using System.Collections.Generic;

namespace KiCad2Gcode
{
    internal class CrossUnit
    {
        public Point2D[] GetCrossingLineLine(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {

            LinkedListNode<Node> n1Prev = node1.Previous ?? node1.List.Last;
            LinkedListNode<Node> n2Prev = node2.Previous ?? node2.List.Last;

            Point2D sP1 = node1.Value.startPt ?? n1Prev.Value.pt;
            Point2D eP1 = node1.Value.pt;
            Point2D sP2 = node2.Value.startPt ?? n2Prev.Value.pt;
            Point2D eP2 = node2.Value.pt;


            double x0, y0, x1, y1;
            double m0, m1, n0, n1;

            x0 = sP1.x;
            y0 = sP1.y;
            m0 = eP1.x - sP1.x;
            n0 = eP1.y - sP1.y;

            x1 = sP2.x;
            y1 = sP2.y;
            m1 = eP2.x - sP2.x;
            n1 = eP2.y - sP2.y;

            double a = m0 * y1 - m0 * y0 - n0 * x1 + n0 * x0;
            double b = n0 * m1 - m0 * n1;

            if (b == 0)
            {
                if (a == 0)
                {
                    /*Parallel, may overlap*/

                    Point2D pt = null;

                    if (Math.Abs(m0) > Math.Abs(n0))
                    {
                        if (Graph2D.IsValueBetween(eP2.x, sP1.x, eP1.x) == true)
                        {
                            pt = eP2;
                        }
                        else if (Graph2D.IsValueBetween(eP1.x, sP2.x, eP2.x) == true)
                        {
                            pt = eP1;
                        }
                    }
                    else
                    {
                        if (Graph2D.IsValueBetween(eP2.y, sP1.y, eP1.y) == true)
                        {
                            pt = eP2;
                        }
                        else if (Graph2D.IsValueBetween(eP1.y, sP2.y, eP2.y) == true)
                        {
                            pt = eP1;
                        }
                    }

                    if (pt != null)
                    {
                        pt.type = Point2D.PointType_et.CROSS_T;
                        Point2D[] ptArr = new Point2D[1];
                        ptArr[0] = pt;
                        return ptArr;
                    }

                    return null;
                }
                else
                {
                    /* parallel not overlap */
                    return null;
                }
            }
            else
            {
                double k = a / b;
                if (k > 0 && k <= 1)
                {
                    double t = 0;
                    if (Math.Abs(m0) > Math.Abs(n0))
                    {
                        t = (x1 + m1 * k - x0) / m0;
                    }
                    else
                    {
                        t = (y1 + n1 * k - y0) / n0;
                    }

                    double x = x0 + m0 * t;
                    double y = y0 + n0 * t;

                    Point2D pt = new Point2D(x, y);

                    if (Graph2D.IsPointOnLine(pt, sP1, eP1) == true && Graph2D.IsPointOnLine(pt, sP2, eP2) == true)
                    {
                        if (pt.IsSameAs(eP1))
                        {
                            pt = eP1;
                            pt.type = Point2D.PointType_et.CROSS_T;
                        }
                        else if (pt.IsSameAs(eP2))
                        {
                            pt = eP2;
                            pt.type = Point2D.PointType_et.CROSS_T;
                        }
                        else
                        {
                            pt.type = Point2D.PointType_et.CROSS_X;
                        }


                        Point2D[] ptArr = new Point2D[1];
                        ptArr[0] = pt;
                        return ptArr;
                    }

                    /*
                    if (t > 0 && t <= 1)
                    {

                        if(pt.IsSameAs(eP1))
                        {
                            pt = eP1;
                            pt.type = Point2D.PointType_et.CROSS_T;
                        }
                        else if(pt.IsSameAs(eP2))
                        {
                            pt = eP2;
                            pt.type = Point2D.PointType_et.CROSS_T;
                        }
                        else
                        {
                            pt.type = Point2D.PointType_et.CROSS_X;
                        }


                        Point2D[] ptArr = new Point2D[1];
                        ptArr[0] = pt;
                        return ptArr;
                    }*/
                }
            }

            return null;


        }

        public Point2D[] GetCrossingLineArc(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {
            LinkedListNode<Node> n1Prev = node1.Previous ?? node1.List.Last;
            LinkedListNode<Node> n2Prev = node2.Previous ?? node2.List.Last;

            Point2D sP1 = node1.Value.startPt ?? n1Prev.Value.pt;
            Point2D eP1 = node1.Value.pt;
            Point2D sP2 = node2.Value.startPt ?? n2Prev.Value.pt;
            Point2D eP2 = node2.Value.pt;

            Arc arc = node2.Value.arc;

            /* calc potential crossing points */
            Vector vL = eP1 - sP1;

            vL.Normalize();

            if (vL.Length == 0)
            {
                return null;
            }

            Vector vC = arc.centre - sP1;

            double a = (vL.x * vC.y - vC.x * vL.y) / vL.Length * vL.Length;

            if (Math.Abs(a) > arc.radius + 0.0000001) { return null; }

            double b;
            if (Math.Abs(vL.x) > Math.Abs(vL.y))
            {
                b = (vC.x + a * vL.y) / vL.x;
            }
            else
            {
                b = (vC.y - a * vL.x) / vL.y;
            }

            double c = Math.Sqrt(Math.Pow(arc.radius, 2) - a * a);

            Point2D ptM = sP1 + b * vL;
            ptM.type = Point2D.PointType_et.CROSS_T;

            Point2D ptM2 = null;

            if (c > 0.0000001)
            {
                ptM2 = ptM + c * vL;
                ptM = ptM - c * vL;
                ptM2.type = Point2D.PointType_et.CROSS_X;
                ptM.type = Point2D.PointType_et.CROSS_X;
            }
            else
            {
                if(ptM.IsSameAs(eP1) == false)
                {
                    ptM = null;
                }
            }

            /* check if points ane on line */

            /*

            if (Graph2D.IsValueBetween(ptM.x, sP1.x, eP1.x) == false)
            {
                ptM = null;
            }
            else if (Graph2D.IsValueBetween(ptM.y, sP1.y, eP1.y) == false)
            {
                ptM = null;
            }*/
            if (ptM != null)
            {
                if (Graph2D.IsPointOnLine(ptM, sP1, eP1) == false)
                {
                    ptM = null;
                }
            }

            if (ptM2 != null)
            {/*
                if (Graph2D.IsValueBetween(ptM2.x, sP1.x, eP1.x) == false)
                {
                    ptM2 = null;
                }
                else if (Graph2D.IsValueBetween(ptM2.y, sP1.y, eP1.y) == false)
                {
                    ptM2 = null;
                }*/

                if (Graph2D.IsPointOnLine(ptM2, sP1, eP1) == false)
                {
                    ptM2 = null;
                }
            }

            /* check if points are on arc */
            if (ptM != null)
            {
                if (Graph2D.IsPointOnArc(ptM, sP2, eP2, arc) == false)
                {
                    ptM = null;
                }

            }

            if (ptM2 != null)
            {
                if (Graph2D.IsPointOnArc(ptM2, sP2, eP2, arc) == false)
                {
                    ptM2 = null;
                }
            }

            int cnt = 0;
            if (ptM != null)
            {
                cnt++;
                if ((ptM.IsSameAs(node1.Value.pt)) || (ptM.IsSameAs(node2.Value.pt)))
                {
                    ptM.type = Point2D.PointType_et.CROSS_T;
                }
            }
            if (ptM2 != null)
            {
                cnt++;
                if ((ptM2.IsSameAs(node1.Value.pt)) || (ptM2.IsSameAs(node2.Value.pt)))
                {
                    ptM2.type = Point2D.PointType_et.CROSS_T;
                }
            }
            Point2D[] ptArr = null;
            if (cnt > 0)
            {
                ptArr = new Point2D[cnt];
            }
            cnt = 0;
            if (ptM != null) { ptArr[cnt] = ptM; cnt++; }
            if (ptM2 != null) { ptArr[cnt] = ptM2; }

            if(ptArr == null)
            {
                cnt = 0; /* trap */

            }

            return ptArr;
        }

        public Point2D[] GetCrossingArcArc(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {
            LinkedListNode<Node> n1Prev = node1.Previous ?? node1.List.Last;
            LinkedListNode<Node> n2Prev = node2.Previous ?? node2.List.Last;

            Point2D sP1 = node1.Value.startPt ?? n1Prev.Value.pt;
            Point2D eP1 = node1.Value.pt;
            Point2D sP2 = node2.Value.startPt ?? n2Prev.Value.pt;
            Point2D eP2 = node2.Value.pt;

            Point2D pc1 = node1.Value.arc.centre;
            Point2D pc2 = node2.Value.arc.centre;

            double r1 = node1.Value.arc.radius;
            double r2 = node2.Value.arc.radius;

            Arc arc1 = node1.Value.arc;
            Arc arc2 = node2.Value.arc;

            Point2D pt1 = null;
            Point2D pt2 = null;

            Vector vcc = pc2 - pc1;

            double c = vcc.Length;



            if (c == 0)
            {
                if (r1 != r2)
                {
                    return null;
                }
                else
                {
                    /* arcs have same centre and radius */
                    if (Graph2D.IsPointOnArc(eP2, sP1, eP1, arc1))
                    {
                        pt1 = eP2;
                        pt1.type = Point2D.PointType_et.CROSS_T;
                    }

                    if (Graph2D.IsPointOnArc(eP1, sP2, eP2, arc2))
                    {
                        if( pt1 == null || pt1.IsSameAs(eP1) == false)
                        {
                            pt2 = eP1;
                            pt2.type = Point2D.PointType_et.CROSS_T;
                        }

                    }



                    

                    /*
                    if (Graph2D.IsAngleBetween(arc2.endAngle, arc1.startAngle, arc1.endAngle, arc1.ccw) == true)
                    {
                        pt1 = node2.Value.pt;
                        pt1.type = Point2D.PointType_et.CROSS_T;
                    }

                    if (Graph2D.IsAngleBetween(arc1.endAngle, arc2.startAngle, arc2.endAngle, arc2.ccw) == true)
                    {
                        pt2 = node1.Value.pt;
                        pt2.type = Point2D.PointType_et.CROSS_T;
                    }*/

                }

            }
            else
            {
                if (c + 0.000001  > r1 + r2 )
                {
                    return null;
                }
                else if (c < Math.Abs(r1 - r2))
                {
                    return null;
                }
                else
                {
                    vcc.Normalize();


                    double a = (c * c + r1 * r1 - r2 * r2) / (2 * c);
                    double h2 = r1 * r1 - a * a;
                    double h = 0;

                    pt1 = pc1 + a * vcc;
                    pt1.type = Point2D.PointType_et.CROSS_T;
                    if (h2 > 0.0000001)
                    {
                        Vector vcc2 = new Vector(vcc.y, -vcc.x);
                        h = Math.Sqrt(h2);
                        pt2 = pt1 + h * vcc2;
                        pt1 = pt1 - h * vcc2;

                        pt1.type = Point2D.PointType_et.CROSS_X;
                        pt2.type = Point2D.PointType_et.CROSS_X;
                    }

                }

                /* check if points are on arc1 */
                if (pt1 != null)
                {
                    if(Graph2D.IsPointOnArc(pt1, sP1, eP1, arc1 ) == false)
                    {
                        pt1 = null;
                    }
                }

                if (pt2 != null)
                {
                    if (Graph2D.IsPointOnArc(pt2, sP1, eP1, arc1) == false)
                    {
                        pt2 = null;
                    }
                }

                /* check if points are on arc2 */
                if (pt1 != null)
                {
                    if (Graph2D.IsPointOnArc(pt1, sP2, eP2, arc2) == false)
                    {
                        pt1 = null;
                    }
                }

                if (pt2 != null)
                {
                    if (Graph2D.IsPointOnArc(pt2, sP2, eP2, arc2) == false)
                    {
                        pt2 = null;
                    }
                }
            }

            

            int cnt = 0;
            if (pt1 != null)
            {
                cnt++;
                if ((pt1.IsSameAs(node1.Value.pt)) || (pt1.IsSameAs(node2.Value.pt)))
                {
                    pt1.type = Point2D.PointType_et.CROSS_T;
                }
            }
            if (pt2 != null)
            {
                cnt++;
                if ((pt2.IsSameAs(node1.Value.pt)) || (pt2.IsSameAs(node2.Value.pt)))
                {
                    pt2.type = Point2D.PointType_et.CROSS_T;
                }
            }
            Point2D[] ptArr = null;
            if (cnt > 0)
            {
                ptArr = new Point2D[cnt];
            }
            cnt = 0;
            if (pt1 != null) { ptArr[cnt] = pt1; cnt++; }
            if (pt2 != null) { ptArr[cnt] = pt2; }

            return ptArr;
        }


        public Point2D[] GetCrosssingPoints(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {
            if (node1.Value.arc == null && node2.Value.arc == null)
            {
                return GetCrossingLineLine(node1, node2);
            }
            else if (node1.Value.arc == null && node2.Value.arc != null)
            {
                return GetCrossingLineArc(node1, node2);
            }
            else if (node1.Value.arc != null && node2.Value.arc == null)
            {
                return GetCrossingLineArc(node2, node1);
            }
            else if (node1.Value.arc != null && node2.Value.arc != null)
            {
                return GetCrossingArcArc(node1, node2);
            }

            return null;
        }


        internal enum CROSS_TYPE_et
        {
            NONE,
            NORMAL,
            DOUBLE,
            END_DN,
            END_UP,
            END2_DN,
            END2_UP,
            EDGE
        };

        enum CR_PT_TYPE_et
        {
            CR_NONE,
            CR_NORMAL,
            CR_UP,
            CR_DN,
        };

        public CROSS_TYPE_et CheckFlatCross(Point2D pt, LinkedListNode<Node> node)
        {
            CROSS_TYPE_et result = CROSS_TYPE_et.NONE;

            LinkedListNode<Node> n1Prev = node.Previous ?? node.List.Last;

            Point2D sP = n1Prev.Value.pt;
            Point2D eP = node.Value.pt;

            if (node.Value.arc == null)
            {
                /* line */

                if (sP.y == eP.y)
                {
                    return CROSS_TYPE_et.NONE;
                }
                else if (sP.y < eP.y)
                {
                    if(sP.y > pt.y || eP.y < pt.y)
                    {
                        return CROSS_TYPE_et.NONE;
                    }
                    else if(sP.y == pt.y)
                    {
                        if(sP.x == pt.x)
                        {
                            return CROSS_TYPE_et.EDGE;
                        }
                        else if(sP.x < pt.x)
                        {
                            return CROSS_TYPE_et.END_UP;
                        }
                        else
                        {
                            return CROSS_TYPE_et.NONE;
                        }
                        
                    }
                    else if (eP.y == pt.y)
                    {
                        if (eP.x == pt.x)
                        {
                            return CROSS_TYPE_et.EDGE;
                        }
                        else if (eP.x < pt.x)
                        {
                            return CROSS_TYPE_et.END_DN;
                        }
                        else
                        {
                            return CROSS_TYPE_et.NONE;
                        }
                    }
                    else
                    {
                        result = CROSS_TYPE_et.NORMAL ;
                    }
                }
                else
                {
                    if (eP.y > pt.y || sP.y < pt.y)
                    {
                        return CROSS_TYPE_et.NONE;
                    }
                    else if (eP.y == pt.y)
                    {
                        if (eP.x == pt.x)
                        {
                            return CROSS_TYPE_et.EDGE;
                        }
                        else if (eP.x < pt.x)
                        {
                            return CROSS_TYPE_et.END_UP;
                        }
                        else
                        {
                            return CROSS_TYPE_et.NONE;
                        }
                    }
                    else if (sP.y == pt.y)
                    {
                        if (eP.x == pt.x)
                        {
                            return CROSS_TYPE_et.EDGE;
                        }
                        else if (eP.x < pt.x)
                        {
                            return CROSS_TYPE_et.END_DN;
                        }
                        else
                        {
                            return CROSS_TYPE_et.NONE;
                        }
                    }
                    else
                    {
                        result = CROSS_TYPE_et.NORMAL;
                    }
                }

                if(result != CROSS_TYPE_et.NORMAL)
                {
                    return CROSS_TYPE_et.NONE;
                }

                if (sP.x > pt.x && eP.x > pt.x) { return CROSS_TYPE_et.NONE; }
                if (sP.x <= pt.x && eP.x <= pt.x) { return CROSS_TYPE_et.NORMAL; }

                double sb =   pt.y - sP.y;
                double eb =  eP.y - pt.y;
                double a = eP.x - sP.x;
                double sa = sb * a / (eb + sb);
                double x = sP.x + sa;

                if (pt.x == x)
                {
                    return CROSS_TYPE_et.EDGE;
                }
                if (pt.x > x)
                {
                    return CROSS_TYPE_et.NORMAL;
                }
                else
                {
                    return CROSS_TYPE_et.NONE;
                }
            }
            else
            {
                /* arc */

                /* fast prechecks */

                Point2D cP = node.Value.arc.centre;
                double r = node.Value.arc.radius;

                if(cP.y + r <= pt.y || cP.y - r >= pt.y)
                {
                    return CROSS_TYPE_et.NONE;
                }

                if(cP.x - r > pt.x)
                {
                    return CROSS_TYPE_et.NONE;
                }

                /* calc arc - line crossing points */

                /* close case */
                double h = pt.y - cP.y;
                double a = Math.Sqrt(r * r - h * h);

                double leftx = cP.x - a;
                double rightx = cP.x + a;

                Point2D leftPt = new Point2D(leftx, pt.y);
                Point2D rightPt = new Point2D(rightx, pt.y);

                CR_PT_TYPE_et leftCr = CR_PT_TYPE_et.CR_NONE;
                CR_PT_TYPE_et rightCr = CR_PT_TYPE_et.CR_NONE;

                if (node.Value.arc.ccw)
                {
                    Point2D tmp = sP;
                    sP = eP;
                    eP = tmp;
                }


                if (leftx < pt.x)
                {
                    if (leftPt.IsSameAs(sP))
                    {
                         leftCr = CR_PT_TYPE_et.CR_UP;
                    }
                    else if (leftPt.IsSameAs(eP))
                    {
                        leftCr = CR_PT_TYPE_et.CR_DN;
                    }
                    else if (Graph2D.IsPointOnArc(leftPt, sP, eP, node.Value.arc))
                    {
                        leftCr = CR_PT_TYPE_et.CR_NORMAL;
                    }
                }


                if (rightx < pt.x)
                {
                    if (leftPt.IsSameAs(sP))
                    {
                        rightCr = CR_PT_TYPE_et.CR_DN;
                    }
                    else if (leftPt.IsSameAs(eP))
                    {
                        rightCr = CR_PT_TYPE_et.CR_UP;
                    }
                    else if ( Graph2D.IsPointOnArc(rightPt, sP, eP, node.Value.arc))
                    {
                        rightCr = CR_PT_TYPE_et.CR_NORMAL;
                    }
                }
                

                if (leftCr == CR_PT_TYPE_et.CR_UP || rightCr == CR_PT_TYPE_et.CR_UP)
                {
                    if (leftCr == CR_PT_TYPE_et.CR_NORMAL || rightCr == CR_PT_TYPE_et.CR_NORMAL)
                    {
                        return CROSS_TYPE_et.END2_UP;
                    }
                    else
                    {
                        return CROSS_TYPE_et.END_UP;
                    }
                }
                else if (leftCr == CR_PT_TYPE_et.CR_DN || rightCr == CR_PT_TYPE_et.CR_DN)
                {
                    if (leftCr == CR_PT_TYPE_et.CR_NORMAL || rightCr == CR_PT_TYPE_et.CR_NORMAL)
                    {
                        return CROSS_TYPE_et.END2_DN;
                    }
                    else
                    {
                        return CROSS_TYPE_et.END_DN;
                    }
                }
                else if (leftCr == CR_PT_TYPE_et.CR_NORMAL && rightCr == CR_PT_TYPE_et.CR_NORMAL)
                {
                    return CROSS_TYPE_et.DOUBLE;
                }
                else if (leftCr == CR_PT_TYPE_et.CR_NORMAL || rightCr == CR_PT_TYPE_et.CR_NORMAL)
                {
                    return CROSS_TYPE_et.NORMAL;
                }
                else
                {
                    return CROSS_TYPE_et.NONE;
                }
            }
        }
    }
}
