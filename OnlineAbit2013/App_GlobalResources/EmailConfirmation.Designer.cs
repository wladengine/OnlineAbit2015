//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option or rebuild the Visual Studio project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Web.Application.StronglyTypedResourceProxyBuilder", "11.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class EmailConfirmation {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EmailConfirmation() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.EmailConfirmation", global::System.Reflection.Assembly.Load("App_GlobalResources"));
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
        ///   Looks up a localized string similar to На ваш адрес электронной почты.
        /// </summary>
        internal static string FirstMail1 {
            get {
                return ResourceManager.GetString("FirstMail1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to было выслано письмо с инструкциями для окончания регистрации..
        /// </summary>
        internal static string FirstMail2 {
            get {
                return ResourceManager.GetString("FirstMail2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Подтверждение EMail адреса.
        /// </summary>
        internal static string Header {
            get {
                return ResourceManager.GetString("Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Вы успешно прошли регистрацию на сайте..
        /// </summary>
        internal static string Success {
            get {
                return ResourceManager.GetString("Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;a href=&quot;../../Abiturient/Main&quot;&gt;Войдите на сайт&lt;/a&gt; для начала работы.
        /// </summary>
        internal static string SuccessLink {
            get {
                return ResourceManager.GetString("SuccessLink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Неверный EMail адрес.
        /// </summary>
        internal static string WrongEmail {
            get {
                return ResourceManager.GetString("WrongEmail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to При подтверждении EMail адреса произошла ошибка. Возможно, вы не до конца скопировали ссылку из письма.
        /// </summary>
        internal static string WrongTicket {
            get {
                return ResourceManager.GetString("WrongTicket", resourceCulture);
            }
        }
    }
}
