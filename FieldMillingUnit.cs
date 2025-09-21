using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCad2Gcode
{
    internal class FieldMillingUnit
    {
        List<Figure> increasedFigures = new List<Figure>();

        List<Figure> invertedFigures = new List<Figure>();

        MainUnit unit;

        internal FieldMillingUnit( MainUnit unit)
        {
            this.unit = unit;
        }

        private List<Polygon> IncreasePolygon(Polygon polygon, double diameter)
        {
            PatchUnit path = new PatchUnit(unit);
            return path.CreatePatch(polygon,diameter);
        }

        internal void CreateFields(Figure board, Net[] netList, double diameter)
        {
            increasedFigures.Clear();
            invertedFigures.Clear();



            /*phase 1 - increase all shapes and holes*/
            foreach (Net net in netList)
            {
                foreach(Figure figure in net.figures) 
                { 
                    Figure newFigure = new Figure();

                    List<Polygon> newPolygons = IncreasePolygon(figure.shape, diameter);
                    newFigure.shape = newPolygons[0];

                    foreach (Polygon h in figure.holes)
                    {
                        newPolygons = IncreasePolygon(h, diameter);
                        foreach(Polygon p in newPolygons)
                        {
                            newFigure.holes.Add(p);
                        }
                    }
                    increasedFigures.Add(newFigure);
                }

                foreach (Figure figure in net.zoneFigures)
                {
                    Figure newFigure = new Figure();

                    List<Polygon> newPolygons = IncreasePolygon(figure.shape, diameter);
                    newFigure.shape = newPolygons[0];

                    foreach (Polygon h in figure.holes)
                    {
                        newPolygons = IncreasePolygon(h, diameter);
                        foreach (Polygon p in newPolygons)
                        {
                            newFigure.holes.Add(p);
                        }
                    }
                    increasedFigures.Add(newFigure);
                }
            }

            /* phase  - merge all Figures*/
            foreach (Figure f in increasedFigures)
            {
                f.shape.GetExtPoints();
                foreach(Node n in f.shape.points)
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
            }

            bool res = false;

            Merger merger = new Merger(unit);
            merger.Init(increasedFigures, increasedFigures);

            do
            {
                res = merger.Step(0);
            } while (res == true);


            /* phase - invert and sort  */

            Figure topFigure = new Figure();
            topFigure.shape = board.shape;

            invertedFigures.Add(topFigure);




            /* print */

            foreach (Figure f in increasedFigures)
            {
                unit.millFieldsPath.Add(f.shape);
                foreach (Polygon p in f.holes)
                {
                    unit.millFieldsPath.Add(p);
                }
            }

        }
    }
}
