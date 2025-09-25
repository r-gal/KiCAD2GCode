using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KiCad2Gcode.Polygon;

namespace KiCad2Gcode
{
    internal class FieldMillingUnit
    {
        List<Figure> increasedFigures = new List<Figure>();

        List<Figure> invertedFigures = new List<Figure>();

        List<Figure> millFigures = new List<Figure>();

        MainUnit unit;

        internal FieldMillingUnit( MainUnit unit)
        {
            this.unit = unit;
        }

        private List<Polygon> IncreasePolygon(Polygon polygon, double diameter)
        {
            PatchUnit path = new PatchUnit(unit);
            List<Polygon> list =  path.CreatePatch(polygon,diameter);
            /*
            if(list.Count > 0)
            {
                Graph2D.ORIENTATION_et ornt = polygon.CheckOrientation();

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].CheckOrientation() != ornt)
                    {
                        list.RemoveAt(i);
                    }
                }
            }*/


            return list;
        }

        private void MoveFigureToHole(Figure f, List<Polygon> holes) 
        {
            foreach(Polygon h in holes)
            {
                POLYGONS_POS_et result = Polygon.CheckPolygonsPosition(f.shape,h);

                if(result == POLYGONS_POS_et.P1_IN_P2)
                {
                    h.innerFigures.Add(f);
                    return;
                }
            }

        }

        private void AddToMillList(List<Figure> increasedFigures)
        {

            foreach (Figure f in increasedFigures)
            {
                if (f.name == null || f.name.Contains("board") == false)
                {
                    unit.millFieldsPath.Add(f.shape);
                }

                foreach (Polygon p in f.holes)
                {
                    unit.millFieldsPath.Add(p);
                }

            }
            unit.RedrawAll();
        }

        private void AddToMillListTest(List<Figure> increasedFigures)
        {
            unit.ClearNetList();
            foreach (Figure f in increasedFigures)
            {
                f.net = 0;
                unit.AddFigure(f);

            }
            unit.RedrawAll();
        }

        private bool SortList(List<Figure> list)
        {

            foreach (Figure f in list)
            {
                f.touched = false;
            }

            int actIdx = 0;

            bool moved = false;

            actIdx = 0;
            


            while (actIdx < list.Count)
            {

                Figure actFigure = list[actIdx];

                int scanIdx = actIdx + 1;

                while (scanIdx < list.Count && actFigure.touched == false)
                {
                    Figure scanFigure = list[scanIdx];

                    if(scanFigure.touched == false)
                    {
                        POLYGONS_POS_et result = Polygon.CheckPolygonsPosition(actFigure.shape, scanFigure.shape);

                        if(result == POLYGONS_POS_et.P1_IN_P2)
                        {
                            /* act in scan */
                            actFigure.touched = true;
                            MoveFigureToHole(actFigure, scanFigure.holes);
                            moved = true;
                        }
                        else if (result == POLYGONS_POS_et.P2_IN_P1)
                        {
                            /* scan in act */
                            scanFigure.touched = true;
                            MoveFigureToHole(scanFigure, actFigure.holes);
                            moved = true;
                        }

                    }


                    scanIdx++;
                }
                actIdx++;
            }

            actIdx = 0;

            if(moved)
            {
                while (actIdx < list.Count)
                {
                    Figure actFigure = list[actIdx];
                    if (actFigure.touched == true)
                    {
                        list.Remove(actFigure);
                    }
                    else
                    {
                        actIdx++;
                    }

                }
            }

            foreach (Figure f in list)
            {
                foreach(Polygon h in f.holes)
                {
                    if (h.innerFigures.Count > 1)
                    {
                        SortList(h.innerFigures);
                    }
                }
            }

            return moved;
        }

