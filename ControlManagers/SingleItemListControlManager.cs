using System.Web.UI.WebControls;
using MemberSuite.SDK.Manifests.Command;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public abstract class SingleItemListControlManager<T> : ListControlManager<T> where T : ListControl, new()
    {
        public override void DataUnbind()
        {
            base.DataUnbind();


            Host.SetModelValue(ControlMetadata, PrimaryControl.SelectedValue);


            string text = PrimaryControl.SelectedItem != null ? PrimaryControl.SelectedItem.Text : null;

            //and, let's set the text, just in case
            ControlMetadata textMeta = ControlMetadata.Clone();
            textMeta.DataSourceExpression += "_Name__transient";
            Host.SetModelValue(textMeta, text, true);
        }
    }
}