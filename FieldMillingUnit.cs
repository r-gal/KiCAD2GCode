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

        private List<Polygon> IncreasePolygon(Polygon polygon, double diameter, bool mainShape)
        {
            PatchUnit path = new PatchUnit(unit);
            List<Polygon> list =  path.CreatePatch(polygon,diameter, mainShape);


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

        private int AddToMillList(List<Figure> increasedFigures)
        {
            int millsCnt = 0;
            foreach (Figure f in increasedFigures)
            {
                if ( f.containBoard == false)
                {
                    unit.millFieldsPath.Add(f.shape);
                    millsCnt++;
                }
                else
                {
                    millsCnt++;
                }

                foreach (Polygon p in f.holes)
                {
                    unit.millFieldsPath.Add(p);
                    millsCnt++;
                }

            }
            unit.RedrawAll();
            return millsCnt;
        }

        private int AddToMillListTest(List<Figure> increasedFigures)
        {
            unit.ClearNetList();
            unit.ClearAll();
            int millsCnt = 0;
            foreach (Figure f in increasedFigures)
            {
                f.net = 0;

                if (f.containBoard == false)
                {
                    unit.AddFigure(f);
                }
                else
                {
                    f.holes.Clear();
                    unit.board = f;
                }

                


                
                
                millsCnt++;

            }
            unit.RedrawAll();
            return millsCnt;
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



        internal void CreateFields(Figure board, Net[] netList, double firstOffset, double step)
        {
            increasedFigures.Clear();
            invertedFigures.Clear();



            /*phase 1 - increase all shapes and holes*/
            foreach (Net net in netList)
            {
                foreach(Figure figure in net.figures) 
                { 
                    Figure newFigure = new Figure();

                    List<Polygon> newPolygons = IncreasePolygon(figure.shape, firstOffset, true);

                    if (newPolygons == null || newPolygons.Count < 1) { return; }

                    int cwCnt = 0;

                    foreach(Polygon p in newPolygons)
                    {
                        if(p.orientation == Graph2D.ORIENTATION_et.CCW)
                        {
                            newFigure.holes.Add(p);
                        }
                        else if(cwCnt == 0)
                        {
                            cwCnt++;
                            newFigure.shape = p;
                        }
                        else
                        {
                            MainUnit.PrintText("Error, found more CW shapes\n ");
                        }
                    }

                    foreach (Polygon h in figure.holes)
                    {
                        newPolygons = IncreasePolygon(h, firstOffset, false);
                        foreach(Polygon p in newPolygons)
                        {
                            newFigure.holes.Add(p);
                        }
                    }
                    if (newFigure.shape.points.Count == 0)
                    {
                        MainUnit.PrintText("Error\n");
                        newFigure.holes[0].CheckOrientation();
                    }
                    increasedFigures.Add(newFigure);
                }

                foreach (Figure figure in net.zoneFigures)
                {
                    Figure newFigure = new Figure();

                    List<Polygon> newPolygons = IncreasePolygon(figure.shape, firstOffset, true);
                    if (newPolygons == null ||newPolygons.Count < 1) { return; }

                    int cwCnt = 0;
                    foreach (Polygon p in newPolygons)
                    {
                        if (p.orientation == Graph2D.ORIENTATION_et.CCW)
                        {
                            newFigure.holes.Add(p);
                        }
                        else if (cwCnt == 0)
                        {
                            cwCnt++;
                            newFigure.shape = p;
                            if (newFigure.shape.points.Count == 0)
                            {
                                MainUnit.PrintText("Error\n");
                            }
                        }
                        else
                        {
                            MainUnit.PrintText("Error, found more CW shapes\n ");
                        }
                    }


                    foreach (Polygon h in figure.holes)
                    {
                        newPolygons = IncreasePolygon(h, firstOffset, false);
                        foreach (Polygon p in newPolygons)
                        {
                            newFigure.holes.Add(p);
                        }
                    }
                    if (newFigure.shape.points.Count == 0)
                    {
                        MainUnit.PrintText("Error\n");
                        newFigure.holes[0].CheckOrientation();
                    }
                    increasedFigures.Add(newFigure);
                }
            }

            /* create top figure */

            Figure topFigure = new Figure();
            List<Polygon> polygons = IncreasePolygon(board.shape, 2* firstOffset, true);
            topFigure.shape = polygons[0];
            topFigure.holes.Add(board.shape);
            topFigure.net = 0;
            topFigure.name = "board";
            topFigure.containBoard = true;

            topFigure.shape.SetOrientation(Graph2D.ORIENTATION_et.CW);
            topFigure.holes[0].SetOrientation(Graph2D.ORIENTATION_et.CCW);

            if (topFigure.shape.points.Count == 0)
            {
                MainUnit.PrintText("Error\n");
            }

            increasedFigures.Add(topFigure);

            foreach(Polygon p in board.holes)
            {
                Figure holeFigure = new Figure();
                holeFigure.net = 0;
                holeFigure.name = "hole";
                holeFigure.shape = p;
                holeFigure.shape.SetOrientation(Graph2D.ORIENTATION_et.CW);
                if (holeFigure.shape.points.Count == 0)
                {
                    MainUnit.PrintText("Error\n");
                }
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

            //AddToMillList(increasedFigures);

            Merger merger = new Merger(unit);
            merger.Init(increasedFigures, increasedFigures);

            do
            {
                res = merger.Step(0);
                //AddToMillList(increasedFigures);
            } while (res == true);

            AddToMillList(increasedFigures);


            //return;

            /* next tries */

            int addedMills = 0;
            int cnt = 0;
            do
            {
                //MainUnit.PrintText("Start try " + cnt.ToString() + "\n ");
                List<Figure> tmpList = new List<Figure>();
                addedMills = 0;
                /*phase 1 - increase all shapes and holes*/


                foreach (Figure figure in increasedFigures)
                {
                    Figure newFigure = new Figure();
                    newFigure.net = figure.net;
                    newFigure.name = figure.name;
                    newFigure.containBoard = figure.containBoard;

                    List<Polygon> newPolygons = IncreasePolygon(figure.shape, step, true);
                    if (newPolygons.Count < 1) { return; }

                    int cwCnt = 0;
                    foreach (Polygon p in newPolygons)
                    {
                        if (p.orientation == Graph2D.ORIENTATION_et.CCW)
                        {
                            newFigure.holes.Add(p);
                        }
                        else if (cwCnt == 0)
                        {
                            cwCnt++;
                            newFigure.shape = p;
                        }
                        else
                        {
                            MainUnit.PrintText("Error, found more CW shapes\n ");
                        }
                    }

                    foreach (Polygon h in figure.holes)
                    {
                        newPolygons = IncreasePolygon(h, step, false);
                        if(newPolygons.Count > 1)
                        {
                            MainUnit.PrintText("More holes\n ");
                        }
                        foreach (Polygon p in newPolygons)
                        {
                            newFigure.holes.Add(p);
                        }
                    }


                    newFigure.shape.GetExtPoints();

                    if (newFigure.shape.points.Count == 0)
                    {
                        MainUnit.PrintText("Error\n");
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

                addedMills = AddToMillList(increasedFigures);

                cnt++;
                //addedMills = 0;
                //MainUnit.PrintText("Added " + addedMills.ToString() + " mills\n ");

            } while (addedMills > 0 && cnt < 50);





            

        }

       
    }
}
