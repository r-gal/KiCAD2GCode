using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace KiCad2Gcode
{
    internal class GCodeGenerator
    {
        Configuration config;

        string F = "F3";
        IFormatProvider I = CultureInfo.CreateSpecificCulture("en-US");
        public GCodeGenerator(Configuration config_)
        {
            this.config = config_;
        }

        private void AddToolChange(StreamWriter file, int toolNumber, double spindleSpeed, string comment)
        {
            file.WriteLine("(Tool change : " + comment + " )");
            file.WriteLine("M5");
            file.WriteLine("M6 T" + toolNumber.ToString());
            file.WriteLine("G43 H" + toolNumber.ToString());
            file.WriteLine("M3 S"+ spindleSpeed.ToString("F0"));
            if(config.m3dwel > 0)
            {
                file.WriteLine("G4 P" + config.m3dwel.ToString("F0"));
            }
            file.WriteLine("G0 Z" + config.clearLevel.ToString(F,I));
        }

        private void AddStopTool(StreamWriter file)
        {
            file.WriteLine("(Tool stop)");
            file.WriteLine("M5");
        }

        private void GeneratePath(StreamWriter file, Polygon polygon, double feedRate, double z)
        {
            Point2D startPt = polygon.points.Last.Value.pt;

            LinkedListNode<Node> nll = polygon.points.First;
            while(nll != null)
            {
                Node n = nll.Value;
                if(n.arc == null)
                {
                    file.WriteLine("G1 X" + n.pt.x.ToString(F,I ) + " Y" + n.pt.y.ToString(F,I) + " Z" + z.ToString(F, I) + " F" + feedRate.ToString(F,I));
                }
                else
                {
                    double cx = n.arc.centre.x - startPt.x;
                    double cy = n.arc.centre.y - startPt.y;

                    /*consistency check */

                    double cxe = n.arc.centre.x - n.pt.x;
                    double cye = n.arc.centre.y - n.pt.y;

                    double r1 = cx * cx + cy * cy;
                    double r2 = cxe * cxe + cye * cye;

                    r1 = Math.Sqrt(r1);
                    r2 = Math.Sqrt(r2);

                    if (Math.Abs(n.arc.radius - r1) > 0.001)
                    {
                        int trap = 0;
                    }
                    if (Math.Abs(n.arc.radius - r2) > 0.001)
                    {
                        int trap = 0;
                    }

                    string instruction = n.arc.ccw ? "G3" : "G2";

                    string epStr = "";

                    if(n.pt.IsSameAs(startPt) == false)
                    {
                        epStr = " X" + n.pt.x.ToString(F,I) + " Y" + n.pt.y.ToString(F,I);
                    }

                    //string commentStr = " ( cX" + n.arc.centre.x.ToString(F,I) + " cY" + n.arc.centre.y.ToString(F,I) + " )";

                    file.WriteLine(instruction + epStr + " Z" + z.ToString(F, I) + " I" + cx.ToString(F, I) + " J" + cy.ToString(F, I) + " F" + feedRate.ToString(F, I));// commentStr);
                }
                startPt = n.pt;
                nll = nll.Next;
            }
        }

        private Polygon FindNearestPolygon(List<Polygon> list, Point2D pt)
        {
            Polygon pRet = null;
            double actLen = 0;

            foreach( Polygon p in list)
            {
                if(p.selected == 0 && p.points.Count > 0)
                {
                    Point2D s = p.points.First.Value.pt;
                    double len = (pt.x - s.x) * (pt.x - s.x) + (pt.y - s.y) * (pt.y - s.y);

                    if(pRet == null || len < actLen)
                    {
                        actLen = len;
                        pRet = p;
                    }
                }
            }    

            return pRet;
        }

        private Point2D FindNearestPoint(List<Point2D> list, Point2D pt)
        {
            Point2D pRet = null;
            double actLen = 0;

            foreach (Point2D p in list)
            {
                if (p.state == Point2D.STATE_et.FREE )
                {
                    double len = (pt.x - p.x) * (pt.x - p.x) + (pt.y - p.y) * (pt.y - p.y);

                    if (pRet == null || len < actLen)
                    {
                        actLen = len;
                        pRet = p;
                    }
                }
            }

            return pRet;
        }

        public static int CompareByDiameter(DrillList drill1, DrillList drill2)
        {
            return drill1.drillData.diameter.CompareTo(drill2.drillData.diameter);  
        }

        public void Generate(string fileName, List<DrillList> drills, List<Polygon> traces, List<Polygon> fields, List<Polygon> cuts, Polygon outerCut)
        {
            /*create file */
            StreamWriter file = File.CreateText(fileName);
            if(file == null)
            {
                return;
            }


            /* generate preamble */
            file.WriteLine("(Generated by KiCAD2GCode)");
            file.WriteLine("(begin preamble)");
            file.WriteLine("G17 G54 G40 G49 G80 G90");
            file.WriteLine("G21");
            file.WriteLine("G0 Z" + config.clearLevel.ToString(F,I)) ;




            /*generate traces*/

            bool toolIsRunning = false;

            if(traces != null && traces.Count > 0 && config.traceActive)
            {

                AddToolChange(file, config.traceMillToolNumber, config.traceMillSpindleSpeed, "mill tool");
                file.WriteLine("(start traces milling)");

                foreach (Polygon p in traces)
                {
                    p.selected = 0;
                }

                Point2D startPoint = new Point2D(0, 0);

                bool cont = true;
                while (cont)
                {
                    Polygon polygon = FindNearestPolygon(traces, startPoint);
                    if (polygon == null)
                    {
                        cont = false;
                    }
                    else
                    {
                        startPoint = polygon.points.First.Value.pt;
                        polygon.selected = 1;

                        double x = polygon.points.Last.Value.pt.x;
                        double y = polygon.points.Last.Value.pt.y;


                        /* add selected to mill */
                        file.WriteLine("G0 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.clearLevel.ToString(F, I));
                        file.WriteLine("G0 Z" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.safeLevel.ToString(F, I));
                        file.WriteLine("G1 Z" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.traceMillLevel.ToString(F, I) + " F" + config.traceMillVFeedRate.ToString(F, I));

                        GeneratePath(file, polygon, config.traceMillHFeedRate, config.traceMillLevel);

                        file.WriteLine("G0 Z" + config.clearLevel.ToString(F, I));

                    }
                }

                if (fields != null && fields.Count > 0 && config.fieldActive && config.fieldUseTraceMill)
                {
                    /* left running for fields milling */
                    toolIsRunning = true;
                }
                else
                {
                    AddStopTool(file);
                }

                file.WriteLine("(stop traces milling)");

                
                
            }

            /* generate fields */

            if (fields != null && fields.Count > 0 && config.fieldActive)
            {

                file.WriteLine("(start fields milling)");

                if(toolIsRunning == false)
                {
                    if(config.fieldUseTraceMill)
                    {
                        AddToolChange(file, config.traceMillToolNumber, config.traceMillSpindleSpeed, "field mill tool");
                    }
                    else
                    {
                        AddToolChange(file, config.fieldMillToolNumber, config.fieldMillSpindleSpeed, "field mill tool");
                    }                    
                }

                foreach (Polygon p in fields)
                {
                    p.selected = 0;
                }

                Point2D startPoint = new Point2D(0, 0);

                bool cont = true;
                while(cont)
                {
                    Polygon polygon = FindNearestPolygon(fields, startPoint);
                    if (polygon == null)
                    {
                        cont = false;
                    }
                    else
                    {
                        startPoint = polygon.points.First.Value.pt;
                        polygon.selected = 1;


                        double vFeedRate = config.fieldUseTraceMill ? config.traceMillVFeedRate : config.fieldMillVFeedRate;
                        double hFeedRate = config.fieldUseTraceMill ? config.traceMillHFeedRate : config.fieldMillHFeedRate;
                        double millLevel = config.fieldUseTraceMill ? config.traceMillLevel : config.fieldMillLevel;

                        double x = polygon.points.Last.Value.pt.x;
                        double y = polygon.points.Last.Value.pt.y;

                        /* add selected to mill */
                        file.WriteLine("G0 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.clearLevel.ToString(F, I));
                        file.WriteLine("G0 Z" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.safeLevel.ToString(F, I));
                        file.WriteLine("G1 Z" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" +  millLevel.ToString(F, I) + " F" + vFeedRate.ToString(F, I));

                        GeneratePath(file, polygon, hFeedRate, millLevel );

                        file.WriteLine("G0 Z" + config.clearLevel.ToString(F, I));

                    }
                }

                file.WriteLine("(stop field milling)");
                AddStopTool(file);
            }

            /*generate drills */
            if (config.drillAcive)
            {
                drills.Sort(CompareByDiameter);



                foreach (DrillList drill in drills)
                {
                    Point2D startPoint = new Point2D(0, 0);


                    foreach (Point2D p in drill.pts)
                    {
                        p.state = Point2D.STATE_et.FREE;
                    }


                    AddToolChange(file, drill.drillData.toolNumber, drill.drillData.spindleSpeed, "drill " + drill.drillData.diameter.ToString(F, I) + "[mm]");
                    file.WriteLine("(start drilling)");

                    bool cont = true;

                    while(cont)
                    {
                        Point2D p = FindNearestPoint(drill.pts, startPoint);

                        if(p == null)
                        {
                            cont = false;
                        }
                        else
                        {
                            file.WriteLine("G0 X" + p.x.ToString(F, I) + " Y" + p.y.ToString(F, I) + " Z" + config.clearLevel.ToString(F, I));
                            file.WriteLine("G0 X" + p.x.ToString(F, I) + " Y" + p.y.ToString(F, I) + " Z" + config.safeLevel.ToString(F, I));
                            file.WriteLine("G81 X" + p.x.ToString(F, I) + " Y" + p.y.ToString(F, I) + " Z" + config.drillLevel.ToString(F, I) + " R" + config.safeLevel.ToString(F, I) + " F" + drill.drillData.feedRate.ToString(F, I));
                            p.state = Point2D.STATE_et.USED;
                        }

                    }

                    file.WriteLine("(stop drilling)");
                    file.WriteLine("G80");
                    file.WriteLine("G0 Z" + config.clearLevel.ToString(F, I));
                    AddStopTool(file);
                }
            }

            /*generate board holes */
            if (cuts != null && cuts.Count> 0 && config.boardHolesActive)
            {
                AddToolChange(file, config.traceMillToolNumber, config.traceMillSpindleSpeed, "mill tool");
                file.WriteLine("(start holes milling)");

                double actLevel = config.safeLevel;                

                do
                {
                    actLevel -= config.boardVStep;

                    if(actLevel < config.boardMillLevel)
                    {
                        actLevel = config.boardMillLevel;
                    }
                    foreach (Polygon polygon in cuts)
                    {
                        double x = polygon.points.Last.Value.pt.x;
                        double y = polygon.points.Last.Value.pt.y;

                        file.WriteLine("G0 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.clearLevel.ToString(F, I));
                        file.WriteLine("G0 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.safeLevel.ToString(F, I));
                        file.WriteLine("G1 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + actLevel.ToString(F, I) + " F" + config.boardMillVFeedRate.ToString(F, I));

                        GeneratePath(file, polygon, config.boardMillHFeedRate, actLevel);

                        file.WriteLine("G0 Z" + config.clearLevel.ToString(F, I));
                    }
                } while (actLevel > config.boardMillLevel);

                file.WriteLine("(stop holes milling)");
                AddStopTool(file);


            }



            /*generate board shape */
            if(outerCut != null && config.boardBorderActive)
            {
                AddToolChange(file, config.traceMillToolNumber, config.traceMillSpindleSpeed, "mill tool");
                file.WriteLine("(start board milling)");

                Polygon polygon = outerCut;

                double actLevel = config.safeLevel;
                do
                {
                    actLevel -= config.boardVStep;

                    if (actLevel < config.boardMillLevel)
                    {
                        actLevel = config.boardMillLevel;
                    }

                    double x = polygon.points.Last.Value.pt.x;
                    double y = polygon.points.Last.Value.pt.y;

                    file.WriteLine("G0 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.clearLevel.ToString(F, I));
                    file.WriteLine("G0 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + config.safeLevel.ToString(F, I));
                    file.WriteLine("G1 X" + x.ToString(F, I) + " Y" + y.ToString(F, I) + " Z" + actLevel.ToString(F, I) + " F" + config.boardMillVFeedRate.ToString(F, I));

                    GeneratePath(file, polygon, config.boardMillHFeedRate, actLevel);

                    file.WriteLine("G0 Z" + config.clearLevel.ToString(F,I));
                } while (actLevel > config.boardMillLevel);
                file.WriteLine("(stop board milling)");
                AddStopTool(file);

            }

            /* add postamble */
            file.WriteLine("G17 G54 G90 G80 G40");
            file.WriteLine("M2");
            /*save file */
            file.Close();
        }
    }
}
