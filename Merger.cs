using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static KiCad2Gcode.CrossUnit;

namespace KiCad2Gcode
{


    internal class Merger
    {
        Form1 mainForm;

        public Merger(Form1 mainForm)
        {
            this.mainForm = mainForm;
        }





        private bool CheckPointInFigure(Point2D pt, Figure f)
        {
            int crosses = 0;

            int state = 0; /* -1: DN, 0 : IDLE, 1 : UP */


            LinkedListNode<Node> actNode = f.points.First;

            CrossUnit crossUnit = new CrossUnit();

            do
            {

                CROSS_TYPE_et result = crossUnit.CheckFlatCross(pt, actNode);

                switch(result)
                {
                    case CROSS_TYPE_et.NORMAL:
                        state = 0;
                        crosses++;
                        break;
                    case CROSS_TYPE_et.DOUBLE:
                        state = 0;
                        crosses+=2;
                        break;
                    case CROSS_TYPE_et.END_DN:
                        if(state == 1)
                        {
                            state = 0;
                            crosses++;
                        }
                        else if(state == -1)
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
                            crosses+=2;
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

                    case CROSS_TYPE_et.END2_UP:
                        if (state == -1)
                        {
                            state = 0;
                            crosses += 2;
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
                }



                actNode = actNode.Next;
            } while (actNode != null);


            if (state == 0)
            {
                if (crosses % 2 == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {

                /* probably point is on edge */
                return false;
            }
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
                    CrossUnit crossUnit = new CrossUnit();
                    Point2D[] points = crossUnit.GetCrosssingPoints(n1, n2);

                    if (points != null)
                    {
                        mainForm.PrintText("Find Point 1 at " + points[0].x.ToString() + "," + points[0].y.ToString() + " type is " + points[0].type.ToString() + "\n");
                        if(points.Length == 2)
                        {
                            mainForm.PrintText("Find Point 2 at " + points[1].x.ToString() + "," + points[1].y.ToString() + " type is " + points[1].type.ToString() + "\n");
                        }


                        /* cut f1 */

                        f1.SplitChunk(n1, points);
                        

                        /* cut f2 */

                        f2.SplitChunk(n2, points);


                        if(points.Length == 1)
                        {
                            /* easy case */
                            n1.Value.oppNode = n2.Next ?? n2.List.First;
                            n2.Value.oppNode = n1.Next ?? n1.List.First;

                            mainForm.PrintText("Set oppNode for f1: " + n1.Value.oppNode.Value.pt.x.ToString() + "," + n1.Value.oppNode.Value.pt.y.ToString() + "\n");
                            mainForm.PrintText("Set oppNode for f2: " + n2.Value.oppNode.Value.pt.x.ToString() + "," + n2.Value.oppNode.Value.pt.y.ToString() + "\n");

                        }
                        else
                        {
                            if(n1.Value.pt == n2.Value.pt)
                            {
                                n1.Value.oppNode = n2.Next ;
                                n2.Value.oppNode = n1.Next ;

                                n1.Next.Value.oppNode = n2.Next.Next ?? n2.List.First;
                                n2.Next.Value.oppNode = n1.Next.Next ?? n1.List.First;
                            }
                            else
                            {
                                n1.Value.oppNode = n2.Next.Next ?? n2.List.First; 
                                n2.Value.oppNode = n1.Next.Next ?? n1.List.First; 

                                n1.Next.Value.oppNode = n2.Next;
                                n2.Next.Value.oppNode = n1.Next;

                            }

                            mainForm.PrintText("Set oppNode for f1: " + n1.Value.oppNode.Value.pt.x.ToString() + "," + n1.Value.oppNode.Value.pt.y.ToString() + "\n");
                            mainForm.PrintText("Set oppNode for f2: " + n2.Value.oppNode.Value.pt.x.ToString() + "," + n2.Value.oppNode.Value.pt.y.ToString() + "\n");

                            mainForm.PrintText("Set oppNode 2 for f1: " + n1.Next.Value.oppNode.Value.pt.x.ToString() + "," + n1.Next.Value.oppNode.Value.pt.y.ToString() + "\n");
                            mainForm.PrintText("Set oppNode 2 for f2: " + n2.Next.Value.oppNode.Value.pt.x.ToString() + "," + n2.Next.Value.oppNode.Value.pt.y.ToString() + "\n");


                        }


                    }
                    n2 = n2.Next;
                }
                n1 = n1.Next;
            }





            Figure newFigure = new Figure();

            LinkedListNode<Node> actNode = f1.points.First;
            int activeFigure = 0;
            LinkedListNode<Node> firstNode = actNode;

            Point2D testPoint = new Point2D(12, 8);
            bool pointInFigure = CheckPointInFigure(testPoint, f1);

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

                    double out1Angle = 0;
                    double out2Angle = 0;

                    double r1 = 0;
                    double r2 = 0;


                    if (nodeA.Value.arc == null)
                    {
                        out1Angle = Vector.GetAlpha(actPoint, nextPoint1);
                        r1 = Double.MaxValue;
                    }
                    else
                    {
                        Vector v = nodeA.Value.arc.centre - actPoint;
                        Vector vt = new Vector(-v.y, v.x);
                        out1Angle = Vector.GetAlpha(vt);
                        r1 = nodeA.Value.arc.radius;
                    }

                    if (nodeB.Value.arc == null)
                    {
                        out2Angle = Vector.GetAlpha(actPoint, nextPoint2);
                        
                        r2 = Double.MaxValue;
                    }
                    else
                    {
                        Vector v = nodeB.Value.arc.centre - actPoint;
                        Vector vt = new Vector(- v.y, v.x);
                        out2Angle = Vector.GetAlpha(vt);
                        r1 = nodeB.Value.arc.radius;
                    }



                    mainForm.PrintText("Test in " + actPoint.x.ToString() + "," + actPoint.y.ToString() + "\n");
                    mainForm.PrintText("NODE1 " + nextPoint1.x.ToString() + "," + nextPoint1.y.ToString() + "\n");
                    mainForm.PrintText("NODE2 " + nextPoint2.x.ToString() + "," + nextPoint2.y.ToString() + "\n");

                    mainForm.PrintText("Alpha : IN=" + inputAngle.ToString() + " OUT1=" + out1Angle.ToString() + " OUT2=" + out2Angle.ToString() + "\n");

                    prevPoint = actNode.Value.pt;

                    if(out1Angle == out2Angle)
                    {
                        if(r1>r2)
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
                    else if (inputAngle > out1Angle)
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
