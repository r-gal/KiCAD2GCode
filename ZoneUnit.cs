using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KiCad2Gcode
{
    internal class ZoneUnit
    {
        Form1 mainForm;


        Double MAX_ERR = 0.01;
        
        Double MIN_LEN = 0.05;
        int MIN_POINTS = 15;
        Double MAX_LEN = 1;
        int MIN_POINTS2 = 3;
        Double MAX_LEN2 = 0.3;

        public ZoneUnit(Form1 mainForm_)
        {
            this.mainForm = mainForm_;
        }
        public void ConvertToValidFigure(Figure f)
        {
            int idx1 = 0;
            int idx2 = 0;
            LinkedListNode < Node > n1 = f.shape.points.First;

            while(n1 != null)
            {
                LinkedListNode<Node> n2 = n1.Next;
                idx2 = idx1 + 1;

                while(n2 != null)
                {
                    if(n2.Value.pt.x == n1.Value.pt.x && n2.Value.pt.y == n1.Value.pt.y)
                    {
                        /*mainForm.PrintText("Find Doubled point idx " + idx1.ToString() + "*" + idx2.ToString() +
                            ":  (" + n1.Value.pt.x.ToString() + " " + n1.Value.pt.y.ToString() + ") (" + n2.Value.pt.x.ToString() + " " + n2.Value.pt.y.ToString() + ")\n");
                        */
                        n2.Value.oppNode = n1;
                        n1.Value.oppNode = n2;

                        n1.Value.pt.type = Point2D.PointType_et.DOUBLED;
                        n2.Value.pt.type = Point2D.PointType_et.DOUBLED;
                    }
                    n2 = n2.Next;
                    idx2++;
                }
                n1 = n1.Next;
                idx1++;
            }

            n1 = f.shape.points.First;
            LinkedListNode<Node> nPrev = f.shape.points.Last;
            while (n1 != null)
            {
                if(nPrev.Value.pt.type != Point2D.PointType_et.NORMAL )
                {
                    if (n1.Value.pt.type != Point2D.PointType_et.NORMAL)
                    {
                        if(n1.Value.oppNode.Next.Value.oppNode == nPrev)
                        {
                            n1.Value.pt.type = Point2D.PointType_et.BRIDGE;
                        }


                        
                    }
                }

                nPrev = n1;
                n1 = n1.Next;
            }
            
            Polygon newShape = new Polygon();


            LinkedListNode<Node>  actNode= f.shape.points.First;

            LinkedListNode<Node> firstNode = actNode;

            while (actNode != null)
            {
                Node newNode = new Node();
                newNode.pt = actNode.Value.pt;
                newShape.points.AddLast(newNode);

                if (actNode.Value.pt.type != Point2D.PointType_et.NORMAL)
                {
                    actNode = ProceedHole(actNode.Next, f, 0);
                }

                if (actNode == null)
                {
                    break;
                }
                actNode = actNode.Next;

            }

            f.shape = newShape;

            /* reset point type */

            foreach(Node n in f.shape.points )
            {
                n.pt.type = Point2D.PointType_et.NORMAL;
            }

            foreach(Polygon h in f.holes)
            {
                foreach (Node n in h.points)
                {
                    n.pt.type = Point2D.PointType_et.NORMAL;
                }
            }

            /* check polygon orientation */



            int pointsA, pointsB, pointsC, pointsD;

            
             mainForm.PrintText("No of points D = " + f.shape.points.Count.ToString() + "\n");
             pointsD = f.shape.points.Count;
            SearchUnnecessaryPoints(f.shape);

            mainForm.PrintText("No of points A = " + f.shape.points.Count.ToString() + "\n");
            pointsA = f.shape.points.Count;
            SearchArcs(f.shape,MIN_POINTS,MAX_LEN);
            mainForm.PrintText("No of points B = " + f.shape.points.Count.ToString() + "\n");
            pointsB = f.shape.points.Count;
            SearchArcs(f.shape,MIN_POINTS2, MAX_LEN2);
            mainForm.PrintText("No of points C = " + f.shape.points.Count.ToString() + "\n");
            pointsC = f.shape.points.Count;
           /* SearchUnnecessaryPoints(f.shape);
            mainForm.PrintText("No of points D = " + f.shape.points.Count.ToString() + "\n");
            pointsD = f.shape.points.Count;*/

            foreach (Polygon h in f.holes)
            {
                 
                 mainForm.PrintText("No of points D = " + h.points.Count.ToString() + "\n");
                 pointsD += h.points.Count;
                SearchUnnecessaryPoints(h);

                mainForm.PrintText("No of points A = " + h.points.Count.ToString() + "\n");
                pointsA += h.points.Count;
                SearchArcs(h, MIN_POINTS, MAX_LEN);
                mainForm.PrintText("No of points B= " + h.points.Count.ToString() + "\n");
                pointsB += h.points.Count;
                SearchArcs(h, MIN_POINTS2, MAX_LEN2);
                mainForm.PrintText("No of points C = " + h.points.Count.ToString() + "\n");
                pointsC += h.points.Count;
               /* SearchUnnecessaryPoints(h);
                mainForm.PrintText("No of points D = " + h.points.Count.ToString() + "\n");
                pointsD += h.points.Count;*/
            }
            mainForm.PrintText("Total o of points D = " + pointsD.ToString() + "\n");
            mainForm.PrintText("Total o of points A = " + pointsA.ToString() + "\n");
            mainForm.PrintText("Total o of points B = " + pointsB.ToString() + "\n");
            mainForm.PrintText("Total o of points C = " + pointsC.ToString() + "\n");
            /*mainForm.PrintText("Total o of points D = " + pointsD.ToString() + "\n");*/



            /* try to identify arcs */

        }

        private void SearchUnnecessaryPoints(Polygon p)
        {
            LinkedListNode<Node> actNode = p.points.First;

            while(actNode != null)
            {
                LinkedListNode<Node> remNode = null;
                if (actNode.Previous != null)
                {
                    Vector v = actNode.Value.pt - actNode.Previous.Value.pt;

                    if(v.Length < MIN_LEN)
                    {
                        remNode = actNode;
                        
                    }
                }

                actNode = actNode.Next;

                if(remNode != null)
                {
                    p.points.Remove(remNode);
                }
            }






        }

        struct ArcCheckData
        {
            internal bool result;
            internal LinkedListNode<Node> nextNode;
            internal double maxError;
            internal int foundPoints;

            internal Point2D cP;

            internal LinkedListNode<Node> sN;
            internal LinkedListNode<Node> eN;
            internal LinkedListNode<Node> mN;
        }


        private Point2D GetCentrePoint(Point2D sPt, Point2D mPt, Point2D ePt)
        {
            Point2D cP = null;
            Vector v1 = mPt - sPt;
            Vector v2 = ePt - mPt;

            Point2D pt1 = sPt + 0.5 * v1;
            Point2D pt2 = mPt + 0.5 * v2;

            v1.Normalize();
            v2.Normalize();

            Vector v1p = new Vector(v1.y, -v1.x);
            Vector v2p = new Vector(v2.y, -v2.x);

            double m = v1p.x * v2p.y - v2p.x * v1p.y;

            if (m != 0)
            {
                double cy = pt1.y * v1p.x * v2p.y - pt1.x * v1p.y * v2p.y - pt2.y * v2p.x * v1p.y + pt2.x * v2p.y * v1p.y;

                double cx = pt1.x * v1p.y * v2p.x - pt1.y * v1p.x * v2p.x - pt2.x * v2p.y * v1p.x + pt2.y * v2p.x * v1p.x;

                cx = -cx / m;
                cy = cy / m;

                cP = new Point2D(cx, cy);

                double m1 = pt1.y * v2p.x - pt2.y * v2p.x - pt1.x * v2p.y + pt2.x * v2p.y;
                m1 = m1 / m;

                if(m1 < 0)
                {
                    cP.type = Point2D.PointType_et.CCW;
                }
                else
                {
                    cP.type = Point2D.PointType_et.CW;
                }

            }
            return cP;
        }

        private double GetMaxError(Point2D cP, LinkedListNode<Node> firstNode, int noOfNodes)
        {
            double maxError = 0;
            double r = 0;

            double[] errArray = new double[noOfNodes];

            LinkedListNode<Node> n = firstNode;
            for (int i = 0; i < noOfNodes; i++)
            {
                Vector rV = n.Value.pt - cP;

                if (r == 0)
                {
                    r = rV.Length;
                }
                else
                {
                    double error = r - rV.Length;

                    errArray[i] = error;

                    error = Math.Abs(error);

                    if (error > maxError)
                    {
                        maxError = error;
                    }
                }

                n = n.Next;
                if (n == null)
                {
                    break;
                }
            }

            return maxError;


        }

        private ArcCheckData CheckArc(LinkedListNode<Node> firstNode, int noOfNodes, double maxLen)
        {
            LinkedListNode<Node> mediumNode = firstNode ;
            LinkedListNode<Node> lastNode = firstNode;

            ArcCheckData res;
            res = new ArcCheckData();
            res.result = false;

            

            for(int i=0; i< noOfNodes; i++)
            {
                Point2D pt1 = lastNode.Value.pt;
                lastNode = lastNode.Next;
                               

                
                if (lastNode == null)
                {
                    return res;
                }

                Vector vlen = lastNode.Value.pt - pt1;

                if (vlen.Length > maxLen)
                {
                    return res;
                }

                if(i == noOfNodes/2)
                {
                    mediumNode = lastNode;
                }


            }


            Point2D sPt = firstNode.Value.pt;
            Point2D mPt = mediumNode.Value.pt;
            Point2D ePt = lastNode.Value.pt;

            Point2D cP = GetCentrePoint(sPt, mPt, ePt);

            double accX, accY;
            int accCnt = 0;
            

            if(cP != null)
            {
                accX = cP.x;
                accY = cP.y;
                accCnt = 1;

                Vector rVi = firstNode.Value.pt - cP;

                double maxError = 0;
                double r = rVi.Length;
                double maxLength = 0;

                maxError = GetMaxError(cP, firstNode, noOfNodes);

                Point2D prevPt = null;





                if(maxError < MAX_ERR )
                {

                    res.maxError = maxError;
                    res.result = true;
                    res.nextNode = lastNode;
                    res.foundPoints = noOfNodes;
                    res.cP = cP;

                    res.sN = firstNode;
                    res.eN = lastNode;
                    res.mN = mediumNode;

                    /* found arc, try next points */

                    LinkedListNode<Node> testNode = lastNode;
                    LinkedListNode<Node> testMediumNode = mediumNode;

                    int newPoints = 0;

                    Point2D testPrevPt = lastNode.Value.pt;

                    bool angleLimitReached = false;

                    while (testNode != null)
                    {
                        Vector lVtest = testNode.Value.pt - testPrevPt;
                        testPrevPt = testNode.Value.pt;
                        if (lVtest.Length < maxLen)
                        {
                            newPoints++;
                            if (newPoints == 2)
                            {
                                newPoints = 0;
                                testMediumNode = testMediumNode.Next;
                            }

                            Point2D newCp = GetCentrePoint(sPt, testMediumNode.Value.pt, testNode.Value.pt);

                            if(newCp == null)
                            {
                                break;
                            }

                            double error2 = GetMaxError(newCp, firstNode, res.foundPoints);


                            if (error2 < MAX_ERR)
                            {
                                accX += newCp.x;
                                accY += newCp.y;
                                accCnt++;
                                newCp.x = accX / accCnt;
                                newCp.y = accY / accCnt;

                                res.eN = testNode;
                                res.cP = newCp;
                                res.mN = testMediumNode;
                                res.maxError = error2;

                                Vector vt = testNode.Value.pt - sPt;
                                Vector vc = newCp - sPt;

                                if(angleLimitReached == true)
                                {
                                    if (vt.Length < 1.4* vc.Length )
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (vt.Length > vc.Length * 1.9)
                                    {
                                        angleLimitReached = true;
                                    }
                                }

                            }
                            else
                            {
                                break;
                            }
                            testNode = testNode.Next;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


            }

            if(res.result == true)
            {
                res.cP = GetCentrePoint(res.sN.Value.pt, res.mN.Value.pt, res.eN.Value.pt);
            }
            


            return res;
            


        }


        private void SearchArcs(Polygon p, int minPoints, double maxLen)
        {
            LinkedListNode<Node> actNode = p.points.First;
            int idx = 0;
            while (actNode != null)
            {



                ArcCheckData r = CheckArc(actNode, minPoints, maxLen);

                if(r.result == true)
                {
                    idx += r.foundPoints;
                    /*
                    mainForm.PrintText("Found arc at idx " + idx.ToString() + ", maxError = " + r.maxError.ToString() +  " seqLen = " + r.foundPoints.ToString() + "\n");
                    mainForm.PrintText("Sp = " + r.sN.Value.pt.x.ToString() + " " + r.sN.Value.pt.y.ToString() + "\n");
                    mainForm.PrintText("Cp = " + r.cP.x.ToString() + " " + r.cP.y.ToString() + "\n");
                    mainForm.PrintText("Ep = " + r.eN.Value.pt.x.ToString() + " " + r.eN.Value.pt.y.ToString() + "\n");
                    */
                    Vector sV = r.sN.Value.pt - r.cP;
                    Vector eV = r.eN.Value.pt - r.cP;

                    double sA = Math.Atan2(sV.y, sV.x);
                    double eA = Math.Atan2(eV.y, eV.x);
                    /*
                    mainForm.PrintText("R = " + sV.Length.ToString() + " sA = " + sA.ToString() + " eA = " + eA.ToString() + "\n");
                    */

                    /* replace points by arc.
                     * First point of arc should be not remove, last point of arc should be signed as arc.
                     * All point beetwen start an en wshoudl be deleted*/
                    
                    Arc arc = new Arc();
                    arc.centre = r.cP;
                    arc.radius = sV.Length;
                    arc.startAngle = sA;
                    arc.endAngle = eA;
                    arc.ccw = (r.cP.type == Point2D.PointType_et.CCW);

                    r.eN.Value.arc = arc;

                    LinkedListNode<Node> remNode = r.sN.Next;

                    while(remNode != r.eN)
                    {
                        p.points.Remove(remNode);
                        remNode = r.sN.Next;
                    }
                    

                    actNode = r.eN;


                }
                else
                {
                    actNode = actNode.Next;
                }
                
                idx++;
            }





        }

        private LinkedListNode<Node> ProceedHole( LinkedListNode<Node> firstNode, Figure f, int level)
        {
            LinkedListNode<Node> actNode = firstNode.Next;

            Polygon newShape = new Polygon();

            string indent = "";
            for(int i=0;i< level; i++)
            {
                indent += " ";
            }
               
            mainForm.PrintText(indent + "Start hole at point " + firstNode.Value.pt.x.ToString() + " " + firstNode.Value.pt.y.ToString() + "\n");


            while (actNode != null ) 
            {
                Node newNode = new Node();
                newNode.pt = actNode.Value.pt;
                newShape.points.AddLast(newNode);

                if (actNode.Value.pt.type != Point2D.PointType_et.NORMAL)
                {
                    mainForm.PrintText(indent + "Spec point at " + actNode.Value.pt.x.ToString() + " " + actNode.Value.pt.y.ToString() + "\n");
                    if (actNode.Value.oppNode == firstNode)
                    {
                        /* hole complete, return */
                        f.holes.Add(newShape);

                        mainForm.PrintText(indent + "End hole at point " + actNode.Value.pt.x.ToString() + " " + actNode.Value.pt.y.ToString() + "\n");
                        return actNode.Next;
                    }
                    else
                    {
                        actNode = ProceedHole(actNode.Next, f, level + 1);
                    }  

                }

                if(actNode == null)
                {
                    mainForm.PrintText("Failed hole\n");
                    return null;
                }
                actNode = actNode.Next;



            }
            mainForm.PrintText("Failed hole\n");
            return null;


        }
    }
}
