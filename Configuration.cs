using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace KiCad2Gcode
{
    public class DrillData
    {
        public int toolNumber;
        public double diameter;
        public double spindleSpeed;
        public double feedRate;

    }
    public class Configuration
    {
        /* general */

        public double clearLevel = 20;
        public double safeLevel = 1;
        public double m3dwel = 0;

        /* traces milling */
        public int traceMillToolNumber = 1;
        public double traceMillDiameter = 0.2;
        public double traceMillSpindleSpeed = 18000;
        public double traceMillHFeedRate = 800;
        public double traceMillVFeedRate = 800;
        public double traceMillLevel = -0.05;
        public bool traceActive = true;

        /* board milling */

        public int boardMillToolNumber = 2;
        public double boardMillDiameter = 0.2;
        public double boardMillSpindleSpeed = 18000;
        public double boardMillHFeedRate = 800;
        public double boardMillVFeedRate = 800;
        public double boardMillLevel = -0.05;
        public double boardVStep = 1.5;
        public bool boardBorderActive = true;
        public bool boardHolesActive = true;
        public bool boardDrillsActive = true;

        /* drilling */

        public List<DrillData> drillList = new List<DrillData> ();
        public double drillLevel = -1.6;
        public bool drillAcive = true;

        /* field milling */

        public int fieldMillToolNumber = 3;
        public double fieldMillDiameter = 0.2;
        public double fieldMillSpindleSpeed = 18000;
        public double fieldMillHFeedRate = 800;
        public double fieldMillVFeedRate = 800;
        public double fieldMillLevel = -0.05;
        public bool fieldActive = false;
        public bool fieldUseTraceMill = true;



        internal void AddDrill(DrillData drill)
        {
            drillList.Add (drill);

            
        }

        internal int GetFirstFreeToolNumber()
        {
            int toolNumber = 1;
            while(true)
            {
                if( CheckIfToolNumberIsFree(toolNumber) )
                {
                    return toolNumber;
                }
                toolNumber++;
            }
        }

        internal bool CheckIfToolNumberIsFree(int toolNumber)
        {
            if (traceMillToolNumber == toolNumber) { return false; }
            if (boardMillToolNumber == toolNumber) { return false; }
            if (fieldMillToolNumber == toolNumber) { return false; }

            return CheckIfToolNumberIsNotUsedByDrills(toolNumber);
        }

        internal bool CheckIfToolNumberIsNotUsedByDrills(int toolNumber)
        {
            foreach (DrillData drill in drillList)
            {
                if (drill.toolNumber == toolNumber)
                {
                    return false;
                }
            }
            return true;
        }

        internal void DeleteDrill(int toolNumber)
        {
            DrillData foundDrill = null;
            foreach (DrillData drill in drillList)
            {
                if (drill.toolNumber == toolNumber)
                {
                    foundDrill = drill;
                    break;
                }
            }

            if(foundDrill != null)
            {
                drillList.Remove(foundDrill);
            }
        }

        internal DrillData GetBestDrill(double d)
        {
            DrillData result = null;

            foreach(DrillData drill in drillList)
            {
                if(drill.diameter >= d)
                {
                    if(result == null)
                    {
                        result = drill;
                    }
                    else if(result.diameter > drill.diameter)
                    {
                        result = drill;
                    }
                }
            }
            /* found the smallest drill but not smaller than hole */



            return result;
        }

        internal void LoadFromFile(string fileName)
        {
            XmlDocument config = new XmlDocument();



            bool ok = false;

            try
            {
                config.Load(fileName);
                ok = true;
            }
            catch
            {
                MessageBox.Show("Invalid file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (ok)
            {
                XmlNode node;

                try
                {
                    node = config.SelectSingleNode("ROOT/GENERAL/safeLevel");
                    safeLevel = Double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/GENERAL/clearLevel");
                    clearLevel = Double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/GENERAL/m3Dwel");
                    m3dwel = Double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/active");
                    traceActive = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/toolNumber");
                    traceMillToolNumber = int.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/diameter");
                    traceMillDiameter = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/level");
                    traceMillLevel = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/HFeedRate");
                    traceMillHFeedRate = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/VFeedRate");
                    traceMillVFeedRate = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/TRACE/SpindleSpeed");
                    traceMillSpindleSpeed = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/borderActive");
                    boardBorderActive = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/holesActive");
                    boardHolesActive = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/drillsActive");
                    boardDrillsActive = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/toolNumber");
                    boardMillToolNumber = int.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/diameter");
                    boardMillDiameter = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/level");
                    boardMillLevel = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/HFeedRate");
                    boardMillHFeedRate = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/VFeedRate");
                    boardMillVFeedRate = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/SpindleSpeed");
                    boardMillSpindleSpeed = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/BOARD/VStep");
                    boardVStep = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/DRILLS/active");
                    drillAcive = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/DRILLS/level");
                    drillLevel = double.Parse(node.InnerText);
                }
                catch { ok = false; }


                int drillsCount = 0;

                drillList.Clear();

                try
                {
                    node = config.SelectSingleNode("ROOT/DRILLS/drillsCount");
                    drillsCount = int.Parse(node.InnerText);

                    for(int i =0;i<drillsCount;i++)
                    {
                        DrillData data = new DrillData();
                        node = config.SelectSingleNode("ROOT/DRILLS/DRILL_"+ i.ToString()+"/toolNumber");
                        data.toolNumber = int.Parse(node.InnerText);
                        node = config.SelectSingleNode("ROOT/DRILLS/DRILL_" + i.ToString() + "/diameter");
                        data.diameter = double.Parse(node.InnerText);
                        node = config.SelectSingleNode("ROOT/DRILLS/DRILL_" + i.ToString() + "/FeedRate");
                        data.feedRate = double.Parse(node.InnerText);
                        node = config.SelectSingleNode("ROOT/DRILLS/DRILL_" + i.ToString() + "/SpindleSpeed");
                        data.spindleSpeed = double.Parse(node.InnerText);
                        drillList.Add(data);
                    }

                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/active");
                    fieldActive = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/useTraceMill");
                    fieldUseTraceMill = Boolean.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/toolNumber");
                    fieldMillToolNumber = int.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/diameter");
                    fieldMillDiameter = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/level");
                    fieldMillLevel = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/HFeedRate");
                    fieldMillHFeedRate = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/VFeedRate");
                    fieldMillVFeedRate = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/FIELD/SpindleSpeed");
                    fieldMillSpindleSpeed = double.Parse(node.InnerText);
                }
                catch { ok = false; }

            }

            if(ok)
            {

            }
        }

        internal void SaveToFile(string fileName)
        {
            XmlDocument config = new XmlDocument();

            XmlElement rootElement = (XmlElement)config.AppendChild(config.CreateElement("ROOT"));

            XmlElement generalConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("GENERAL"));

            generalConfig.AppendChild(config.CreateElement("safeLevel")).InnerText = safeLevel.ToString();
            generalConfig.AppendChild(config.CreateElement("clearLevel")).InnerText = clearLevel.ToString();
            generalConfig.AppendChild(config.CreateElement("m3Dwel")).InnerText = m3dwel.ToString();

            XmlElement traceConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("TRACE"));

            traceConfig.AppendChild(config.CreateElement("active")).InnerText = traceActive.ToString();
            traceConfig.AppendChild(config.CreateElement("toolNumber")).InnerText = traceMillToolNumber.ToString();
            traceConfig.AppendChild(config.CreateElement("diameter")).InnerText = traceMillDiameter.ToString();
            traceConfig.AppendChild(config.CreateElement("level")).InnerText = traceMillLevel.ToString();
            traceConfig.AppendChild(config.CreateElement("HFeedRate")).InnerText = traceMillHFeedRate.ToString();
            traceConfig.AppendChild(config.CreateElement("VFeedRate")).InnerText = traceMillVFeedRate.ToString();
            traceConfig.AppendChild(config.CreateElement("SpindleSpeed")).InnerText = traceMillSpindleSpeed.ToString();

            XmlElement boardConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("BOARD"));

            boardConfig.AppendChild(config.CreateElement("borderActive")).InnerText = boardBorderActive.ToString();
            boardConfig.AppendChild(config.CreateElement("holesActive")).InnerText = boardHolesActive.ToString();
            boardConfig.AppendChild(config.CreateElement("drillsActive")).InnerText = boardDrillsActive.ToString();
            boardConfig.AppendChild(config.CreateElement("toolNumber")).InnerText = boardMillToolNumber.ToString();
            boardConfig.AppendChild(config.CreateElement("diameter")).InnerText = boardMillDiameter.ToString();
            boardConfig.AppendChild(config.CreateElement("level")).InnerText = boardMillLevel.ToString();
            boardConfig.AppendChild(config.CreateElement("HFeedRate")).InnerText = boardMillHFeedRate.ToString();
            boardConfig.AppendChild(config.CreateElement("VFeedRate")).InnerText = boardMillVFeedRate.ToString();
            boardConfig.AppendChild(config.CreateElement("SpindleSpeed")).InnerText = boardMillSpindleSpeed.ToString();
            boardConfig.AppendChild(config.CreateElement("VStep")).InnerText = boardVStep.ToString();

            XmlElement drillsConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("DRILLS"));

            drillsConfig.AppendChild(config.CreateElement("active")).InnerText = drillAcive.ToString();
            drillsConfig.AppendChild(config.CreateElement("level")).InnerText = drillLevel.ToString();

            drillsConfig.AppendChild(config.CreateElement("drillsCount")).InnerText = drillList.Count.ToString();    

            for(int i = 0; i < drillList.Count; i++)
            {
                XmlElement drillConfig = (XmlElement)drillsConfig.AppendChild(config.CreateElement("DRILL_" + i.ToString()));

                drillConfig.AppendChild(config.CreateElement("toolNumber")).InnerText = drillList[i].toolNumber.ToString();
                drillConfig.AppendChild(config.CreateElement("diameter")).InnerText = drillList[i].diameter.ToString();
                drillConfig.AppendChild(config.CreateElement("FeedRate")).InnerText = drillList[i].feedRate.ToString();
                drillConfig.AppendChild(config.CreateElement("SpindleSpeed")).InnerText = drillList[i].spindleSpeed.ToString();
            }

            XmlElement fieldConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("FIELD"));

            fieldConfig.AppendChild(config.CreateElement("active")).InnerText = fieldActive.ToString();
            fieldConfig.AppendChild(config.CreateElement("useTraceMill")).InnerText = fieldUseTraceMill.ToString();
            fieldConfig.AppendChild(config.CreateElement("toolNumber")).InnerText = fieldMillToolNumber.ToString();
            fieldConfig.AppendChild(config.CreateElement("diameter")).InnerText = fieldMillDiameter.ToString();
            fieldConfig.AppendChild(config.CreateElement("level")).InnerText = fieldMillLevel.ToString();
            fieldConfig.AppendChild(config.CreateElement("HFeedRate")).InnerText = fieldMillHFeedRate.ToString();
            fieldConfig.AppendChild(config.CreateElement("VFeedRate")).InnerText = fieldMillVFeedRate.ToString();
            fieldConfig.AppendChild(config.CreateElement("SpindleSpeed")).InnerText = fieldMillSpindleSpeed.ToString();

            config.Save(fileName);
        }

    }
}
