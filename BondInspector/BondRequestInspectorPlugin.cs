using BondReader;
using Fiddler;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace BondInspector
{
    // Request-side inspector: derives from shared base
    public class BondRequestInspector : BondInspectorBase, IRequestInspector2
    {
        public HTTPRequestHeaders headers { get; set; }

        public override byte[] body
        {
            get { return _body; }
            set { UpdateBody(value); }
        }
    }
}
