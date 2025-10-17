using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KiCad2Gcode.PcbFileParser;
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
            FIELDS_PREPARED,
            GCODE_GENERATED
        }

        

        public STATE_et state = STATE_et.IDLE;

        static Form1 mainForm;

        PcbFileParser pcbFileParser;
        internal Drawer drawer;
        Merger merger;
        Configuration config;

        FieldMillingUnit fieldMillingUnit;

        //List<Net> zones = new List<Net>();
        List<Node> cuts = new List<Node>();
        List<Polygon> cutPolygons = new List<Polygon>();
        List<Drill> drills = new List<Drill>();
        public Figure board;

        List<Polygon> millPath = new List<Polygon>();
        List<Polygon> boardHolesMillPath = new List<Polygon>();
        List<DrillList> sortedDrills = new List<DrillList>();
        Polygon boardMillPath;
        public  List<Polygon> millFieldsPath = new List<Polygon>();

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
            fieldMillingUnit = new FieldMillingUnit(this);

            SetState(STATE_et.IDLE);
        }

        public void SetConfig(Configuration config_)
        {
            this.config = config_;
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

        internal void ClearNetList()
        {
            foreach(Net n in netList)
            {
                n.figures.Clear();
                n.zoneFigures.Clear();
            }
        }
        internal void ClearAll()
        {
            foreach (Net n in netList)
            {
                n.figures.Clear();
                n.zoneFigures.Clear();
            }

            cuts.Clear();
            cutPolygons.Clear();
            drills.Clear();
            board = null; 

            
        }

        internal bool LoadFile(string filePath, ACTIVE_LAYER_et activeLayer_)
        {
            netList = null;
            cuts.Clear();
            cutPolygons.Clear();
            drills.Clear();
            //zones.Clear();

            millPath.Clear();
            boardHolesMillPath.Clear();
            boardMillPath = null;
            millFieldsPath.Clear();


            bool result = pcbFileParser.Parse(filePath, activeLayer_);

            if (result)
            { 
                foreach (Net n in netList)
                {
                    foreach (Figure f in n.figures)
                    {
                        f.shape.GetExtPoints();
                    }
                    foreach (Figure f in n.zoneFigures)
                    {
                        f.shape.GetExtPoints();
                    }
                }

                BorderUnit borderUnit = new BorderUnit();
                board = borderUnit.SortNets(cuts, cutPolygons);

                board.shape.GetExtPoints();
                foreach (Polygon h in board.holes)
                {
                    h.GetExtPoints();
                }

                Point2D offset = FindCornerPoint();
                if(offset != null)
                {
                    Vector moveVector = new Vector(-offset.x, -offset.y);


                    foreach (Net n in netList)
                    {
                        foreach (Figure f in n.figures)
                        {
                            f.Move(moveVector);
                        }
                        foreach (Figure f in n.zoneFigures)
                        {
                            f.Move(moveVector);
                        }
                    }

                    board.Move(moveVector);

                    foreach (Drill d in drills)
                    {
                        d.pos += moveVector;
                    }
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
                    ProceedFields();
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
                case 7: /*PROCEED FIELDS */
                    ProceedFields();
                    break;
                case 8: /*GENERATE G - CODE*/
                    GenerateGCode();
                    break;

                case 10:
                    ProceedFields();
                    //RedrawAll();
                    break;

            }


        }

        internal void TestStep()
        {
            Step(0);
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

            foreach(Net net in netList)
            {
                bool res = false;

                merger.Init(net.figures, net.figures);

                do
                {
                    res = merger.Step(0);
                } while (res == true);
            }
            RedrawAll();


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


            foreach (Net net in netList)
            {
                bool res = false;

                merger.Init(net.zoneFigures, net.figures);

                do
                {
                    res = merger.Step(1);
                } while (res == true);
            }
            RedrawAll();

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

            foreach (Net net in netList)
            {
                bool res = false;

                merger.Init(net.zoneFigures, net.zoneFigures);

                do
                {
                    res = merger.Step(2);
                } while (res == true);
            }
            RedrawAll();

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

            double millDiameter = config.traceMillDiameter;

            millPath.Clear();


            foreach (Net n in netList)
            {
                n.Renumerate();
                foreach (Figure f in n.figures)
                {
                    List<Polygon> pathPolygons = path.CreatePatch(f.shape, millDiameter, true);

                    foreach (Polygon p in pathPolygons)
                    {
                        millPath.Add(p);
                    }

                    foreach (Polygon h in f.holes)
                    {
                        pathPolygons = path.CreatePatch(h, millDiameter, false);

                        if (pathPolygons.Count == 0)
                        {
                            PrintText("No path for hole detected \n");
                        }

                        foreach (Polygon p in pathPolygons)
                        {
                            millPath.Add(p);
                        }

                    }
                }

                foreach (Figure f in n.zoneFigures)
                {
                    /*if (f.idx == 23)
                    { */
                    List<Polygon> pathPolygons = path.CreatePatch(f.shape, millDiameter, true);

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
                        pathPolygons = path.CreatePatch(h, millDiameter, false);

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

            double millDiameter = config.boardMillDiameter;

            boardHolesMillPath.Clear();            

            if (board.shape.points.Count> 0)
            {
                List<Polygon> pathPolygons;
                pathPolygons = path.CreatePatch(board.shape, millDiameter, true);
                if(pathPolygons != null && pathPolygons.Count > 0)
                {
                    boardMillPath = pathPolygons[0];
                }
            }

            foreach(Polygon h in board.holes)
            {
                List<Polygon> pathPolygons;
                pathPolygons = path.CreatePatch(h, millDiameter, false);
                foreach (Polygon p in pathPolygons)
                {
                    boardHolesMillPath.Add(p);
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

            sortedDrills.Clear();

            foreach(Drill d in drills)
            {
                if(config.boardDrillsActive && d.diameter >= config.boardMillDiameter)
                {
                    if(board == null)
                    {
                        board = new Figure();
                        board.shape = null;
                    }

                    Polygon p = new Polygon();

                    Node n = new Node();
                    Arc arc = new Arc();
                    arc.startAngle = -Math.PI;
                    arc.endAngle = Math.PI;
                    arc.radius = (d.diameter - config.boardMillDiameter)/2;
                    arc.centre = d.pos;
                    arc.ccw = true;
                    n.pt = new Point2D(arc.centre);
                    n.pt.x -= arc.radius;
                    n.arc = arc;
                    p.points.AddLast(n);
                    board.holes.Add(p);

                }
                else
                {
                    /*search the best drill size for given hole*/
                    DrillData drill = config.GetBestDrill(d.diameter);
                    if(drill != null)
                    {
                        DrillList dList = sortedDrills.Find(dl => dl.drillData == drill);
                        if(dList == null)
                        {
                            dList = new DrillList();
                            dList.drillData = drill;
                            dList.pts = new List<Point2D>();
                            sortedDrills.Add(dList);
                        }

                        dList.pts.Add(d.pos);

                        foreach (DrillList dl in sortedDrills)
                        {


                            if(dl.drillData == drill)
                            {

                            }
                        }
                    }
                    else
                    {
                        PrintText("WARNING: Drill not found for hole " + d.diameter.ToString() + "\n");
                    }
                }
            }

            RedrawAll();
            PrintText("Done\n");
            SetState(STATE_et.HOLES_PREPARED);
            
        }

        private void ProceedFields()
        {
            if (state != STATE_et.HOLES_PREPARED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run proceed fields\n");
            }

            millFieldsPath.Clear();

            if(config.fieldActive)
            {
                double firstOffset = 0;
                double step = 0;
                if (config.fieldUseTraceMill)
                {
                    firstOffset = 1.5 * config.traceMillDiameter;
                    step = config.traceMillDiameter;
                }
                else
                {
                    firstOffset = config.fieldMillDiameter + 0.5 * config.traceMillDiameter;
                    step = config.fieldMillDiameter;
                }
                firstOffset *= 0.8;
                step *= 0.8;

                fieldMillingUnit.CreateFields(board, netList,firstOffset, step);
            }            

            PrintText("Done\n");
            SetState(STATE_et.FIELDS_PREPARED);
        }

        private void GenerateGCode()
        {
            if (state != STATE_et.FIELDS_PREPARED)
            {
                PrintText("File not loaded or invalid state\n");
                return;
            }
            else
            {
                PrintText("Run generate G-Code\n");
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = "GCODE file";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                GCodeGenerator generator = new GCodeGenerator(config);
                generator.Generate(saveFileDialog.FileName, sortedDrills, millPath, boardHolesMillPath, boardMillPath);
            }

            PrintText("Done\n");
            SetState(STATE_et.GCODE_GENERATED);
        }

        public void AddFigure(Figure f)
        {
            netList[f.net].figures.Add(f);
        }


        public void AddZoneFigure(Figure f)
        {
            netList[f.net].zoneFigures.Add(f);
        }

        public void AddCuts(Node n)
        {
            cuts.Add(n);
        }

        public void AddCutsPolygon(Polygon p)
        {
            cutPolygons.Add(p);
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
            drawer.Redraw(netList, board, drills, millPath,  boardMillPath, boardHolesMillPath, millFieldsPath);
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
                /*
                foreach (Net z in zones)
                {
                    if (z.net == idxNet)
                    {
                        listA = z.figures;
                        break;
                    }
                }
                listB = netList[idxNet].figures;*/
            }
            else if (phase == 2)
            {
                /*foreach (Net z in zones)
                {
                    if (z.net == idxNet)
                    {
                        listA = z.figures;
                        listB = z.figures;

                        break;
                    }
                }*/
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

        private Point2D FindCornerPoint()
        {
            Point2D corner = null;

            if (board != null && board.shape != null && board.shape.points.Count > 0)
            {
                board.shape.GetExtPoints();

                corner = new Point2D(board.shape.extPoint[0].x, board.shape.extPoint[3].y);

            }
            else if (netList.Length > 0)
            {
                foreach(Net nz in netList)
                {
                    if(nz.zoneFigures != null && nz.zoneFigures.Count > 0)
                    {
                        foreach(Figure f in  nz.zoneFigures)
                        {
                            f.shape.GetExtPoints();

                            if(corner == null)
                            {
                                corner = new Point2D(f.shape.extPoint[0].x, f.shape.extPoint[3].y);
                            }
                            else
                            {
                                if (corner.x > f.shape.extPoint[0].x) { corner.x = f.shape.extPoint[0].x; }
                                if (corner.y > f.shape.extPoint[3].y) { corner.y = f.shape.extPoint[3].y; }
                            }


                        }
                    }
                }

            }
            return corner;

        }


    }
}
