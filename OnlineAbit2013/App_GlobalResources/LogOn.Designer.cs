//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
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
    internal class LogOn {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LogOn() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.LogOn", global::System.Reflection.Assembly.Load("App_GlobalResources"));
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
        ///   Looks up a localized string similar to Войти.
        /// </summary>
        internal static string btnLogOn {
            get {
                return ResourceManager.GetString("btnLogOn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Email.
        /// </summary>
        internal static string Email {
            get {
                return ResourceManager.GetString("Email", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Введите Email пользователя.
        /// </summary>
        internal static string EmailValidationMessage {
            get {
                return ResourceManager.GetString("EmailValidationMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Забыли пароль?.
        /// </summary>
        internal static string ForgotPassword {
            get {
                return ResourceManager.GetString("ForgotPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Информация об учётной записи.
        /// </summary>
        internal static string HeaderAccountInformation {
            get {
                return ResourceManager.GetString("HeaderAccountInformation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Вход на сайт.
        /// </summary>
        internal static string HeaderLogOn {
            get {
                return ResourceManager.GetString("HeaderLogOn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Данная учётная запись ещё не активирована.
        /// </summary>
        internal static string NotApprovedError {
            get {
                return ResourceManager.GetString("NotApprovedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пароль.
        /// </summary>
        internal static string Password {
            get {
                return ResourceManager.GetString("Password", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Введите пароль.
        /// </summary>
        internal static string PasswordValidationMessage {
            get {
                return ResourceManager.GetString("PasswordValidationMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Зарегистрироваться.
        /// </summary>
        internal static string Register {
            get {
                return ResourceManager.GetString("Register", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Запомнить меня.
        /// </summary>
        internal static string RememberMe {
            get {
                return ResourceManager.GetString("RememberMe", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to При авторизации возникли следующие ошибки:.
        /// </summary>
        internal static string ValidationSummaryHeader {
            get {
                return ResourceManager.GetString("ValidationSummaryHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Данная учётная запись ещё не активирована. Пожалуйста, проверьте указанный Вами e-mail.
        /// </summary>
        internal static string ValidationSummaryNotApproved {
            get {
                return ResourceManager.GetString("ValidationSummaryNotApproved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Неверное имя пользователя или пароль.
        /// </summary>
        internal static string ValidationSummaryWrongUsernamePassword {
            get {
                return ResourceManager.GetString("ValidationSummaryWrongUsernamePassword", resourceCulture);
            }
        }
    }
}
