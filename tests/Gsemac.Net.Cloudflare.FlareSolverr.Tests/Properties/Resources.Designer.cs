﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gsemac.Net.Cloudflare.FlareSolverr.Tests.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gsemac.Net.Cloudflare.FlareSolverr.Tests.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;status&quot;: &quot;ok&quot;,
        ///  &quot;message&quot;: &quot;&quot;,
        ///  &quot;startTimestamp&quot;: 1646868923535,
        ///  &quot;endTimestamp&quot;: 1646868926209,
        ///  &quot;version&quot;: &quot;v2.1.0&quot;,
        ///  &quot;solution&quot;: {
        ///    &quot;url&quot;: &quot;https://example.com/&quot;,
        ///    &quot;status&quot;: 200,
        ///    &quot;headers&quot;: {},
        ///    &quot;response&quot;: &quot;&quot;,
        ///    &quot;cookies&quot;: [
        ///      {
        ///        &quot;name&quot;: &quot;valid_cookie&quot;,
        ///        &quot;value&quot;: &quot;value&quot;,
        ///        &quot;domain&quot;: &quot;.example.com&quot;,
        ///        &quot;path&quot;: &quot;/&quot;,
        ///        &quot;expires&quot;: 1709940925,
        ///        &quot;size&quot;: 47,
        ///        &quot;httpOnly&quot;: false,
        ///        &quot;secure&quot;: false,
        ///        &quot;sess [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ResponseWithCookiesWithEmptyStringForName {
            get {
                return ResourceManager.GetString("ResponseWithCookiesWithEmptyStringForName", resourceCulture);
            }
        }
    }
}