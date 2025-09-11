using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace KiCad2Gcode
{
    internal class MainUnit
    {
        public enum STATE_et
        {
            IDLE,
            FILE_LOADED,
            PATHES_MERGED,
            ZONES_JOINED,
            ZONES_MERGED,
            TRACE_MILLING_GENERATED,
            BOARD_MILLING_GENERATED,
            HOLES_PREPARED,
            GCODE_GENERATED
        }

        public STATE_et state = STATE_et.IDLE;

        static Form1 mainForm;

        PcbFileParser pcbFileParser;
        internal Drawer drawer;
        Merger merger;

        List<Net> zones = new List<Net>();
        List<Node> cuts = new List<Node>();
        List<Drill> drills = new List<Drill>();
        Figure board;

        List<Polygon> millPath = new List<Polygon>();
        List<Polygon> boardMillPath = new List<Polygon>();

        Net[] netList;

        int idxA = 0;
        int idxB = 1;
        int idxNet = 0;

        public MainUnit(Form1 mainForm_, Drawer drawer_) 
        {
            MainUnit.mainForm = mainForm_;
            drawer = drawer_;

            pcbFileParser = new PcbFileParser(this);
            merger = new Merger(this);

            SetState(STATE_et.IDLE);
        }

        public static void PrintText(string text)
        {
            mainForm.PrintText(text);
        }

        public void SetState(STATE_et newState)
        {
            state = newState;
        }

        internal void SetScale(int newScale)
        {
            drawer.SetScale(newScale);
            RedrawAll();
        }

        internal bool LoadFile(string filePath)
        {
            netList = null;
            cuts.Clear();
            drills.Clear();
            zones.Clear();

            millPath.Clear();
            boardMillPath.Clear();



            bool result = pcbFileParser.Parse(filePath);

            if (result)
            { 
                foreach (Net n in netList)
                {
                    foreach (Figure f in n.figures)
                    {
                        f.shape.GetExtPoints();
                    }
                }

                foreach (Net n in zones)
                {
                    foreach (Figure f in n.figures)
                    {
                        f.shape.GetExtPoints();
                    }
                }

                BorderUnit borderUnit = new BorderUnit();
                board = borderUnit.SortNets(cuts);

                board.shape.GetExtPoints();
                foreach (Polygon h in board.holes)
                {
                    h.GetExtPoints();
                }

                RedrawAll();

                SetState(STATE_et.FILE_LOADED);

                List<Double[]> drillsStatistics = new List<Double[]>();
                foreach(Drill drill in drills)
                {
                    double diameter = drill.diameter;
                    bool found = false;
                    foreach (Double[] arr in drillsStatistics)
                    {
                        if(arr[0] == diameter)
                        {
                            arr[1]++;
                            found = true;
                        }
                    }
                    if(found ==false)
                    {
                        Double[] arr = new double[2];
                        arr[0] = diameter;
                        arr[1] = 1;
                        drillsStatistics.Add(arr);
                    }
                }

                foreach (Double[] arr in drillsStatistics)
                {
                    PrintText("Found " + arr[1].ToString() + " holes with diameter " + arr[0].ToString().ToString() + "\n");
                }


                PrintText("File loaded \n");

                idxA = 0;
                idxB = 1;
            }
            return result;
        }

        internal void Run(int idx)
        {
            switch(idx)
            {
                case 0:/*ALL*/
                    MergePolygons();
                    MergePolygonsToZones();
                    MergeZones();
                    ProceedTracesMilling();
                    ProceedBoardMilling();
                    ProceedHoles();
                    GenerateGCode();
                    break;
                case 1:/*MERGE POLYGONS*/
                    MergePolygons();
                    break;
                case 2: /*JOIN ZONES*/
                    MergePolygonsToZones();
                    break;
                case 3:/*MERGE ZONES*/
                    MergeZones();
                    break;
                case 4:/*TRACE PATH MILLING*/
                    ProceedTracesMilling();
                    break;
                case 5:/*TRACE BOARD MILLING*/
                    ProceedBoardMilling();
                    break;
                case 6: /*PROCEED HOLES*/
                    ProceedHoles();
                    break;
                case 7: /*GENERATE G - CODE*/
                    GenerateGCode();
                    break;

            }


        }

        internal void TestStep()
        {
            Step(1);
            RedrawAll();
    }

        private void MergePolygons()
        {

            if(state != STATE_et.FILE_LOADED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run merge polygons\n");
            }


            bool res = false;

            idxA = 0;
            idxB = 0;
            idxNet = 0;

            do
            {
                res = Step(0);
            } while (res == true);
            RedrawAll();

            idxA = 0;
            idxB = 0;
            idxNet = 0;

            SetState(STATE_et.PATHES_MERGED);
            PrintText("Done\n");
        }

        private void MergePolygonsToZones()
        {
            if (state != STATE_et.PATHES_MERGED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run join zones\n");
            }

            bool res = false;

            idxA = 0;
            idxB = 0;
            idxNet = 0;

            do
            {
                res = Step(1);
            } while (res == true);
            RedrawAll();

            idxA = 0;
            idxB = 0;
            idxNet = 0;

            SetState(STATE_et.ZONES_JOINED);
            PrintText("Done\n");
        }

        private void MergeZones()
        {
            if (state != STATE_et.ZONES_JOINED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run merge zones\n");
            }
            bool res = false;

            idxA = 0;
            idxB = 0;
            idxNet = 0;

            do
            {
                res = Step(2);
            } while (res == true);
            RedrawAll();

            idxA = 0;
            idxB = 0;
            idxNet = 0;
            SetState(STATE_et.ZONES_MERGED);
            PrintText("Done\n");
        }

        private void ProceedTracesMilling()
        {
            if (state != STATE_et.ZONES_MERGED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run generate traces nilling\n");
            }
            PatchUnit path = new PatchUnit(this);

            double millDiameter = 0.2;

            millPath.Clear();


            foreach (Net n in netList)
            {
                n.Renumerate();
                foreach (Figure f in n.figures)
                {
                    List<Polygon> pathPolygons = path.CreatePatch(f.shape, millDiameter);

                    foreach (Polygon p in pathPolygons)
                    {
                        millPath.Add(p);
                    }

                }
            }

            foreach (Net z in zones)
            {
                z.Renumerate();
                foreach (Figure f in z.figures)
                {
                    /*if (f.idx == 23)
                    { */
                    List<Polygon> pathPolygons = path.CreatePatch(f.shape, millDiameter);

                    if (pathPolygons.Count == 0)
                    {
                        PrintText("No path detected \n");
                    }

                    foreach (Polygon p in pathPolygons)
                    {
                        millPath.Add(p);
                    }

                    foreach (Polygon h in f.holes)
                    {
                        pathPolygons = path.CreatePatch(h, millDiameter);

                        if (pathPolygons.Count == 0)
                        {
                            PrintText("No path for hole detected \n");
                        }

                        foreach (Polygon p in pathPolygons)
                        {
                            millPath.Add(p);
                        }

                    }
                    // }

                }
            }

            SetState(STATE_et.TRACE_MILLING_GENERATED);
            RedrawAll();
            PrintText("Done\n");
        }

        private void ProceedBoardMilling()
        {
            if (state != STATE_et.TRACE_MILLING_GENERATED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run generate board milling\n");
            }

            PatchUnit path = new PatchUnit(this);

            double millDiameter = 1;

            boardMillPath.Clear();            

            if (board.shape.points.Count> 0)
            {
                List<Polygon> pathPolygons;
                pathPolygons = path.CreatePatch(board.shape, millDiameter);
                foreach (Polygon p in pathPolygons)
                {
                    boardMillPath.Add(p);
                }
            }

            foreach(Polygon h in board.holes)
            {
                List<Polygon> pathPolygons;
                pathPolygons = path.CreatePatch(h, millDiameter);
                foreach (Polygon p in pathPolygons)
                {
                    boardMillPath.Add(p);
                }
            }  
            RedrawAll();
            SetState(STATE_et.BOARD_MILLING_GENERATED);
            PrintText("Done\n");
        }

        private void ProceedHoles()
        {
            if (state != STATE_et.BOARD_MILLING_GENERATED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run proceed holes\n");
            }



            PrintText("Done\n");
            SetState(STATE_et.HOLES_PREPARED);
            
        }

        private void GenerateGCode()
        {
            if (state != STATE_et.HOLES_PREPARED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run generate G-Code\n");
            }



            PrintText("Done\n");
            SetState(STATE_et.GCODE_GENERATED);
        }

        public void AddFigure(Figure f)
        {
            netList[f.net].figures.Add(f);
            //figures.Add(f);
        }

        public void InitZone(int net)
        {
            foreach (Net z in zones)
            {
                if (z.net == net)
                {
                    /*zone already defined */
                    return;
                }
            }

            Net zn = new Net();
            zn.net = net;
            zn.figures = new List<Figure>();
            zones.Add(zn);
        }
        public void AddZoneFigure(Figure f)
        {
            foreach (Net z in zones)
            {
                if (z.net == f.net)
                {
                    z.figures.Add(f);
                    return;
                }
            }
        }

        public void AddCuts(Node n)
        {
            cuts.Add(n);
        }

        public void AddDrill(Drill drill)
        {
            drills.Add(drill);
        }

        public void InitNetList(int netCnt)
        {
            if (netCnt > 0)
            {
                netList = new Net[netCnt];
            }

            for (int i = 0; i < netCnt; i++)
            {
                netList[i] = new Net();
                netList[i].figures = new List<Figure>();
            }

        }



        internal void RedrawAll()
        {
            drawer.Redraw(netList, zones, board, drills, millPath, boardMillPath);
        }

        private bool Step(int phase)
        {
            /*step button */
            bool result;
            bool merged = false;

            List<Figure> listA = null;
            List<Figure> listB = null;

            if (phase == 0)
            {
                listA = netList[idxNet].figures;
                listB = netList[idxNet].figures;
            }
            else if (phase == 1)
            {

                foreach (Net z in zones)
                {
                    if (z.net == idxNet)
                    {
                        listA = z.figures;
                        break;
                    }
                }
                listB = netList[idxNet].figures;
            }
            else if (phase == 2)
            {
                foreach (Net z in zones)
                {
                    if (z.net == idxNet)
                    {
                        listA = z.figures;
                        listB = z.figures;

                        break;
                    }
                }
            }
            else
            {
                return false;
            }


            foreach (Figure f in netList[idxNet].figures)
            {
                f.shape.selected = 0;
            }

            if ((listA == listB) && (idxB == idxA)) { idxB++; }

            if (listA != null && idxA < listA.Count && idxB < listB.Count && ((listA != listB) || (idxA != idxB)))
            {

                merged = false;

                listA[idxA].shape.selected = 1;
                listB[idxB].shape.selected = 2;

                Figure mergedFigure = null;

                if(phase != 2 || listA[idxA].touched == true || listB[idxB].touched == true)
                {
                    mergedFigure = merger.Merge(listA[idxA], listB[idxB]);
                }
                


                if (mergedFigure != null)
                {

                    //PrintText("Found " + idxA.ToString() + " vs " + idxB.ToString() + "size = " + netList[idxNet].figures.Count.ToString() + "\n");

                    listA[idxA] = mergedFigure;
                    listB.RemoveAt(idxB);


                    merged = true;

                    if (phase >= 1)
                    {
                        mergedFigure.touched = true;
                    }
                }
                else
                {

                    //PrintText("skip\n");
                    idxB++;
                    if ((listA == listB) && (idxB == idxA)) { idxB++; }
                }

                if (merged == true)
                {
                    //idxB = 0;
                    if (listA == listB)
                    {
                        idxB = idxA + 1;
                    }
                    else
                    {
                        idxB = 0;
                    }

                }


                if (idxB >= listB.Count)
                {
                    idxA++;
                    if (listA == listB)
                    {
                        idxB = idxA + 1;
                    }
                    else
                    {
                        idxB = 0;
                    }
                }


                result = true;
            }
            else
            {
                idxNet++;
                if (idxNet < netList.Length)
                {
                    idxA = 0;
                    idxB = 0;

                    return true;
                }
                else
                {
                    //PrintText("Nothing to do ! \n");
                    result = false;
                }
            }
            return result;
        }



    }
}
