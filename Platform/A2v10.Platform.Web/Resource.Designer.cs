﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace A2v10.Platform.Web {
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
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("A2v10.Platform.Web.Resource", typeof(Resource).Assembly);
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
        ///   Looks up a localized string similar to // Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.
        ///
        ////*20240118-8226*/
        ///
        ///(function () {
        ///
        ///	const Shell = component(&apos;std:shellController&apos;);
        ///
        ///	const AppHeader = component(&apos;std:appHeader&apos;);
        ///	const MainView = component(&apos;std:mainView&apos;);
        ///
        ///	const menu = $(Menu);
        ///	const companies = $(Companies);
        ///	const initialPeriod = $(Period);
        ///
        ///	const sp = menu.SysParams || {};
        ///
        ///	const shell = new Shell({
        ///		el: &apos;#shell&apos;,
        ///		components: {
        ///			&apos;a2-app-header&apos;: AppHeader,
        ///			&apos;a2-main-view&apos;: MainView
        ///		 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string shell {
            get {
                return ResourceManager.GetString("shell", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.
        ///
        ////*20240118-8226*/
        ///
        ///(function () {
        ///
        ///	const Shell = component(&apos;std:shellController&apos;);
        ///	const locale = window.$$locale;
        ///
        ///	const menu = $(Menu);
        ///	const menucomp = component(&apos;std:navmenu&apos;);
        ///
        ///	const shell = new Shell({
        ///		el: &apos;#shell&apos;,
        ///		data: {
        ///			title: menu.SysParams ? menu.SysParams.AppTitle : &apos;&apos;,
        ///			subtitle: locale.$Admin,
        ///			version: &apos;$(AppVersion)&apos;,
        ///			menu: menu.Menu[0].Menu
        ///		},
        ///		methods: {
        ///			root() {
        ///				let opts = [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string shellAdmin {
            get {
                return ResourceManager.GetString("shellAdmin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.
        ///
        ////*20240118-8226*/
        ///
        ///(function () {
        ///
        ///	const menu = $(Menu);
        ///
        ///	const Shell = component(&apos;std:shellPlain&apos;)
        ///
        ///	const sp = menu.SysParams || {};
        ///
        ///	const elem = new Shell({
        ///		el: &apos;#shell&apos;,
        ///		data: {
        ///			version: &apos;$(AppVersion)&apos;,
        ///			menu: menu.Menu ? menu.Menu[0].Menu : null,
        ///			title: sp.AppTitle || &apos;&apos;,
        ///			subtitle: sp.AppSubTitle || &apos;&apos;,
        ///			userState: menu.UserState,
        ///			isDebug: $(Debug),
        ///			appData: $(AppData),
        ///		}
        ///	});
        ///
        ///	wind [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string shellPlain {
            get {
                return ResourceManager.GetString("shellPlain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.
        ///
        ////*20240118-8226*/
        ///
        ///(function () {
        ///
        ///	const Shell = component(&apos;std:shellSinglePage&apos;)
        ///
        ///	const sp = menu.SysParams || {};
        ///
        ///	const elem = new Shell({
        ///		el: &apos;#shell&apos;,
        ///		data: {
        ///			version: &apos;$(AppVersion)&apos;,
        ///			title: sp.AppTitle || &apos;&apos;,
        ///			subtitle: sp.AppSubTitle || &apos;&apos;,
        ///			userState: menu.UserState,
        ///			isDebug: $(Debug),
        ///			appData: $(AppData),
        ///		}
        ///	});
        ///
        ///	window.$$rootUrl = &apos;&apos;;
        ///	window.$$debug = $(Debug);
        ///})();.
        /// </summary>
        internal static string shellSinglePage {
            get {
                return ResourceManager.GetString("shellSinglePage", resourceCulture);
            }
        }
    }
}
