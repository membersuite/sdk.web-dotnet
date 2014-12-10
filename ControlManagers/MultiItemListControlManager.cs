using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class MultiItemListControlManager<T> : ListControlManager<T> where T : ListControl, new()
    {
        public override void DataUnbind()
        {
            base.DataUnbind();

            var items = new List<string>();
            foreach (ListItem item in PrimaryControl.Items)
                if (item.Selected)
                    items.Add(item.Value);

            Host.SetModelValue(ControlMetadata, items);
        }
    }
}