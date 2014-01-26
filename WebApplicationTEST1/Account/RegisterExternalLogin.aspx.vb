Imports System.Web.Security
Imports DotNetOpenAuth.AspNet
Imports Microsoft.AspNet.Membership.OpenAuth

Public Class RegisterExternalLogin
    Inherits System.Web.UI.Page

    Protected Property ProviderName As String
        Get
            Return If(DirectCast(ViewState("ProviderName"), String), String.Empty)
        End Get
        Private Set(value As String)
            ViewState("ProviderName") = value
        End Set
    End Property

    Protected Property ProviderDisplayName As String
        Get
            Return If(DirectCast(ViewState("PropertyProviderDisplayName"), String), String.Empty)
        End Get
        Private Set(value As String)
            ViewState("ProviderDisplayName") = value
        End Set
    End Property

    Protected Property ProviderUserId As String
        Get
            Return If(DirectCast(ViewState("ProviderUserId"), String), String.Empty)
        End Get

        Private Set(value As String)
            ViewState("ProviderUserId") = value
        End Set
    End Property

    Protected Property ProviderUserName As String
        Get
            Return If(DirectCast(ViewState("ProviderUserName"), String), String.Empty)
        End Get

        Private Set(value As String)
            ViewState("ProviderUserName") = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ProcessProviderResult()
        End If
    End Sub

    Protected Sub logIn_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        CreateAndLoginUser()
    End Sub

    Protected Sub cancel_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        RedirectToReturnUrl()
    End Sub

    Private Sub ProcessProviderResult()
        ' 要求の認証プロバイダーからの結果を処理します
        ProviderName = OpenAuth.GetProviderNameFromCurrentRequest()

        If String.IsNullOrEmpty(ProviderName) Then
            Response.Redirect(FormsAuthentication.LoginUrl)
        End If

        ' OpenAuth 検証のリダイレクト URL をバインドします
        Dim redirectUrl As String = "~/Account/RegisterExternalLogin"
        Dim returnUrl As String = Request.QueryString("ReturnUrl")
        If Not String.IsNullOrEmpty(returnUrl) Then
            redirectUrl &= "?ReturnUrl=" & HttpUtility.UrlEncode(returnUrl)
        End If

        ' OpenAuth ペイロードを検証します
        Dim authResult As AuthenticationResult = OpenAuth.VerifyAuthentication(redirectUrl)
        ProviderDisplayName = OpenAuth.GetProviderDisplayName(ProviderName)
        If Not authResult.IsSuccessful Then
            Title = "外部ログインが失敗しました"
            userNameForm.Visible = False
            
            ModelState.AddModelError("Provider", String.Format("{0} での外部ログインが失敗しました。", ProviderDisplayName))
            
            ' このエラーを表示するには、web.config (<system.web><trace enabled="true"/></system.web>) でページ トレースを有効にし、~/Trace.axd にアクセスします
            Trace.Warn("OpenAuth", String.Format("{0} での認証の検証でエラーが発生しました)", ProviderDisplayName), authResult.Error)
            Return
        End If

        ' ユーザーはプロバイダーを使用して正常にログインしました
        ' ユーザーが既にローカルで登録されているかどうかを確認します
        If OpenAuth.Login(authResult.Provider, authResult.ProviderUserId, createPersistentCookie:=False) Then
            RedirectToReturnUrl()
        End If

        ' プロバイダーの詳細情報を ViewState に格納します
        ProviderName = authResult.Provider
        ProviderUserId = authResult.ProviderUserId
        ProviderUserName = authResult.UserName

        ' アクションからクエリ文字列を削除します
        Form.Action = ResolveUrl(redirectUrl)

        If (User.Identity.IsAuthenticated) Then
            ' ユーザーは既に認証されています、戻り先 URL に外部ログインとリダイレクトを追加します
            OpenAuth.AddAccountToExistingUser(ProviderName, ProviderUserId, ProviderUserName, User.Identity.Name)
            RedirectToReturnUrl()
        Else
            ' ユーザーは新規ユーザーです、ユーザーが希望するメンバーシップ名をたずねます
            userName.Text = authResult.UserName
        End If
    End Sub

    Private Sub CreateAndLoginUser()
        If Not IsValid Then
            Return
        End If

        Dim createResult As CreateResult = OpenAuth.CreateUser(ProviderName, ProviderUserId, ProviderUserName, userName.Text)

        If Not createResult.IsSuccessful Then
            
            ModelState.AddModelError("UserName", createResult.ErrorMessage)
            
        Else
            ' 作成および関連付けられたユーザー OK
            If OpenAuth.Login(ProviderName, ProviderUserId, createPersistentCookie:=False) Then
                RedirectToReturnUrl()
            End If
        End If
    End Sub

    Private Sub RedirectToReturnUrl()
        Dim returnUrl As String = Request.QueryString("ReturnUrl")
        If Not String.IsNullOrEmpty(returnUrl) And OpenAuth.IsLocalUrl(returnUrl) Then
            Response.Redirect(returnUrl)
        Else
            Response.Redirect("~/")
        End If
    End Sub
End Class