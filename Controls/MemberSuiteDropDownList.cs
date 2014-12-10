using System.Web;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.Controls
{
    /// <summary>
    /// Extended drop down list designed to store the view state of the list items
    /// </summary>
    public class MemberSuiteDropDownList : DropDownList
    {
        protected override object SaveViewState()
        {
            // Create an object array with one element for the CheckBoxList's
            // ViewState contents, and one element for each ListItem in skmCheckBoxList
            var state = new object[Items.Count + 1];

            object baseState = base.SaveViewState();
            state[0] = baseState;

            // Now, see if we even need to save the view state
            bool itemHasAttributes = false;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Attributes.Count > 0)
                {
                    itemHasAttributes = true;

                    // Create an array of the item's Attribute's keys and values
                    var attribKV = new object[Items[i].Attributes.Count*2];
                    int k = 0;
                    foreach (string key in Items[i].Attributes.Keys)
                    {
                        attribKV[k++] = key;
                        attribKV[k++] = Items[i].Attributes[key];
                    }

                    state[i + 1] = attribKV;
                }
            }

            // return either baseState or state, depending on whether or not
            // any ListItems had attributes
            if (itemHasAttributes)
                return state;
            else
                return baseState;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null) return;

            // see if savedState is an object or object array
            if (savedState is object[])
            {
                // we have an array of items with attributes
                var state = (object[]) savedState;
                base.LoadViewState(state[0]); // load the base state

                for (int i = 1; i < state.Length; i++)
                {
                    if (state[i] != null)
                    {
                        // Load back in the attributes
                        var attribKV = (object[]) state[i];
                        for (int k = 0; k < attribKV.Length; k += 2)
                            Items[i - 1].Attributes.Add(attribKV[k].ToString(),
                                                        attribKV[k + 1].ToString());
                    }
                }
            }
            else
                // we have just the base state
                base.LoadViewState(savedState);
        }

          public void AddItemGroup(string groupTitle)
        {
            this.Items.Add(new ListItem(groupTitle, "$$OPTGROUP$$OPTGROUP$$"));
        }

          protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
          {

              if (this.Items.Count > 0)
              {
                  bool selected = false;
                  bool optGroupStarted = false;
                  for (int i = 0; i < this.Items.Count; i++)
                  {
                      ListItem item = this.Items[i];
                      if (item.Enabled)
                      {
                          if (item.Value == "$$OPTGROUP$$OPTGROUP$$")
                          {
                              if (optGroupStarted) writer.WriteEndTag("optgroup");
                              writer.WriteBeginTag("optgroup");
                              writer.WriteAttribute("label", item.Text);
                              writer.Write('>');
                              writer.WriteLine();
                              optGroupStarted = true;
                          }
                          else
                          {
                              writer.WriteBeginTag("option");
                              if (item.Selected)
                              {
                                  if (selected)
                                  {
                                      this.VerifyMultiSelect();
                                  }
                                  selected = true;
                                  writer.WriteAttribute("selected", "selected");
                              }
                              writer.WriteAttribute("value", item.Value, true);
                              if (item.Attributes.Count > 0)
                              {
                                  item.Attributes.Render(writer);
                              }
                              if (this.Page != null)
                              {
                                  this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, item.Value);
                              }
                              writer.Write('>');
                              HttpUtility.HtmlEncode(item.Text, writer);
                              writer.WriteEndTag("option");
                              writer.WriteLine();
                          }
                      }
                  }
                  if (optGroupStarted)
                  {
                      writer.WriteEndTag("optgroup");
                  }
              }
          }

    }
}