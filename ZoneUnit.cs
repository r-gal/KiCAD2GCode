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
                        mainForm.PrintText("Find Doubled point idx " + idx1.ToString() + "*" + idx2.ToString() +
                            ":  (" + n1.Value.pt.x.ToString() + " " + n1.Value.pt.y.ToString() + ") (" + n2.Value.pt.x.ToString() + " " + n2.Value.pt.y.ToString() + ")\n");

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


            SearchArcs(f.shape);

            foreach (Polygon h in f.holes)
            {
                SearchArcs(h);
            }



            /* try to identify arcs */

        }


        private void SearchArcs(Polygon p)
        {
            double r = 0;
            double seqLength = 0;

            Vector pV = null;

            if(p.points.Count<2)
            {
                return;
            }

            LinkedListNode<Node> actNode = p.points.First;
            LinkedListNode<Node> prevNode = p.points.Last;

            while(actNode != null)
            {
                Point2D sPt = prevNode.Value.pt;
                Point2D ePt = actNode.Value.pt;

                Vector v = ePt - sPt;

                if(pV != null)
                {
                    double actAngle = Vector.AngleBetween(v, pV);

                    //mainForm.PrintText("L = " + v.Length.ToString() + "A=" + actAngle.ToString() + "\n");

                    double actR = 0;
                    bool cont = true;
                    if(actAngle != 0)
                    {
                        actR = v.Length / actAngle;

                        if(Math.Abs(r - actR) < 0.5)
                        {
                            seqLength++;

                        }
                        else
                        {
                            cont = false;
                        }



                    }
                    else
                    {
                        cont = false;
                    }

                    if(cont == false)
                    {
                        if (seqLength > 1)
                        {
                            mainForm.PrintText("Seq found, length = " + seqLength.ToString() + "\n");
                        }
                        seqLength = 0;
                        r = actR ;


                    }






                }

                pV = v;
                prevNode = actNode; 
                actNode = actNode.Next;
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
