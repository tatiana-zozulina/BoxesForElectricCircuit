using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;

namespace BoxesForElectricCircuit
{
    public partial class UserForm : System.Windows.Forms.Form
    {
        public List<ElementId> SelectedCircuits = new List<ElementId>();
        public ElementId SelectedConduitType = null;

        private List<TreeNode> _checkedNodes = new List<TreeNode>();
        private Dictionary<string, ElementId> _conduitTypes = new Dictionary<string, ElementId>();
        public UserForm(Document document ,Dictionary<ElementId,
                List<ElementId>>  systemsByEquipmentIds,
                Dictionary<string, ElementId> conduitTypes)
        {
            InitializeComponent();

            equipmentTreeView.CheckBoxes = true;
            _conduitTypes = conduitTypes;
            foreach (var keyValue in systemsByEquipmentIds)
            {
                
                var parentNode = new TreeNode(document.GetElement(keyValue.Key).Name);
                parentNode.Tag = keyValue.Key;
                foreach (var id in keyValue.Value)
                {
                    var node = new TreeNode(document.GetElement(id).Name);
                    node.Tag = id;
                    parentNode.Nodes.Add(node);
                }
                equipmentTreeView.Nodes.Add(parentNode);
            }

            foreach (var keyValue in conduitTypes)
            {
                conduitTypesComboBox.Items.Add(keyValue.Key);
            }


        }

        private void conduitTypesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            conduitTypesErrorLabel.Visible = false;

        }

        void equipmentTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            equipmentErrorLabel.Visible = false;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            _checkedNodes = GetCheckedNodes();
            
            if (_checkedNodes.Count == 0)
                equipmentErrorLabel.Visible = true;
            if (conduitTypesComboBox.SelectedItem == null)
                conduitTypesErrorLabel.Visible = true;

            if (conduitTypesErrorLabel.Visible || equipmentErrorLabel.Visible)
                return;
            
            foreach (var checkedNode in _checkedNodes)
            {
                SelectedCircuits.Add((ElementId)checkedNode.Tag);
            }

            var selectedConduitType = _conduitTypes.ElementAt(conduitTypesComboBox.SelectedIndex).Value;
            SelectedConduitType = selectedConduitType;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private List<TreeNode> GetCheckedNodes()
        {
            var result = new List<TreeNode>();
            for(var i=0; i < equipmentTreeView.Nodes.Count; i++)
            {
                foreach (TreeNode childNode in equipmentTreeView.Nodes[i].Nodes)
                {
                    if (childNode.Checked)
                        result.Add(childNode);
                }
            }
            return result;
        }
    }
}
