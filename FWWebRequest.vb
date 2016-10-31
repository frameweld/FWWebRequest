Imports System.Collections.Specialized
Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Security
Imports System.Net.Security

''' <copyright file="FWWebrequest.vb" company="Frameweld">
''' Copyright (c) 2016 All Rights Reserved
''' </copyright>
''' 
''' This program is free software: you can redistribute it and/or modify
''' it under the terms of the GNU General Public License as published by
''' the Free Software Foundation, either version 3 of the License, or
''' (at your option) any later version.
''' This program is distributed in the hope that it will be useful,
''' but WITHOUT ANY WARRANTY; without even the implied warranty of
''' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
''' GNU General Public License for more details.
''' 
''' You should have received a copy of the GNU General Public License
''' along with this program.  If not, see http://www.gnu.org/licenses/.
''' <author>Omar D. Ellis, MISM</author>
''' <date>10/28/2016 10:00 AM </date>
''' <summary>
''' WebRequest Class for sending GET/POST requests. POST requests can be sent with parameters and files attached within a single request.
''' </summary>
Public Class FWWebRequest
    ''' <summary>
    ''' Content disposition value used for sending POST parameters.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const CONTENT_DISPOSITION As String = "Content-Disposition: form-data; name="
    ''' <summary>
    ''' The below varaibles will be used to build POST content body.
    ''' </summary>
    Protected sbContentBody As New StringBuilder
    Protected strBeginBoundary As String
    Protected strContentBoundary As String
    Protected strEndingBoundary As String
    ''' <summary>
    ''' Uri used for request.
    ''' </summary>
    Protected strUri As Uri = Nothing
    ''' <summary>
    ''' Deafault request is POST.
    ''' </summary>
    Protected strMethod As String = "POST"
    ''' <summary>
    ''' Default accept is <c>application/json</c> for Frameweld requests.
    ''' </summary>
    Protected strAccept As String = "application/json"
    ''' <summary>
    ''' The amount of time before a request times out. Default is 20 minutes.
    ''' </summary>
    Protected intTimeout As Integer = (60000 * 20)
    ''' <summary>
    ''' The chunk size for sending files. Default is 4 KB
    ''' </summary>
    Protected intChunkSize As Integer = 4096
    ''' <summary>
    ''' The security protocol being used to send requests. Default is SSL13
    ''' </summary>
    Protected spSecurityProtocol As SecurityProtocolType = SecurityProtocolType.Ssl3
    ''' <summary>
    ''' Used to set extra headers
    ''' </summary>
    Protected nvcHeaders As New NameValueCollection
    ''' <summary>
    ''' Used to store post values for POST request only.
    ''' </summary>
    Protected nvcParams As New NameValueCollection
    ''' <summary>
    ''' Used to store files for POST request only.
    ''' </summary>
    Protected nvcFiles As New NameValueCollection
    ''' <summary>
    ''' Response from remote server.
    ''' </summary>
    Protected strResponse As String = vbNull
    ''' <summary>
    ''' Class constructor with a Uri provided.
    ''' </summary>
    ''' <param name="uri">Uri used for the request.</param>
    Public Sub New(ByVal uri As String)
        Me.Initalize()
        strUri = New Uri(uri)
    End Sub
    ''' <summary>
    ''' Class constructor without Uri.
    ''' </summary>
    Public Sub New()
        Me.Initalize()
    End Sub
    ''' <summary>
    ''' Initialize content boundry. Used durin POST requests only.
    ''' </summary>
    Private Sub Initalize()
        strBeginBoundary = "---------------------" & DateTime.Now.Ticks.ToString("x")
        strContentBoundary = "--" & strBeginBoundary
        strEndingBoundary = strContentBoundary & "--"
    End Sub
    ''' <summary>
    ''' Set Uri to use for request.
    ''' </summary>
    ''' <param name="uri">Uri used for the request.</param>
    Public Sub SetUri(ByVal uri As String)
        strUri = New Uri(uri)
    End Sub
    ''' <summary>
    ''' Set the method to use for request. This class only supports GET and POST request.
    ''' </summary>
    ''' <param name="method">Method to use for request.</param>
    ''' <remarks></remarks>
    Public Sub SetMethod(ByVal method As String)
        If method.ToUpper() <> "GET" And method.ToUpper() <> "POST" Then
            Throw New Exception("Only GET and POST is supported.")
            Return
        End If

        strMethod = method.ToUpper()
    End Sub
    ''' <summary>
    ''' Set the request timeout.
    ''' </summary>
    ''' <param name="timeout">The amount of time to wait before ending a request.</param>
    Public Sub SetTimeout(ByVal timeout As Integer)
        intTimeout = timeout
    End Sub
    ''' <summary>
    ''' The chunk size used when uploading a file. 
    ''' </summary>
    ''' <param name="chunkSize">The chunk size to use.</param>
    Public Sub SetChunkSize(ByVal chunkSize As Integer)
        intChunkSize = chunkSize
    End Sub
    ''' <summary>
    ''' Set security protocol for requests.
    ''' </summary>
    ''' <param name="protocol">Security protocol to use for requests.</param>
    Public Sub SetSecurityProtocol(ByVal protocol As SecurityProtocolType)
        spSecurityProtocol = protocol
    End Sub
    ''' <summary>
    ''' Get response from server.
    ''' </summary>
    ''' <returns>String if there is a response from the remote server.</returns>
    Public Function GetResponse() As String
        Return strResponse
    End Function
    ''' <summary>
    ''' Add header to request.
    ''' </summary>
    ''' <param name="key">Key to used for header.</param>
    ''' <param name="value">Value of the key to use for header.</param>
    Public Sub AddHeader(ByVal key As String, ByVal value As String)
        nvcHeaders.Add(key, value)
    End Sub
    ''' <summary>
    ''' Add single POST parameter.
    ''' </summary>
    ''' <param name="key">Key to use for POST parameter.</param>
    ''' <param name="value">Value to use for POST parameter.</param>
    Public Sub AddPostData(ByVal key As String, ByVal value As String)
        nvcParams.Add(key, value)
    End Sub
    ''' <summary>
    ''' Add multiple POST parameters.
    ''' </summary>
    ''' <param name="params">Multiple parameters to add to the POST request.</param>
    Public Sub AddPostData(ByVal params As NameValueCollection)
        For Each key In params.Keys
            nvcParams.Add(key, params(key))
        Next
    End Sub
    ''' <summary>
    ''' Add single FILE parameter.
    ''' </summary>
    ''' <param name="key">Key to use for POST parameter.</param>
    ''' <param name="value">Value to use for POST parameter.</param>
    ''' <remarks></remarks>
    Public Sub AddFile(ByVal key As String, ByVal value As String)
        nvcFiles.Add(key, value)
    End Sub
    ''' <summary>
    ''' Add multiple FILE parameters.
    ''' </summary>
    ''' <param name="files">Multiple parameters to add to the POST request.</param>
    Public Sub AddFile(ByVal files As NameValueCollection)
        For Each key In files.Keys
            nvcFiles.Add(key, files(key))
        Next
    End Sub
    ''' <summary>
    ''' Send current request.
    ''' </summary>
    Public Sub Send()
        ' Make sure there isn't any data in the content body.
        sbContentBody.Clear()

        ' Frameweld uses Tls for HTTPS requests.
        ServicePointManager.SecurityProtocol = spSecurityProtocol

        ' Bytes stored in content body of a POST request only.
        Dim contentBodyBytes() As Byte
        Dim wr As HttpWebRequest = DirectCast(WebRequest.Create(strUri), HttpWebRequest)

        wr.Accept = strAccept
        wr.Method = strMethod
        wr.KeepAlive = True
        wr.ProtocolVersion = HttpVersion.Version11
        wr.Timeout = intTimeout

        If Not IsNothing(nvcHeaders) Then
            For Each key In nvcHeaders.Keys
                wr.Headers.Add(key, nvcHeaders(key))
            Next
        End If

        Select Case strMethod
            Case "POST"
                Using rs As Stream = wr.GetRequestStream()
                    wr.ContentType = "multipart/form-data; boundary=" & strBeginBoundary

                    ' The below will add the POST parameters to the content body.
                    If Not IsNothing(nvcParams) Then
                        For Each key As String In nvcParams.Keys
                            sbContentBody.Append(strContentBoundary)
                            sbContentBody.AppendLine()
                            sbContentBody.Append(CONTENT_DISPOSITION & """" & key & """")
                            sbContentBody.AppendLine()
                            sbContentBody.AppendLine()
                            sbContentBody.Append(nvcParams(key))
                            sbContentBody.AppendLine()
                        Next

                        contentBodyBytes = Encoding.ASCII.GetBytes(sbContentBody.ToString())
                        rs.Write(contentBodyBytes, 0, contentBodyBytes.Length)
                    End If

                    If Not IsNothing(nvcFiles) Then
                        ' The below will include files to POST conent body.
                        For Each filePath In nvcFiles.Keys
                            sbContentBody.Clear()
                            sbContentBody.Append(strContentBoundary)
                            sbContentBody.AppendLine()
                            sbContentBody.Append(CONTENT_DISPOSITION & """" & filePath & """; filename=""" & Path.GetFileName(nvcFiles(filePath)) & """")
                            sbContentBody.AppendLine()
                            sbContentBody.Append("Content-type: application/octet-stream")
                            sbContentBody.AppendLine()
                            sbContentBody.AppendLine()

                            contentBodyBytes = Encoding.ASCII.GetBytes(sbContentBody.ToString())
                            rs.Write(contentBodyBytes, 0, contentBodyBytes.Length)

                            Using fileStream As FileStream = New FileStream(nvcFiles(filePath), FileMode.Open, FileAccess.Read)
                                Dim bufferLength As Integer = intChunkSize
                                Dim buffer(bufferLength - 1) As Byte
                                Dim contentLen As Integer = fileStream.Read(buffer, 0, bufferLength)

                                Do While contentLen <> 0
                                    rs.Write(buffer, 0, contentLen)
                                    contentLen = fileStream.Read(buffer, 0, bufferLength)
                                Loop

                                fileStream.Close()
                            End Using
                        Next
                    End If

                    ' Write closing boundary to content body
                    sbContentBody.Clear()
                    sbContentBody.AppendLine()
                    sbContentBody.Append(strEndingBoundary)
                    contentBodyBytes = Encoding.ASCII.GetBytes(sbContentBody.ToString())

                    rs.Write(contentBodyBytes, 0, contentBodyBytes.Length)
                    rs.Close()

                    nvcHeaders = Nothing
                    nvcFiles = Nothing
                    nvcParams = Nothing
                End Using
        End Select

        Try
            Dim wresp As HttpWebResponse = DirectCast(wr.GetResponse(), HttpWebResponse)

            If wresp.StatusCode = Net.HttpStatusCode.OK Then
                Dim responseStream As Stream = wresp.GetResponseStream()
                Dim reader As New StreamReader(responseStream)
                Dim serverResponse As String = reader.ReadToEnd()

                reader.Close()
                responseStream.Close()
                wresp.Close()

                strResponse = serverResponse
            Else
                strResponse = wresp.StatusCode & " - " & wresp.StatusDescription
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
End Class
