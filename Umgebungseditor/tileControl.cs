using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Umgebungseditor
{
    public partial class tileControl : UserControl
    {
        
        public event Action<Image, string> userSelect = (i, s) => { };

        public tileControl()
        {
            InitializeComponent();
        }

        public void ShowControl(Image _image, string _name)
        {
            Label.Text = _name;
            Button.BackgroundImage = _image;
            toolTip.SetToolTip(Button, _name);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            userSelect.Invoke(Button.BackgroundImage, Label.Text);
        }
    }
}
