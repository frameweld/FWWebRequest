# FWWebRequest
WebRequest Class for sending GET/POST requests. POST requests can be sent with parameters and files in a single request.

<h2>Example</h2>

Dim strSyncWordsUri As String = "https://api.syncwords.com/[Add resource path here]"
        Dim strMethod As String = "POST"       
        ' Instantiate the Frameweld web request class.
        Dim request As New FWWebRequest()

        request.SetMethod(strMethod)
        request.SetUri(strSyncWordsUri)

        Select Case strMethod
            Case "POST"
                ' Add POST parameters below. Refer to FWWebRequest.vb for more information on adding POST parameters.
                request.AddPostData("[name]", "[Value goes here]")
                
                ' Add FILEs below. Refer to FWWebRequest.vb for more information on adding FILEs.
                request.AddFile("transcript", "C:\Users\omardellis\Desktop\mcgrawhill.txt")
        End Select

        request.Send()

        MessageBox.Show(request.GetResponse())
