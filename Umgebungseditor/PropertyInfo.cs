using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Umgebungseditor
{
    public class PropertyInfo
    {
        public PropertyInfo(string _tileName, List<string> _allTiles)
        {
            tile = _tileName;
            allTiles = _allTiles.ToArray();
        }
        public event Action<string> userSelect = s => { };

        private string tile;
        public string Tile
        {
            get { return tile; }
            set
            {
                tile = value;
                userSelect.Invoke(tile);
            }
        }

        public string[] allTiles { get; private set; } = null;
    }
}
