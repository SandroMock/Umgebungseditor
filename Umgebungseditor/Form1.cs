using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Umgebungseditor
{
    public partial class Umgebungseditor : Form
    {
        int tileWidth = 50;
        int tileHeight = 50;

        Image map = null;
        Image activeTile = null;
        Image[,] tiles = null;

        Dictionary<string, Image> allTiles = new Dictionary<string, Image>();

        string activeTileName = "";
        string savePath = null;
        string[,] tileNames = null;

        enum MODE { DRAW, EDIT, COUNT};
        MODE mode = MODE.DRAW;

        public Umgebungseditor()
        {
            InitializeComponent();
            LoadWindowSettings();
            LoadTiles();
            NewMap();
            tiles = new Image[15, 15];
            tileNames = new string[15, 15];
            propertyGrid.PropertySort = PropertySort.NoSort;
        }

        // Konstruktorüberladung für Befehlszeilenparameter
        public Umgebungseditor(string _commandLine)
        {
            InitializeComponent();
            LoadWindowSettings();
            LoadTiles();
            NewMap();
            tiles = new Image[15, 15];
            tileNames = new string[15, 15];
            propertyGrid.PropertySort = PropertySort.NoSort;
            LoadMap(_commandLine);
        }

        // Fenstereigenschaften laden
        void LoadWindowSettings()
        {
            var wd = new WindowData();
            wd.Load();
            Size = wd.WindowSize;
            Location = wd.WindowLocation;
            WindowState = wd.WindowState;
        }

        // Anzeigen der Tiles
        void LoadTiles()
        {
            if (!Directory.Exists(Application.StartupPath + "/Background") ||
                !Directory.Exists(Application.StartupPath + "/Foreground"))
            {
                MessageBox.Show(
                    "Die Dateien exsitieren nicht.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] backgroundTiles = Directory.GetFiles(
                Application.StartupPath + "/Background/", "*.png");
            string[] foregroundTiles = Directory.GetFiles(
                Application.StartupPath + "/Foreground/", "*.png");

            for (int i = 0; i < backgroundTiles.Length; i++)
            {
                var tileCtrl = new tileControl();
                FlowMenuPanel.Controls.Add(tileCtrl);
                string name = Path.GetFileName(backgroundTiles[i]);
                name = name.Replace(".png", "");
                Image tileImage = Image.FromFile(backgroundTiles[i]);
                allTiles.Add(name, tileImage);
                tileCtrl.ShowControl(tileImage, name);
                tileCtrl.userSelect += ChangeTile;
                tileCtrl.Focus();
            }

            for (int i = 0; i < foregroundTiles.Length; i++)
            {
                var tileCtrl = new tileControl();
                FlowMenuPanel.Controls.Add(tileCtrl);
                string name = Path.GetFileName(foregroundTiles[i]);
                name = name.Replace(".png", "");
                Image tileImage = Image.FromFile(foregroundTiles[i]);
                if(allTiles.ContainsKey(name))
                {
                    name += " 1";
                }
                allTiles.Add(name, tileImage);
                tileCtrl.ShowControl(tileImage, name);
                tileCtrl.userSelect += ChangeTile;
                tileCtrl.Focus();
            }
        }

        // Umschalten von zeichnen oder editieren der Tiles
        void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            switch (mode)
            {
                case MODE.DRAW:
                    DrawTile(e);
                    break;
                case MODE.EDIT:
                    EditTile(e);
                    break;
            }
        }

        // Tile zeichnen
        void DrawTile(MouseEventArgs e)
        {
            if (activeTile == null) return;
            Point point = new Point(0, 0);
            point.X = e.Location.X - (e.Location.X % tileWidth);
            point.Y = e.Location.Y - (e.Location.Y % tileHeight);
            Point tilePosition = Point.Empty;
            tilePosition.X = point.X / 50;
            tilePosition.Y = point.Y / 50;
            var rect = new Rectangle(
                point, new Size(tileWidth, tileHeight));
            Image image = map;
            var graphics = Graphics.FromImage(image);
            graphics.DrawImage(activeTile, rect);
            tiles[tilePosition.X, tilePosition.Y] = activeTile;
            tileNames[tilePosition.X, tilePosition.Y] = activeTileName;
            this.Invalidate(true);
            this.Update();
        }

        //Zeichnen eines neuen Tiles
        void DrawTile(Image tileToDraw, string _tileName, MouseEventArgs e)
        {
            if (tileToDraw == null) return;
            var point = new Point(0, 0);
            point.X = e.Location.X - (e.Location.X % tileWidth);
            point.Y = e.Location.Y - (e.Location.Y % tileHeight);
            Point tilePosition = Point.Empty;
            tilePosition.X = point.X / 50;
            tilePosition.Y = point.Y / 50;
            var rect = new Rectangle(
                point, new Size(tileWidth, tileHeight));
            Image image = map;
            var graphics = Graphics.FromImage(image);
            graphics.DrawImage(tileToDraw, rect);
            tiles[tilePosition.X, tilePosition.Y] = tileToDraw;
            tileNames[tilePosition.X, tilePosition.Y] = _tileName;
            this.Invalidate(true);
            this.Update();
        }

        // Tiles editieren
        void EditTile(MouseEventArgs e)
        {
            var point = new Point(0, 0);
            point.X = e.Location.X - (e.Location.X % tileWidth);
            point.Y = e.Location.Y - (e.Location.Y % tileHeight);
            Point tilePosition = Point.Empty;
            tilePosition.X = point.X / 50;
            tilePosition.Y = point.Y / 50;
            var propInfo = new PropertyInfo(
                tileNames[tilePosition.X, tilePosition.Y], allTiles.Keys.ToList());

            propInfo.userSelect += (string _tileName) => 
            {
                if (allTiles.ContainsKey(_tileName))
                {
                    DrawTile(allTiles[_tileName], _tileName, e);
                }
                else
                {
                    string validTileName = "";
                    List<string> validTileNames = allTiles.Keys.ToList();
                    var rand = new Random();
                    do
                    {
                        validTileName = validTileNames[
                            rand.Next(0, validTileNames.Count())];
                    }
                    while (validTileName == _tileName);

                    MessageBox.Show
                    ($"Ungültiger Name: {_tileName}\n" +
                    $"Bitte einen gültigen Namen eingeben!\n" +
                    $"Zum Beispiel: {validTileName}.",
                    "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            propertyGrid.SelectedObject = propInfo;
        }

        // Grid anzeigen
        void DrawGrid()
        {
            if (TilePictureBox.Image == null) return;
            Image image = TilePictureBox.Image;
            var drawGrid = Graphics.FromImage(image);
            var pen = new Pen(Color.LightGray);

            for (int x = -1; x < image.Width; x += tileWidth)
            {
                drawGrid.DrawRectangle(pen, x, 0, 1, image.Height);
            }
            for (int y = -1; y < image.Height; y += tileHeight)
            {
                drawGrid.DrawRectangle(pen, 0, y, image.Width, 1);
            }
            TilePictureBox.Image = image;
        }

        // Ausgewähltes Tile ändern
        void ChangeTile(Image _image, string _name)
        {
            activeTile = _image;
            activeTileName = _name;
        }

        //-------------------- Speicher Methoden -------------------------------
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editorData = new EditorData();
            editorData.map = map;
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(saveFileDialog1.FileName))
                {
                    if (MessageBox.Show("Diese Datei gibt es schon", "Datei vorhanden",
                        MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        SaveMap(editorData, (
                            Path.GetFileName(
                                saveFileDialog1.FileName)).Replace(".map", ""));
                    }
                }
                SaveMap(editorData, (
                    Path.GetFileName(
                        saveFileDialog1.FileName)).Replace(".map", ""));
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var editorData = new EditorData();
            editorData.map = map;
            if(savePath == null)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    SaveMap(editorData, (
                        Path.GetFileName(
                            saveFileDialog1.FileName)).Replace(".map", ""));
                }
            }
            else
            {
                if (File.Exists(saveFileDialog1.FileName))
                {
                    if (MessageBox.Show(
                        "Eine Datei mit diesem Namen ist schon vorhanden",
                        "Datei vorhanden",
                        MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        SaveMap(editorData, (
                            Path.GetFileName(
                                saveFileDialog1.FileName)).Replace(".map", ""));
                        return;
                    }
                }
                SaveMap(editorData, (
                    Path.GetFileName(
                        saveFileDialog1.FileName)).Replace(".map", ""));
            }
        }

        private void SaveMap(EditorData _map, string _fileName)
        {
            string path = $"{Application.StartupPath}\\Maps";

            if (!Directory.Exists($"{ Application.StartupPath}\\Maps"))
            {
                Directory.CreateDirectory(path);
            }

            path += $"\\{_fileName}.map";

            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(fs, _map);
            }
        }
        //----------------------------------------------------------------------

        //-------------------------- Lade Methoden -----------------------------
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if(openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    LoadMap(openFileDialog1.FileName);
                }
            }
            catch
            {
                MessageBox.Show(
                    "Die Datei konnte nicht geladen werden", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMap(string _path)
        {
            if (!File.Exists(_path))
            {
                return;
            }

            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(_path, FileMode.Open))
            {
                TilePictureBox.Image = ((EditorData)formatter.Deserialize(fs)).map;
            }
        }
        //----------------------------------------------------------------------

        //----------------------- Neue Map laden -------------------------------
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMap();
        }
        private void NewMap()
        {
            var bitmap = new Bitmap(tileWidth * 15, tileHeight * 15);
            var graphics = Graphics.FromImage(bitmap);
            TilePictureBox.Image = bitmap;
            map = bitmap;
            DrawGrid();
        }
        //----------------------------------------------------------------------

        // Datenklasse
        EditorData GetData()
        {
            return new EditorData() { map = this.map };
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Invalidate(true);
            this.Update();
        }

        // Fenster Einstellungen speichern
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var wd = new WindowData()
            {
                WindowLocation = Location,
                WindowState = this.WindowState
                
            };
            if(WindowState != FormWindowState.Maximized)
            {
                wd.WindowSize = Size;
            }
            if(WindowState == FormWindowState.Minimized)
            {
                wd.WindowLocation = new Point(50, 50);
                wd.WindowSize = new Size(1120, 900);
                wd.WindowState = FormWindowState.Normal;
            }
            wd.Save();
        }

        //---------------- Modus ändern ----------------------------------------
        private void zeichnenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = MODE.DRAW;
            toolStripStatusLabel1.Text = "Modus: Zeichnen";
        }

        private void editierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = MODE.EDIT;
            toolStripStatusLabel1.Text = "Modus: Editieren";
        }
        //----------------------------------------------------------------------

        // Hilfe anzeigen
        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            statusStrip1.BackColor = Color.LightGray;
            if(mode == MODE.DRAW)
            {
                toolStripStatusLabel1.Text = "Modus: Zeichnen";
            }
            else
            {
                toolStripStatusLabel1.Text = "Modus: Editieren";
            }
        }
    }
}
