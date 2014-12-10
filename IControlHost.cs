using System;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Manifests.Searching;
using MemberSuite.SDK.Types;

namespace MemberSuite.SDK.Web
{
    /// <summary>
    /// Provides a host that controls and control managers can use for resolution
    /// </summary>
    public interface IControlHost
    {
        /// <summary>
        /// Applies a control metadata against the view metadata to find a value
        /// </summary>
        /// <param name="cMeta">The c meta.</param>
        /// <returns></returns>
        object Resolve(ControlMetadata cMeta);

        /// <summary>
        /// Attempts to determine a resource value
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="returnNullIfNothingFound">if set to <c>true</c> [return null if nothing found].</param>
        /// <returns></returns>
        string ResolveResource(string resourceName, bool returnNullIfNothingFound);

        string ResolveComplexExpression(string complexExpression);

        /// <summary>
        /// Gets the field metadata associated with the specified control metadata
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        FieldMetadata GetBoundFieldFor(ControlMetadata meta);

        /// <summary>
        /// Gets the service API proxy.
        /// </summary>
        /// <returns></returns>
        IConciergeAPIService GetServiceAPIProxy();

        
        /// <summary>
        /// Sets the value of the control in the model
        /// </summary>
        /// <param name="ControlMetadata">The control metadata.</param>
        /// <param name="valueToSet">The value to set.</param>
        void SetModelValue(ControlMetadata ControlMetadata, object valueToSet);

        /// <summary>
        /// Sets the value of the control in the model
        /// </summary>
        /// <param name="ControlMetadata">The control metadata.</param>
        /// <param name="valueToSet">The value to set.</param>
        void SetModelValue(ControlMetadata ControlMetadata, object valueToSet, bool onlyIfMemberSuiteObject);


        /// <summary>
        /// Resolves the acceptable values.
        /// </summary>
        /// <param name="ControlMetadata">The control metadata.</param>
        /// <returns></returns>
        object ResolveAcceptableValues(ControlMetadata ControlMetadata);

        /// <summary>
        /// Describes the search.
        /// </summary>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="searchContext">The search context.</param>
        /// <returns></returns>
        SearchManifest DescribeSearch(string searchType, string searchContext);

        /// <summary>
        /// Gets the current time zone.
        /// </summary>
        /// <returns></returns>
        TimeZoneInfo GetCurrentTimeZone();
    }
}