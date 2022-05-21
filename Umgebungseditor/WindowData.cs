using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Umgebungseditor
{
    class WindowData
    {
        public void Save()
        {

            string data = 
                $"{WindowLocation.X}|{WindowLocation.Y}|" +
                $"{WindowSize.Width}|{WindowSize.Height}|{WindowState}";
            string path = $"{Application.StartupPath}/Window Data.txt";
            File.WriteAllText(path, data);
        }
        public void Load()
        {
            try
            {
                string rawData = File.ReadAllText(
                    $"{Application.StartupPath}/Window Data.txt");
                string[] loadData = rawData.Split('|');

                WindowLocation = new Point(
                    int.Parse(loadData[0]), int.Parse(loadData[1]));
                WindowSize = new Size(
                    int.Parse(loadData[2]), int.Parse(loadData[3]));
                WindowState = (FormWindowState)Enum.Parse(
                    typeof(FormWindowState), loadData[4]);
            }
            catch
            {
                WindowLocation = Point.Empty;
                WindowSize = new Size(725, 570);
                WindowState = FormWindowState.Normal;
            }

        }
        public Point WindowLocation { get; set; }
        public Size WindowSize { get; set; }
        public FormWindowState WindowState { get; set; } = FormWindowState.Normal;
    }
}
