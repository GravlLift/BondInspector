using BondReader;
using Fiddler;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace BondInspector
{
    // Shared base for Bond inspectors: handles UI host and body processing
    public abstract class BondInspectorBase : Inspector2, IWSMInspector
    {
        protected readonly BondInspectorView bondInspectorView;
        protected readonly ElementHost host = new ElementHost();
        protected byte[] _body;

        // IBaseInspector2 requires a body property; leave abstract for subclasses
        public abstract byte[] body { get; set; }

        protected BondInspectorBase()
        {
            bondInspectorView = new BondInspectorView();
        }

        protected void UpdateBody(byte[] data)
        {
            _body = data;
            if (data != null)
            {
                bondInspectorView.ViewModel.BondText = new BondProcessor(2).ProcessBytes(
                    data,
                    false
                );
            }
            else
            {
                bondInspectorView.ViewModel.Clear();
            }
        }

        public bool bDirty { get { return false; } }

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
            _body = null;
            bondInspectorView.ViewModel.Clear();
        }

        public override int GetOrder()
        {
            return 150;
        }
    }
}
