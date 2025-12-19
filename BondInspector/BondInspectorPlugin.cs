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
    public class BondInspector : Inspector2, IResponseInspector2, IWSMInspector, IRequestInspector2
    {
        private readonly BondInspectorView bondInspectorView;
        private readonly ElementHost host = new ElementHost();
        private byte[] _body;

        public BondInspector()
        {
            bondInspectorView = new BondInspectorView();
        }

        HTTPRequestHeaders IRequestInspector2.headers { get; set; }
        public HTTPResponseHeaders headers { get; set; }
        public byte[] body
        {
            get { return _body; }
            set
            {
                _body = value;

                if (body != null)
                {
                    bondInspectorView.ViewModel.BondText = new BondProcessor(2).ProcessBytes(
                        body,
                        false
                    );
                }
            }
        }

        public bool bDirty
        {
            get { return false; }
        }

        public bool bReadOnly { get; set; }

        public override void AddToTab(TabPage o)
        {
            host.Dock = DockStyle.Fill;
            host.Child = bondInspectorView;
            o.Text = "Bond";
            o.Controls.Add(host);
        }

        public void AssignMessage(WebSocketMessage oWSM)
        {
            bondInspectorView.ViewModel.BondText = new BondProcessor(2).ProcessBytes(
                oWSM.PayloadData,
                false
            );
        }

        public void Clear()
        {
            body = null;
            bondInspectorView.ViewModel.Clear();
        }

        public override int GetOrder()
        {
            return 150;
        }
    }
}
