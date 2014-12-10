using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.ControlManagers;
 

namespace MemberSuite.SDK.Web
{
    public class ControlHost  
    {

        //protected Dictionary<ControlMetadata, ControlManager> _controlManagerDictionary = new Dictionary<ControlMetadata, ControlManager>();

        ///// <summary>
        ///// Renders the control from specification.
        ///// </summary>
        ///// <param name="control">The control.</param>
        ///// <returns></returns>
        ///// <remarks>This baby is the heart of the UI!</remarks>
        //protected ControlManager getControlManagerFor(ControlMetadata control)
        //{
        //    ControlManager mngr;

        //    if (_controlManagerDictionary.TryGetValue(control, out mngr))
        //        return mngr;

        //    FieldDisplayType? controlType;

        //    if (control.DisplayType != null)
        //        controlType = control.DisplayType;  // goo to go
        //    else
        //    {

        //        // first, we have to locate the appropriate ControlManager for this control
        //        var fMeta = GetBoundFieldFor(control);

        //        if (fMeta == null)
        //            return null; // can't do anything

        //        controlType = fMeta.DisplayType;
        //    }
        //    mngr = ControlManagerResolver.Resolve(controlType.Value);

        //    if (mngr == null)
        //        throw new ApplicationException( string.Format( "Unable to locate control manager for display type '{0}'",
        //            controlType.Value) );

        //    mngr.Initialize(this, control);
        //    _controlManagerDictionary[control] = mngr;

        //    return mngr;

        //}
        //private Dictionary<ControlMetadata, FieldMetadata> _fieldMetadataDictionary = new Dictionary<ControlMetadata, FieldMetadata>();

        ///// <summary>
        ///// Gets the field metadata for the field that is bound
        ///// to this control, if possible.
        ///// </summary>
        ///// <returns></returns>
        //public FieldMetadata GetBoundFieldFor(ControlMetadata controlSpec)
        //{
        //    FieldMetadata fm;

        //    if (_fieldMetadataDictionary.TryGetValue(controlSpec, out fm))
        //        return fm;



        //    MemberSuiteObject mso = ResolveDataSource(controlSpec) as MemberSuiteObject;

        //    if (mso == null || mso.ClassType == null) // it's not an object
        //        return null;

        //    var meta = Manifest.Describer.Describe(mso.ClassType);

        //    fm = meta.Fields.Find(f1 => f1.Name == controlSpec.DataSourceExpression);

        //    _fieldMetadataDictionary[controlSpec] = fm; // cache it
        //    return fm;
        //}

        ///// <summary>
        ///// Resolves the specified spec, pulling it from the model
        ///// </summary>
        ///// <param name="controlSpec">The control spec.</param>
        ///// <returns></returns>
        //public object ResolveDataSource(ControlMetadata controlSpec)
        //{
        //    // ok - are we bound to a membersuite object?
        //    if (Manifest == null || Manifest.RawViewModel == null)
        //        return null; // nothing to do

        //    if (controlSpec.DataSource == null && controlSpec.DataSourceExpression != null)
        //        controlSpec.DataSource = "TargetObject";

        //    if (controlSpec.DataSource == null)
        //        return null;

        //    var model = Manifest.RawViewModel;
        //    if (!model.ContainsKey(controlSpec.DataSource))
        //        return null;

        //    return model[controlSpec.DataSource];
        //}

        ///// <summary>
        ///// Resolves the specified spec, pulling it from the model
        ///// </summary>
        ///// <param name="controlSpec">The control spec.</param>
        ///// <returns></returns>
        //public object ResolveAcceptableValuesDataSource(ControlMetadata controlSpec)
        //{
        //    // ok - are we bound to a membersuite object?
        //    if (Manifest == null || Manifest.RawViewModel == null)
        //        return null; // nothing to do

        //    if (controlSpec.AcceptableValuesDataSource == null)
        //        return null;

        //    var model = Manifest.RawViewModel;
        //    if (!model.ContainsKey(controlSpec.AcceptableValuesDataSource))
        //        return null;

        //    return model[controlSpec.AcceptableValuesDataSource];
        //}

        //public object Resolve(ControlMetadata controlSpec)
        //{
        //    if (controlSpec.DataSource == null &&
        //        controlSpec.DataSourceExpression == null)
        //        return null; // always - important

        //    var dataSource = ResolveDataSource(controlSpec);

        //    if (dataSource == null || controlSpec.DataSourceExpression == null)
        //        return dataSource; // just give it back

        //    // are we dealing with a membersuite object?
        //    MemberSuiteObject mso = dataSource as MemberSuiteObject;
        //    if (mso != null)
        //        return mso.ResolveExpression(controlSpec.DataSourceExpression);

        //    // a data table?
        //    DataRowView drv = dataSource as DataRowView;
        //    if (drv != null)
        //        if (drv.Row.Table.Columns.Contains(controlSpec.DataSourceExpression))
        //            return drv[controlSpec.DataSourceExpression];
        //        else
        //            return null;

        //    DataRow dr = dataSource as DataRow;
        //    if (dr != null)
        //        if (dr.Table.Columns.Contains(controlSpec.DataSourceExpression))
        //            return dr[controlSpec.DataSourceExpression];
        //        else
        //            return null;

        //    // finally, just try the old way

        //    try
        //    {
        //        return Expression.Parse(controlSpec.DataSourceExpression).GetValue(dataSource);
        //    }
        //    catch
        //    {
        //        return null; // could not parse
        //    }


        //}

        //public object ResolveAcceptableValues(ControlMetadata controlSpec)
        //{
        //    var dataSource = ResolveAcceptableValuesDataSource(controlSpec);

        //    if (dataSource == null || controlSpec.AcceptableValuesDataSource == null)
        //        return dataSource;  // just give it back

        //    MemberSuiteObject mso = dataSource as MemberSuiteObject;
        //    if (mso == null)
        //        return dataSource;

        //    return mso.ResolveExpression(controlSpec.AcceptableValuesDataSourceExpression);
        //}

        //#region IControlHost Members


        //public string ResolveResource(string resourceName, bool returnNullIfNothingFound)
        //{
        //    throw new NotImplementedException();
        //}

        //public MemberSuite.SDK.Concierge.IConciergeAPIService GetServiceAPIProxy()
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetModelValue(ControlMetadata ControlMetadata, object valueToSet)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetModelValue(ControlMetadata ControlMetadata, object valueToSet, bool onlyIfMemberSuiteObject)
        //{
        //    throw new NotImplementedException();
        //}

        //public MemberSuite.SDK.Manifests.Searching.SearchManifest DescribeSearch(string searchType, string searchContext)
        //{
        //    throw new NotImplementedException();
        //}

        //public TimeZoneInfo GetCurrentTimeZone()
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion
    }
}