        private void CreateMillFigures(Figure figure)
        {
            foreach(Polygon h in figure.holes)
            {
                Figure newFigure = new Figure();

                newFigure.shape = h;
                foreach(Figure innerFig in h.innerFigures)
                {
                    newFigure.holes.Add(innerFig.shape);
                    CreateMillFigures(innerFig);
                }
                millFigures.Add(newFigure); 
            }

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

            /* create top figure */

            Figure topFigure = new Figure();
            List<Polygon> polygons = IncreasePolygon(board.shape, 2*diameter);
            topFigure.shape = polygons[0];
            topFigure.holes.Add(board.shape);
            topFigure.net = 0;
            topFigure.name = "board";

            topFigure.shape.SetOrientation(Graph2D.ORIENTATION_et.CW);
            topFigure.holes[0].SetOrientation(Graph2D.ORIENTATION_et.CCW);

            increasedFigures.Add(topFigure);

            foreach(Polygon p in board.holes)
            {
                Figure holeFigure = new Figure();
                holeFigure.net = 0;
                holeFigure.name = "hole";
                holeFigure.shape = p;
                holeFigure.shape.SetOrientation(Graph2D.ORIENTATION_et.CW);
                increasedFigures.Add(holeFigure);
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

            AddToMillList(increasedFigures);


            return;

            /* next tries */

            int addedMills = 0;
            do
            {
                List<Figure> tmpList = new List<Figure>();
                addedMills = 0;
                /*phase 1 - increase all shapes and holes*/


                foreach (Figure figure in increasedFigures)
                {
                    Figure newFigure = new Figure();
                    newFigure.net = figure.net;
                    newFigure.name = figure.name;

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
                    tmpList.Add(newFigure);
                }

                increasedFigures = tmpList;

                /* phase  - merge all Figures*/
                foreach (Figure f in increasedFigures)
                {
                     f.shape.GetExtPoints();
                     foreach (Node n in f.shape.points)
                     {
                         n.pt.type = Point2D.PointType_et.NORMAL;
                     }
                     foreach (Polygon h in f.holes)
                     {
                         foreach (Node n in h.points)
                         {
                             n.pt.type = Point2D.PointType_et.NORMAL;
                         }
                     }
                 }

                 res = false;

                 merger.Init(increasedFigures, increasedFigures);

                 do
                 {
                     res = merger.Step(0);


                } while (res == true);

                AddToMillList(increasedFigures);


                addedMills = 0;

            } while (addedMills > 0);





            

        }

        internal void CreateFieldsOld(Figure board, Net[] netList, double diameter)
        {
            increasedFigures.Clear();
            invertedFigures.Clear();



            /*phase 1 - increase all shapes and holes*/
            foreach (Net net in netList)
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
                foreach (Node n in f.shape.points)
                {
                    n.pt.type = Point2D.PointType_et.NORMAL;
                }
                foreach (Polygon h in f.holes)
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

            /* phase - invert holes  */

            foreach (Figure f in increasedFigures)
            {
                foreach (Polygon p in f.holes)
                {
                    p.SetOrientation(Graph2D.ORIENTATION_et.CW);
                    p.innerFigures = new List<Figure>();
                }
                f.touched = false;
            }

            /* phase - sort  */

            SortList(increasedFigures);

            Figure topFigure = new Figure();
            topFigure.shape = board.shape;

            foreach (Figure innerFig in increasedFigures)
            {
                topFigure.holes.Add(innerFig.shape);
                CreateMillFigures(innerFig);
            }
            millFigures.Add(topFigure);

            foreach (Figure f in millFigures)
            {
                f.shape.SetOrientation(Graph2D.ORIENTATION_et.CCW);
            }

            /* fill */
            /*
                        foreach (Figure f in millFigures)
                        {

                            Figure newFig = new Figure();

                            newFig.shape = Mill
                        }*/

            /* print */

            foreach (Figure f in millFigures)
            {
                unit.millFieldsPath.Add(f.shape);
                unit.RedrawAll();
                foreach (Polygon p in f.holes)
                {
                    unit.millFieldsPath.Add(p);
                    unit.RedrawAll();
                }

            }

        }
    }
}
