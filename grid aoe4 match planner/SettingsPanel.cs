using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class SettingsPanel : BasePopupPanel
    {
        public BuildingSettingsPopup settingsPanelContent;
        public PlacedBuilding parent;

        public SettingsPanel(PlacedBuilding parentBuilding, string titleText) : base(titleText)
        {
            parent = parentBuilding;
            Create();
        }

        public void Create()
        {
            settingsPanelContent = new BuildingSettingsPopup(parent.myForm, parent);
        }

        public override void ShowPanel()
        {
            base.ShowPanel(); // Show base components first
            Create();
            parent.PopupButtonManager.Hide();
            ContentPanel.Controls.Add(settingsPanelContent);
            UpdatePosition(parent.PlacedPictureBox.Location);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of resources specific to QueuePanel here
                if (settingsPanelContent != null)
                {
                    parent.myForm.drawingPanel.Controls.Remove(settingsPanelContent);
                    settingsPanelContent.Dispose();
                    settingsPanelContent = null;
                }
            }
            base.Dispose(disposing);

            MyForm.drawingPanel.Invalidate();
        }
    }
}
