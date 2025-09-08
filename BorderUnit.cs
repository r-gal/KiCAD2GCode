using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace KiCad2Gcode
{
    internal class BorderUnit
    {
        private Node SearchNextNode(List<Figure> figures, Node actNode)
        {
            /* retrun null if node has been not found or if is found more than once */

            Node foundNode = null;
            Figure ff = null;

            foreach (Figure f in figures)
            {
                if (actNode.pt.IsSameAs(f.shape.points.First().startPt))
                {
                    if(foundNode != null)
                    {
                        /* another one, discard */
                        return null;
                    }
                    else
                    {
                        foundNode = f.shape.points.First();
                        ff = f;

                    }
                }
                if (actNode.pt.IsSameAs(f.shape.points.First().pt))
                {
                    if (foundNode != null)
                    {
                        /* another one, discard */
                        return null;
                    }
                    else
                    {
                        foundNode = f.shape.points.First();

                        Point2D pt = foundNode.pt;
                        foundNode.pt = foundNode.startPt;
                        foundNode.startPt = pt;
                        if(foundNode.arc != null)
                        {
                            Double angle = foundNode.arc.startAngle;
                            foundNode.arc.startAngle = foundNode.arc.endAngle;
                            foundNode.arc.endAngle = angle;
                            foundNode.arc.ccw = !foundNode.arc.ccw;
                        }
                        
                        ff = f;

                    }
                }
            }

            if(foundNode != null)
            {
                figures.Remove(ff);
            }

            return foundNode;
        }




        public Figure SortNets(List<Figure> figures)
        {
            Figure sorted = new Figure();


            /* get first node */

            while(figures.Count > 0)
            {
                Polygon p = new Polygon();
                Node firstNode = figures[0].shape.points.First();
                p.points.AddLast(firstNode);

                figures.RemoveAt(0);


                Node n = firstNode;

                while (true)
                {

                    if (n.pt.IsSameAs(firstNode.startPt))
                    {
                        /* polygon completed */

                        sorted.holes.Add(p);
                        break;
                    }

                    n = SearchNextNode(figures, n);

                    if(n != null)
                    {
                        p.points.AddLast(n);
                        


                    }
                    else
                    {
                        break;
                    }
                }
            }

            return sorted;
        }
    }
}
