## Synopsis

WebRequest Class for sending GET/POST requests. POST requests can be sent with parameters and files attached within a single request.

## Code Example
```
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
```
## License

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
