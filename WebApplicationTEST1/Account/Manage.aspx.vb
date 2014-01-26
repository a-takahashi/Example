Imports System.Collections.Generic

Imports Microsoft.AspNet.Membership.OpenAuth

Public Class Manage
    Inherits System.Web.UI.Page

    Private successMessageTextValue As String
    Protected Property SuccessMessageText As String
        Get
            Return successMessageTextValue
        End Get
        Private Set(value As String)
            successMessageTextValue = value
        End Set
    End Property

    Private canRemoveExternalLoginsValue As Boolean
    Protected Property CanRemoveExternalLogins As Boolean
        Get
            Return canRemoveExternalLoginsValue
        End Get
        Set(value As Boolean)
            canRemoveExternalLoginsValue = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' レンダリングするセクションを判別します
            Dim hasLocalPassword = OpenAuth.HasLocalPassword(User.Identity.Name)
            setPassword.Visible = Not hasLocalPassword
            changePassword.Visible = hasLocalPassword

            CanRemoveExternalLogins = hasLocalPassword

            ' 成功メッセージをレンダリングします
            Dim message = Request.QueryString("m")
            If Not message Is Nothing Then
                ' アクションからクエリ文字列を削除します
                Form.Action = ResolveUrl("~/Account/Manage")

                Select Case message
                    Case "ChangePwdSuccess"
                        SuccessMessageText = "パスワードが変更されました。"
                    Case "SetPwdSuccess"
                        SuccessMessageText = "パスワードが設定されました。"
                    Case "RemoveLoginSuccess"
                        SuccessMessageText = "外部ログインが削除されました。"
                    Case Else
                        SuccessMessageText = String.Empty
                End Select

                successMessage.Visible = Not String.IsNullOrEmpty(SuccessMessageText)
            End If
        End If

        
    End Sub

    Protected Sub setPassword_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        If IsValid Then
            Dim result As SetPasswordResult = OpenAuth.AddLocalPassword(User.Identity.Name, password.Text)
            If result.IsSuccessful Then
                Response.Redirect("~/Account/Manage?m=SetPwdSuccess")
            Else
                
                ModelState.AddModelError("NewPassword", result.ErrorMessage)
                
            End If
        End If
    End Sub

    
    Public Function GetExternalLogins() As IEnumerable(Of OpenAuthAccountData)
        Dim accounts = OpenAuth.GetAccountsForUser(User.Identity.Name)
        CanRemoveExternalLogins = CanRemoveExternalLogins OrElse accounts.Count() > 1
        Return accounts
    End Function

    Public Sub RemoveExternalLogin(ByVal providerName As String, ByVal providerUserId As String)
        Dim m = If(OpenAuth.DeleteAccount(User.Identity.Name, providerName, providerUserId), "?m=RemoveLoginSuccess", String.Empty)
        Response.Redirect("~/Account/Manage" & m)
    End Sub
    

    Protected Shared Function ConvertToDisplayDateTime(ByVal utcDateTime As Nullable(Of DateTime)) As String
        ' このメソッドを変更すると、UTC の日付と時刻を必要な表示のオフセットと形式に
        ' 変換できます。ここでは、現在のスレッド カルチャを使用して、短い日付および長い時間の文字列として、
        ' それをサーバーのタイムゾーンと書式設定に変換しています。
        Return If(utcDateTime.HasValue, utcDateTime.Value.ToLocalTime().ToString("G"), "[しない]")
    End Function
End Class