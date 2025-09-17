using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static KiCad2Gcode.Polygon;

namespace KiCad2Gcode
{
    internal class BorderUnit
    {
        private Node SearchNextNode(List<Node> nodes, Node actNode)
        {
            /* retrun null if node has been not found or if is found more than once */

            Node foundNode = null;
            Node fn = null;

            foreach (Node n in nodes)
            {
                if (actNode.pt.IsSameAs(n.startPt))
                {
                    if(foundNode != null)
                    {
                        /* another one, discard */
                        return null;
                    }
                    else
                    {
                        foundNode = n;
                        fn = n;

                    }
                }
                if (actNode.pt.IsSameAs(n.pt))
                {
                    if (foundNode != null)
                    {
                        /* another one, discard */
                        return null;
                    }
                    else
                    {
                        foundNode = n;

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
                        
                        fn = n;

                    }
                }
            }

            if(foundNode != null)
            {
                nodes.Remove(fn);
            }

            return foundNode;
        }

        public Figure SortNets(List<Node> nodes, List<Polygon> polygons)
        {
            Figure sorted = new Figure();


            /* get first node */

            while(nodes.Count > 0)
            {
                Polygon p = new Polygon();
                Node firstNode = nodes[0];
                p.points.AddLast(firstNode);

                nodes.RemoveAt(0);


                Node n = firstNode;

                while (true)
                {

                    if (n.pt.IsSameAs(firstNode.startPt))
                    {
                        /* polygon completed */

                        sorted.holes.Add(p);
                        break;
                    }

                    n = SearchNextNode(nodes, n);

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

            foreach(Polygon p in polygons)
            {
                sorted.holes.Add(p);
            }

            SearchOuterPolygon(sorted);

            return sorted;
        }

        internal bool SearchOuterPolygon(Figure figure)
        {
            if(figure.holes.Count == 0)
            {
                /*ERROR: outer not found */
                return false;
            }
            else if(figure.holes.Count == 1)
            {
                /* only one polygon, probably outer cut */
                Polygon p = figure.holes[0];
                figure.holes.RemoveAt(0);
                figure.shape = p;
                figure.shape.SetOrientation(Polygon.ORIENTATION_et.CW);
                return true;
            }

            int idx1 = 0;
            int idx2 = 1;



            Polygon outer = null;

            while(idx1 < figure.holes.Count)
            {
                while(idx2 < figure.holes.Count)
                {

                    Polygon p1 = figure.holes[idx1];
                    Polygon p2 = figure.holes[idx2];

                    Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(p1, p2);

                    if (pos == POLYGONS_POS_et.P1_IN_P2)
                    {
                        if(outer != null)
                        {
                            if(outer != p2)
                            {
                                /* ERROR: another outer polygon found */
                                return false;
                            }
                        }
                        else
                        {
                            outer = p2;
                        }
                        
                    }
                    else if(pos == POLYGONS_POS_et.P2_IN_P1)
                    {
                        if (outer != null)
                        {
                            if (outer != p1)
                            {
                                /* ERROR: another outer polygon found */
                                return false;
                            }
                        }
                        else
                        {
                            outer = p1;
                        }
                    }

                    idx2++;
                }

                idx1++;
                idx2 = idx1 + 1;
            }

            if(outer == null)
            {
                /*ERROR: outer not found */
                return false;
            }
            else
            {
                foreach(Polygon p in figure.holes)
                {
                    if(p != outer)
                    {
                        Polygon.POLYGONS_POS_et pos = Polygon.CheckPolygonsPosition(outer, p);

                        if(pos != POLYGONS_POS_et.P2_IN_P1)
                        {
                            /*ERROR: not all holes are located in outer */
                            return false;
                        }

                    }
                }
            }

            figure.shape = outer;

            figure.holes.Remove(outer);

            foreach (Polygon p in figure.holes)
            {
                p.SetOrientation(Polygon.ORIENTATION_et.CCW);
            }
            figure.shape.SetOrientation(Polygon.ORIENTATION_et.CW);

            return true;

        }
    }
}
