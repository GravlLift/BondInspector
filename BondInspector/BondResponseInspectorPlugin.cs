#region using
using BondReader;
using Fiddler;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
#endregion using

namespace BondInspector
{
    public class BondResponseInspector : BondInspectorBase, IResponseInspector2
    {
        public HTTPResponseHeaders headers { get; set; }
        public override byte[] body
        {
            get { return _body; }
            set { UpdateBody(value); }
        }
    }
}
