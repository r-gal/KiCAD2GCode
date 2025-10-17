using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Policy;

namespace KiCad2Gcode
{
    internal class CrossUnit
    {
        public List<Point2D> GetCrossingLineLine(LinkedListNode<Node> node1, LinkedListNode<Node> node2, bool allPoints)
        {
            Point2D eP1 = node1.Value.pt;
            Point2D eP2 = node2.Value.pt;

            /* very early checks */

            if (eP1.IsSameAs(eP2))
            {
                eP1.type = Point2D.PointType_et.CROSS_T;
                List<Point2D> ptArr = new List<Point2D> { eP1 };
                return ptArr;
            }


            LinkedListNode<Node> n1Prev = node1.Previous ?? node1.List.Last;
            LinkedListNode<Node> n2Prev = node2.Previous ?? node2.List.Last;
            Point2D sP1 = node1.Value.startPt ?? n1Prev.Value.pt;
            Point2D sP2 = node2.Value.startPt ?? n2Prev.Value.pt;
            /*
            if(sP1.IsSameAs(eP2) || sP2.IsSameAs(eP1))
            {
                return null;
            }*/

            Point2D pt1 = null;
            Point2D pt2 = null;

            Vector v1 = eP1 - sP1;
            Vector v2 = eP2 - sP2;

            if (v1.Length == 0 || v2.Length == 0)
            {
                return null;
            }

            Double length1 = v1.Length;
            Double length2 = v2.Length;

            v1.Normalize();
            v2.Normalize();

            Point2D t = eP1;
            Point2D s = sP2;
            Vector v = v2;

            double a = t.x*v.x - s.x * v.x+  t.y*v.y - s.y * v.y;
            double tmpX = s.x + a * v.x;
            double tmpY = s.y + a * v.y;
            Point2D tmpPt = new Point2D(tmpX, tmpY);

            if (  tmpPt.IsSameAs(eP1) && tmpPt.IsSameAs(sP2) == false)
            {
                if(a >= 0 && a <= length2)
                {
                    pt1 = eP1;
                    pt1.type = Point2D.PointType_et.CROSS_T;
                }

            }

             t = eP2;
             s = sP1;
             v = v1;

             a = t.x * v.x - s.x * v.x + t.y * v.y - s.y * v.y;
             tmpX = s.x + a * v.x;
             tmpY = s.y + a * v.y;
             tmpPt = new Point2D(tmpX, tmpY);

            if ( tmpPt.IsSameAs(eP2) && tmpPt.IsSameAs(sP1) == false)
            {
                if(a >= 0 && a <= length1)
                {
                    pt2 = eP2;
                    pt2.type = Point2D.PointType_et.CROSS_T;
                }

            }

            if (pt1 != null || pt2 != null)
            {
                if (sP1.IsSameAs(eP2) || sP2.IsSameAs(eP1))
                {
                    //return null;
                }
                List<Point2D> ptArr = new List<Point2D>();

                if (pt1 != null)
                {
                    ptArr.Add(pt1);
                }
                if (pt2 != null)
                {
                    ptArr.Add(pt2);
                }
                return ptArr;
            }




            double div = v1.x * v2.y - v1.y * v2.x;

            double m1 = sP1.y * v2.x - sP2.y * v2.x - sP1.x * v2.y + sP2.x * v2.y;
            double m2 = sP2.y * v1.x - sP1.y * v1.x - sP2.x * v1.y + sP1.x * v1.y;

            if(Math.Abs(div)  <  0.0000001)
            {
                if(Math.Abs(m1) < 0.0000001)
                {
                    if (sP1.IsSameAs(eP2) || sP2.IsSameAs(eP1))
                    {
                        return null;
                    }
                    /*Parallel, may overlap*/

                    pt1 = null;
                    pt2 = null;


                    if(Graph2D.IsPointOnLine(eP1,sP2,eP2, allPoints))
                    {
                        pt1 = eP1;
                        pt1.type = Point2D.PointType_et.CROSS_T;
                    }

                    if (Graph2D.IsPointOnLine(eP2, sP1, eP1, allPoints))
                    {
                        pt2 = eP2;
                        pt2.type = Point2D.PointType_et.CROSS_T;
                    }

                    if (pt1 != null && (sP1.IsSameAs(pt1) || sP2.IsSameAs(pt1)))
                    {
                        pt1 = null;
                    }

                    if (pt2 != null && (sP1.IsSameAs(pt2) || sP2.IsSameAs(pt2)))
                    {
                        pt2 = null;
                    }

                    if (pt1 != null || pt2 != null)
                    {
                        List<Point2D> ptArr = new List<Point2D>();

                        if(pt1!= null)
                        {
                            ptArr.Add(pt1);
                        }
                        if (pt2 != null)
                        {
                            ptArr.Add(pt2);
                        }
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

                if (sP1.IsSameAs(eP2) || sP2.IsSameAs(eP1))
                {
                    return null;
                }

                m1 = m1 / div;
                m2 = m2 / -div;

                Point2D cP = sP1 + m1 * v1;

                if(cP.IsSameAs(eP1)  && Math.Abs(m2 - length2) < 0.000001)
                {
                    cP.type = Point2D.PointType_et.CROSS_T;
                    List<Point2D> ptArr = new List<Point2D> { cP };
                    return ptArr;
                }

                if ( cP.IsSameAs(eP2) && Math.Abs(m1 - length1) < 0.000001)
                {
                    cP.type = Point2D.PointType_et.CROSS_T;
                    List<Point2D> ptArr = new List<Point2D> { cP };
                    return ptArr;
                }

                if (cP.IsSameAs(sP1) || cP.IsSameAs(sP2))
                {
                    return null;
                }


                if (m1>= 0 && m1 <= length1 && m2 >= 0 && m2 <= length2)
                {
                    cP.type = Point2D.PointType_et.CROSS_X;
                    List<Point2D> ptArr = new List<Point2D> { cP };
                    return ptArr;
                }

            }


            return null;


        }

        
        public List<Point2D> GetCrossingLineArc(LinkedListNode<Node> node1, LinkedListNode<Node> node2, bool allPoints)
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

            double cLen = arc.radius * arc.radius - a * a;

            if (cLen < 0) 
            { 
                if(cLen > -0.0000001)
                {
                    cLen = 0;
                }
                else
                {
                    return null;
                }
                
            }



            double b;
            if (Math.Abs(vL.x) > Math.Abs(vL.y))
            {
                b = (vC.x + a * vL.y) / vL.x;
            }
            else
            {
                b = (vC.y - a * vL.x) / vL.y;
            }

            double c = Math.Sqrt(cLen);

            Point2D ptM = sP1 + b * vL;

            Point2D ptM2 = ptM + c * vL;
            ptM = ptM - c * vL;
            ptM2.type = Point2D.PointType_et.CROSS_X;
            ptM.type = Point2D.PointType_et.CROSS_X;
            if (ptM.IsSameAs(ptM2))
            {
                ptM2 = null;
                ptM.type = Point2D.PointType_et.CROSS_T;

                if (ptM.IsSameAs(eP1) == false && ptM.IsSameAs(eP2) == false)
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
                if (Graph2D.IsPointOnLine(ptM, sP1, eP1, allPoints) == false)
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

                if (Graph2D.IsPointOnLine(ptM2, sP1, eP1, allPoints) == false)
                {
                    ptM2 = null;
                }
            }

            /* check if points are on arc */
            if (ptM != null)
            {
                if (Graph2D.IsPointOnArc(ptM, sP2, eP2, arc, allPoints) == false)
                {
                    ptM = null;
                }

            }

            if (ptM2 != null)
            {
                if (Graph2D.IsPointOnArc(ptM2, sP2, eP2, arc, allPoints) == false)
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
            List<Point2D> ptArr = null;
            if (cnt > 0)
            {
                 ptArr = new List<Point2D>() ;
            }
            cnt = 0;
            if (ptM != null) { ptArr.Add(ptM);  }
            if (ptM2 != null) { ptArr.Add( ptM2);  }

            if(ptArr == null)
            {
                cnt = 0; /* trap */

            }

            return ptArr;
        }

        public List<Point2D> GetCrossingArcArc(LinkedListNode<Node> node1, LinkedListNode<Node> node2, bool allPoints)
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
                    if (Graph2D.IsPointOnArc(eP2, sP1, eP1, arc1, allPoints))
                    {
                        pt1 = eP2;
                        pt1.type = Point2D.PointType_et.CROSS_T;
                    }

                    if (Graph2D.IsPointOnArc(eP1, sP2, eP2, arc2, allPoints))
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
                if (c - 0.000001  > r1 + r2 )
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
                    if (h2 > 0.000000001)
                    {
                        Vector vcc2 = new Vector(vcc.y, -vcc.x);
                        h = Math.Sqrt(h2);
                        pt2 = pt1 + h * vcc2;
                        pt1 = pt1 - h * vcc2;

                        pt1.type = Point2D.PointType_et.CROSS_X;
                        pt2.type = Point2D.PointType_et.CROSS_X;
                    }
                    else
                    {
                        if(allPoints == false && pt1.IsSameAs(eP1) == false && eP1.IsSameAs(eP2) == false)
                        {
                            pt1 = null;
                        }
                    }

                }

                /* check if points are on arc1 */
                if (pt1 != null)
                {
                    if(Graph2D.IsPointOnArc(pt1, sP1, eP1, arc1 , allPoints) == false)
                    {
                        pt1 = null;
                    }
                }

                if (pt2 != null)
                {
                    if (Graph2D.IsPointOnArc(pt2, sP1, eP1, arc1, allPoints) == false)
                    {
                        pt2 = null;
                    }
                }

                /* check if points are on arc2 */
                if (pt1 != null)
                {
                    if (Graph2D.IsPointOnArc(pt1, sP2, eP2, arc2, allPoints) == false)
                    {
                        pt1 = null;
                    }
                }

                if (pt2 != null)
                {
                    if (Graph2D.IsPointOnArc(pt2, sP2, eP2, arc2, allPoints) == false)
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
            List<Point2D> ptArr = null;
            if (cnt > 0)
            {
                ptArr = new List<Point2D>();
            }
            cnt = 0;
            if (pt1 != null) { ptArr.Add(pt1); }
            if (pt2 != null) { ptArr.Add(pt2); }

            return ptArr;
        }


        public List<Point2D> GetCrosssingPoints(LinkedListNode<Node> node1, LinkedListNode<Node> node2, bool allPoints)
        {
            if (node1.Value.arc == null && node2.Value.arc == null)
            {
                return GetCrossingLineLine(node1, node2, allPoints);
            }
            else if (node1.Value.arc == null && node2.Value.arc != null)
            {
                return GetCrossingLineArc(node1, node2, allPoints);
            }
            else if (node1.Value.arc != null && node2.Value.arc == null)
            {
                return GetCrossingLineArc(node2, node1, allPoints);
            }
            else if (node1.Value.arc != null && node2.Value.arc != null)
            {
                return GetCrossingArcArc(node1, node2, allPoints);
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

                double h = pt.y - cP.y;

                if( Math.Abs(h) > r + 0.00000001)
                {
                    return CROSS_TYPE_et.NONE;
                }

                double l2 = r * r - h * h;


                

                if(cP.x - r > pt.x)
                {
                    return CROSS_TYPE_et.NONE;
                }

                /* calc arc - line crossing points */

                /* close case */

                double a = 0;

                CR_PT_TYPE_et leftCr = CR_PT_TYPE_et.CR_NONE;
                CR_PT_TYPE_et rightCr = CR_PT_TYPE_et.CR_NONE;

                if (l2 > 0)
                {
                    a = Math.Sqrt(l2);
                }

                    
                double leftx = cP.x - a;
                double rightx = cP.x + a;

                Point2D leftPt = new Point2D(leftx, pt.y);
                Point2D rightPt = new Point2D(rightx, pt.y);

                if(leftx > pt.x)
                {
                    return CROSS_TYPE_et.NONE;
                }

                int caseS = sP.y > pt.y ? 2 : (sP.y == pt.y ? 1 : 0);
                int caseE = eP.y > pt.y ? 2 : (eP.y == pt.y ? 1 : 0);

                int caseCom = caseS + 3 * caseE;

                switch( caseCom)
                {
                    case 0:  /* ys <y, ye < y */
                    case 8:  /* ys >y, ye > y */
                        if (leftx > pt.x ^ rightx > pt.x)
                        {
                            Point2D ptM = new Point2D(cP.x, pt.y);
                            double angleS = Vector.GetAlpha(ptM, sP);
                            double angleE = Vector.GetAlpha(ptM, eP);



                            if (node.Value.arc.ccw == false)
                            {
                                if (angleS < angleE)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                                else if (angleS == angleE && node.Value.arc.startAngle != node.Value.arc.endAngle)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                            }
                            else
                            {
                                if (angleS > angleE)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                                else if (angleS == angleE && node.Value.arc.startAngle != node.Value.arc.endAngle)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                            }
                        }
                        return CROSS_TYPE_et.NONE; /* may be also double but effect is this same */
                        
                            

                    case 2:  /* ys >y, ye < y */
                        if(node.Value.arc.ccw == true)
                        {
                            if (leftx < pt.x)
                            {
                                return CROSS_TYPE_et.NORMAL;
                            }
                        }
                        else
                        {
                            if (rightx < pt.x)
                            {
                                return CROSS_TYPE_et.NORMAL;
                            }
                        }
                        return CROSS_TYPE_et.NONE;

                    case 6:  /* ys <y, ye > y */
                        if (node.Value.arc.ccw == true)
                        {
                            if (rightx < pt.x)
                            {
                                return CROSS_TYPE_et.NORMAL;
                            }
                        }
                        else
                        {
                            if (leftx < pt.x)
                            {
                                return CROSS_TYPE_et.NORMAL;
                            }
                        }
                        return CROSS_TYPE_et.NONE;

                    case 4:  /* ys =y, ye = y */
                        if(rightx >= pt.x && leftx < pt.x)
                        {
                            if(sP.x < pt.x ^ node.Value.arc.ccw)
                            {
                                return CROSS_TYPE_et.END_UP;
                            }
                            else
                            {
                                return CROSS_TYPE_et.END_DN;
                            }
                        }
                        return CROSS_TYPE_et.NONE;

                    case 1:  /* ys =y, ye < y */
                        if(rightx < pt.x)
                        {
                            return CROSS_TYPE_et.END_DN;
                        }
                        if (leftx < pt.x)
                        {
                            if (sP.x < pt.x)
                            {
                                if (node.Value.arc.ccw == true)
                                {
                                    return CROSS_TYPE_et.END_DN;
                                }
                                else
                                {
                                    return CROSS_TYPE_et.END_UP;
                                }
                            }
                            else
                            {
                                if (node.Value.arc.ccw == true)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                            }
                        } 
                        return CROSS_TYPE_et.NONE;
                    case 3:  /* ys <y, ye = y */
                        if (rightx < pt.x)
                        {
                            return CROSS_TYPE_et.END_DN;
                        }
                        if (leftx < pt.x)
                        {
                            if (eP.x < pt.x)
                            {
                                if (node.Value.arc.ccw == true)
                                {
                                    return CROSS_TYPE_et.END_UP;
                                }
                                else
                                {
                                    return CROSS_TYPE_et.END_DN;
                                }
                            }
                            else
                            {
                                if (node.Value.arc.ccw == false)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                            }
                        }
                        return CROSS_TYPE_et.NONE;
                    case 5:  /* ys >y, ye = y */
                        if (rightx < pt.x)
                        {
                            return CROSS_TYPE_et.END_UP;
                        }
                        if (leftx < pt.x)
                        {
                            if (eP.x < pt.x)
                            {
                                if (node.Value.arc.ccw == true)
                                {
                                    return CROSS_TYPE_et.END_UP;
                                }
                                else
                                {
                                    return CROSS_TYPE_et.END_DN;
                                }
                            }
                            else
                            {
                                if (node.Value.arc.ccw == true)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                            }
                        }
                        return CROSS_TYPE_et.NONE;
                    case 7:  /* ys =y, ye > y */
                        if (rightx < pt.x)
                        {
                            return CROSS_TYPE_et.END_UP;
                        }
                        if (leftx < pt.x)
                        {
                            if (sP.x < pt.x)
                            {
                                if (node.Value.arc.ccw == true)
                                {
                                    return CROSS_TYPE_et.END_DN;
                                }
                                else
                                {
                                    return CROSS_TYPE_et.END_UP;
                                }
                            }
                            else
                            {
                                if (node.Value.arc.ccw == false)
                                {
                                    return CROSS_TYPE_et.NORMAL;
                                }
                            }
                        }
                        return CROSS_TYPE_et.NONE;
                    default:
                        return CROSS_TYPE_et.NONE;
                }



                

            }
        }

        public CROSS_TYPE_et CheckFlatCrossOld(Point2D pt, LinkedListNode<Node> node)
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
                    if (sP.y > pt.y || eP.y < pt.y)
                    {
                        return CROSS_TYPE_et.NONE;
                    }
                    else if (sP.y == pt.y)
                    {
                        if (sP.x == pt.x)
                        {
                            return CROSS_TYPE_et.EDGE;
                        }
                        else if (sP.x < pt.x)
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
                        result = CROSS_TYPE_et.NORMAL;
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

                if (result != CROSS_TYPE_et.NORMAL)
                {
                    return CROSS_TYPE_et.NONE;
                }

                if (sP.x > pt.x && eP.x > pt.x) { return CROSS_TYPE_et.NONE; }
                if (sP.x <= pt.x && eP.x <= pt.x) { return CROSS_TYPE_et.NORMAL; }

                double sb = pt.y - sP.y;
                double eb = eP.y - pt.y;
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

                double h = pt.y - cP.y;

                double l2 = r * r - h * h;

                if (l2 < -0.0000001)
                {
                    return CROSS_TYPE_et.NONE;
                }


                if (cP.x - r > pt.x)
                {
                    return CROSS_TYPE_et.NONE;
                }

                /* calc arc - line crossing points */

                /* close case */

                double a = 0;
                if (l2 > 0)
                {
                    a = Math.Sqrt(l2);
                }
                else if (cP.x < pt.x)
                {
                    Point2D comPt = new Point2D(cP.x, pt.y);

                    if (comPt.IsSameAs(sP) ^ comPt.IsSameAs(eP))
                    {
                        if (cP.y < pt.y)
                        {
                            return CROSS_TYPE_et.END_DN;
                        }
                        else
                        {
                            return CROSS_TYPE_et.END_UP;
                        }

                    }
                    else
                    {
                        return CROSS_TYPE_et.NONE;
                    }
                }
                else
                {
                    return CROSS_TYPE_et.NONE;
                }

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
                    else if (Graph2D.IsPointOnArc(leftPt, sP, eP, node.Value.arc, false))
                    {
                        leftCr = CR_PT_TYPE_et.CR_NORMAL;
                    }
                }


                if (rightx < pt.x)
                {
                    if (rightPt.IsSameAs(sP))
                    {
                        rightCr = CR_PT_TYPE_et.CR_DN;
                    }
                    else if (rightPt.IsSameAs(eP))
                    {
                        rightCr = CR_PT_TYPE_et.CR_UP;
                    }
                    else if (Graph2D.IsPointOnArc(rightPt, sP, eP, node.Value.arc, false))
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
