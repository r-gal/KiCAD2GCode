using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KiCad2Gcode
{


    internal class Merger
    {
        Form1 mainForm;

        public Merger(Form1 mainForm)
        {
            this.mainForm = mainForm;
        }

        bool IsValueBeetween(double val, double v1, double v2)
        {
            if (v1 < v2)
            {
                return (val > v1 && val <= v2);
            }
            else
            {
                return (val >= v2 && val < v1);
            }
        }


        bool IsAngleBetween(double a, double v1, double v2)
        {
            while (a > Math.PI) { a -= 2 * Math.PI; }
            while (a < -Math.PI) { a += 2 * Math.PI; }

            while (v1 > Math.PI) { v1 -= 2 * Math.PI; }
            while (v1 < -Math.PI) { v1 += 2 * Math.PI; }

            while (v2 > Math.PI) { v2 -= 2 * Math.PI; }
            while (v2 < -Math.PI) { v2 += 2 * Math.PI; }

            if (v1 > v2)
            {
                return (a <= v1 && a >= v2);
            }
            else
            {
                return (a >= v1 && a <= v2);
            }
        }

        public Point2D[] GetCrossingLineLine(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {

            LinkedListNode<Node> n1Prev = node1.Previous ?? node1.List.Last;
            LinkedListNode<Node> n2Prev = node2.Previous ?? node2.List.Last;

            Point2D sP1 = n1Prev.Value.pt;
            Point2D eP1 = node1.Value.pt;
            Point2D sP2 = n2Prev.Value.pt;
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

            if(b == 0)
            {
                if(a == 0)
                {
                    /*Parallel, may overlap*/

                    Point2D pt = null;

                    if(Math.Abs(m0) > Math.Abs(n0))
                    {
                        if (IsValueBeetween(eP2.x, sP1.x, eP1.x) == true)
                        {
                            pt = eP2;
                        }
                        else if (IsValueBeetween(eP1.x, sP2.x, eP2.x) == true)
                        {
                            pt = eP1;
                        }
                    }
                    else
                    {
                        if (IsValueBeetween(eP2.y, sP1.y, eP1.y) == true)
                        {
                            pt = eP2;
                        }
                        else if (IsValueBeetween(eP1.y, sP2.y, eP2.y) == true)
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
                if(k > 0 && k <= 1)
                {
                    double t = 0;
                    if(Math.Abs(m0) > Math.Abs(n0) )
                    {
                        t = (x1 + m1 * k - x0) / m0;
                    }
                    else
                    {
                        t = (y1 + n1 * k - y0) / n0;
                    }
                    
                    if(t>0 &&  t<=1)
                    {
                        double x = x0 + m0 * t;
                        double y = y0 + n0 * t;

                        Point2D pt = new Point2D(x, y);

                        if(t<1 && k < 1)
                        {
                            pt.type = Point2D.PointType_et.CROSS_X;
                        }
                        else
                        {
                            pt.type = Point2D.PointType_et.CROSS_T;
                        }
                        

                        Point2D[] ptArr = new Point2D[1];
                        ptArr[0] = pt;
                        return ptArr;
                    }                    
                }
            }

            return null; 


        }

        public Point2D[] GetCrossingLineArc(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {
            LinkedListNode<Node> n1Prev = node1.Previous ?? node1.List.Last;
            //LinkedListNode<Node> n2Prev = node1.Previous ?? node2.List.Last;

            Point2D sP1 = n1Prev.Value.pt;
            Point2D eP1 = node1.Value.pt;
            //Point2D sP2 = n2Prev.Value.pt;
            //Point2D eP2 = node2.Value.pt;

            Arc arc = node2.Value.arc;

            /* calc potential crossing points */
            Vector vL = eP1 - sP1;

            vL.Normalize();

            if(vL.Length == 0 )
            {
                return null;
            }

            Vector vC = arc.centre - sP1;

            double a = (vL.x * vC.y - vC.x * vL.y) / vL.Length * vL.Length;

            if(Math.Abs(a) > arc.radius) { return null; }

            double b;
            if(Math.Abs(vL.x) >  Math.Abs(vL.y))
            {
                b = (vC.x + a * vL.y) / vL.x;
            }
            else
            {
                b = (vC.y - a * vL.x) / vL.y;
            }

            Point2D ptM = sP1 + b * vL;
            Point2D ptM2 = null;

            if (b != 0)
            {
                ptM2 = ptM + b * vL;
                ptM = ptM - b * vL;
            }

            /* check if points ane on line */

            if(IsValueBeetween(ptM.x, sP1.x, eP1.x) == false)
            {
                ptM = null;
            }
            else if (IsValueBeetween(ptM.y, sP1.y, eP1.y) == false)
            {
                ptM = null;
            }

            if(ptM2 != null)
            {
                if (IsValueBeetween(ptM2.x, sP1.x, eP1.x) == false)
                {
                    ptM2 = null;
                }
                else if (IsValueBeetween(ptM2.y, sP1.y, eP1.y) == false)
                {
                    ptM2 = null;
                }
            }

            /* check if points are on arc */
            if (ptM != null)
            {
                Vector vTmp = ptM - arc.centre;
                double angle = Math.Atan2(vTmp.y, vTmp.x);
                if(IsAngleBetween (angle , arc.startAngle, arc.endAngle) == false)
                {
                    ptM = null;
                }                
            }

            if (ptM2 != null)
            {
                Vector vTmp = ptM2 - arc.centre;
                double angle = Math.Atan2(vTmp.y, vTmp.x);
                if (IsAngleBetween(angle, arc.startAngle, arc.endAngle) == false)
                {
                    ptM2 = null;
                }
            }

            int cnt = 0;
            if(ptM != null) {  cnt ++; }
            if (ptM2 != null) { cnt++; }
            Point2D[] ptArr = null;
            if (cnt > 0)
            {
                ptArr = new Point2D[cnt];
            }
            cnt = 0;
            if (ptM != null) { ptArr[cnt] = ptM; cnt++; }
            if (ptM2 != null) { ptArr[cnt] = ptM2; }

            return ptArr;
        }

        public Point2D[] GetCrossingArcArc(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {
            return null;
        }
        

        public Point2D[] GetCrosssingPoints(LinkedListNode<Node> node1, LinkedListNode<Node> node2)
        {
            if(node1.Value.arc == null && node2.Value.arc == null)
            {
                return GetCrossingLineLine(node1 , node2);
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






        public Figure Merge(Figure f1, Figure f2)
        {

            /* find crossing points */

            LinkedListNode<Node> n1 = f1.points.First;

            while (n1 != null)
            {                
                LinkedListNode<Node> n2 = f2.points.First;

                while (n2 != null)
                {                   

                    Point2D[] points = GetCrosssingPoints(n1, n2);

                    if (points != null)
                    {
                        mainForm.PrintText("Find Point at " + points[0].x.ToString() + "," + points[0].y.ToString() + " type is " + points[0].type.ToString() + "\n");



                        /* cut f1 */

                        f1.SplitChunk(n1, points);
                        

                        /* cut f2 */

                        f2.SplitChunk(n2, points);


                        n1.Value.oppNode = n2.Next ?? n2.List.First ;
                        n2.Value.oppNode = n1.Next ?? n1.List.First;

                        mainForm.PrintText("Set oppNode for f1: " + n1.Value.oppNode.Value.pt.x.ToString() + "," + n1.Value.oppNode.Value.pt.y.ToString() + "\n");
                        mainForm.PrintText("Set oppNode for f2: " + n2.Value.oppNode.Value.pt.x.ToString() + "," + n2.Value.oppNode.Value.pt.y.ToString() + "\n");
                    }
                    n2 = n2.Next;
                }
                n1 = n1.Next;
            }





            Figure newFigure = new Figure();

            LinkedListNode<Node> actNode = f1.points.First;
            int activeFigure = 0;
            LinkedListNode<Node> firstNode = actNode;

            Point2D prevPoint = f1.points.Last.Value.pt;

            do
            {
                Node n = actNode.Value;
                LinkedListNode<Node> copiedNode = new LinkedListNode<Node>(n);



                newFigure.points.AddLast(copiedNode);

                if (actNode.Value.pt.type == Point2D.PointType_et.NORMAL)
                {
                    prevPoint = actNode.Value.pt;
                    actNode = actNode.Next ?? actNode.List.First;
                }
                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_X)
                {

                    prevPoint = actNode.Value.pt;
                    actNode = actNode.Value.oppNode;

                }
                else if (actNode.Value.pt.type == Point2D.PointType_et.CROSS_T)
                {
                    LinkedListNode<Node> nodeA = actNode.Next ?? actNode.List.First;
                    LinkedListNode<Node> nodeB = actNode.Value.oppNode;

                    /* choose correct node */

                    /* angle is for compare only so alfa function is enough */

                    Point2D actPoint = actNode.Value.pt;
                    
                    Point2D nextPoint1 = nodeA.Value.pt;
                    Point2D nextPoint2 = nodeB.Value.pt;

                    double inputAngle = Vector.GetAlpha(actPoint, prevPoint);

                    double out1Angle = Vector.GetAlpha(actPoint, nextPoint1);
                    double out2Angle = Vector.GetAlpha(actPoint, nextPoint2);


                    mainForm.PrintText("Test in " + actPoint.x.ToString() + "," + actPoint.y.ToString() + "\n");
                    mainForm.PrintText("NODE1 " + nextPoint1.x.ToString() + "," + nextPoint1.y.ToString() + "\n");
                    mainForm.PrintText("NODE2 " + nextPoint2.x.ToString() + "," + nextPoint2.y.ToString() + "\n");

                    mainForm.PrintText("Alpha : IN=" + inputAngle.ToString() + " OUT1=" + out1Angle.ToString() + " OUT2=" + out2Angle.ToString() + "\n");

                    prevPoint = actNode.Value.pt;


                    if (inputAngle > out1Angle)
                    {
                        if(inputAngle > out2Angle)
                        {
                            /* prev is 3 */
                            if(out1Angle > out2Angle)
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
                    else if(inputAngle > out2Angle)
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

                    mainForm.PrintText("Go to  " + actNode.Value.pt.x.ToString() + "," + actNode.Value.pt.y.ToString() + "\n");

                }
            }
            while (actNode != firstNode);


            return newFigure;
        }
    }
}
