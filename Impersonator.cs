using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace Net6ImpersonationDemo;

/// <summary>
/// 模仿者：切換登入身份。
/// </summary>
/// <remarks>
/// 參考文件1：[WindowsIdentity.RunImpersonated 方法](https://docs.microsoft.com/zh-tw/dotnet/api/system.security.principal.windowsidentity.runimpersonated?view=net-6.0)
/// 參考文件2：[切換身分Impersonation](https://ithelp.ithome.com.tw/articles/10252658)
/// </remarks>
/// <example>
/// using var imuser = new Impersonator("userName", "domain", "password");
/// imuser.RunImpersonated(user =>
/// {
///   // User action  
///   Console.WriteLine("During impersonation: " + user.Name);
/// });
/// </example>
internal sealed class Impersonator : IDisposable
{
  [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
    int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

  //This parameter causes LogonUser to create a primary token.   
  const int LOGON32_LOGON_INTERACTIVE = 2;
  const int LOGON32_PROVIDER_DEFAULT = 0;
  const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

  SafeAccessTokenHandle _safeAccessTokenHandle = null;

  /// <summary>
  /// 切換身份
  /// </summary>
  public Impersonator(string userName, string domainName, string mima)
  {
    // Call LogonUser to obtain a handle to an access token.   
    if (!LogonUser(userName, domainName, mima,
        LOGON32_LOGON_NEW_CREDENTIALS,
        LOGON32_PROVIDER_DEFAULT,
        out _safeAccessTokenHandle))
    {
      int ret = Marshal.GetLastWin32Error();
      throw new System.ComponentModel.Win32Exception(ret);
    }
  }

  /// <summary>
  /// 執行Action
  /// </summary>
  public void RunImpersonated(Action<WindowsIdentity> action) =>
    WindowsIdentity.RunImpersonated(_safeAccessTokenHandle, () => action.Invoke(WindowsIdentity.GetCurrent(true)));

  void IDisposable.Dispose()
  {
    _safeAccessTokenHandle?.Dispose();
  }

  /// <summary>
  /// 或直接切換身份並執行Action。
  /// </summary>
  /// <example>
  /// Impersonator.RunImpersonated("userName", "domain", "password", id =>
  /// {
  ///   // User action  
  ///   Console.WriteLine("During impersonation: " + user.Name);
  /// });
  /// </example>
  public static void RunImpersonated(string userName, string domainName, string mima, Action<WindowsIdentity> action)
  {
    using var imuser = new Impersonator(userName, domainName, mima);
    imuser.RunImpersonated(action);
  }

}
