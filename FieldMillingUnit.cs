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

        internal void CreateFields(Figure board, List<Net> zones, Net[] netList, double diameter)
        {
            increasedFigures.Clear();



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
            }

            foreach (Net net in zones)
            {
                foreach (Figure figure in net.figures)
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

            foreach(Figure f in increasedFigures)
            {
                unit.millFieldsPath.Add(f.shape);
                foreach(Polygon p in f.holes)
                {
                    unit.millFieldsPath.Add(p);
                }
            }

            /* phase  - merge all Figures*/

        }
    }
}
