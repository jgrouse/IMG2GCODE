Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.ShowDialog()
        TextBox1.Text = OpenFileDialog1.FileName
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim outputfile As String = TextBox1.Text + ".csv"
        Dim output As String = ""
        Dim myBitmap As New Bitmap(TextBox1.Text)
        Dim picwidth As Integer = myBitmap.Width
        Dim picheight As Integer = myBitmap.Height
        Dim aspectratio As Double = picwidth / picheight
        Dim outwidth As Single = CSng(Width.Text)
        If Height.Text = "" Then
            Height.Text = CStr(CDbl(Width.Text) / aspectratio)
        End If
        Dim xoffset As Double = CDbl(X_Offset.Text)
        Dim yoffset As Double = CDbl(Y_Offset.Text)

        Dim feedr As Single = CSng(Feedrate.Text)

        Dim outheight As Single = CSng(Height.Text)

        Dim dotsize As Single = 0.2

        Dim scanlines As Integer = outheight / dotsize 'vertical
        Dim scanrows As Integer = outwidth / dotsize 'horizontal

        'resize image to output resolution
        'myBitmap = ResizeBitmap(myBitmap, scanrows, scanlines)
        myBitmap = ScaleImage(myBitmap, scanlines, scanrows)

        Dim outLineCount As Integer = 0

        Dim powerDiff As Double = 0
        Dim currentPower As Double = 0
        Dim previousPower As Double = 0

        Dim OutputAxis As String = ""
        Select Case LaserAxis.SelectedItem
            Case "Spindle"
                OutputAxis = "S"
            Case "A", "B", "C"
                OutputAxis = LaserAxis.SelectedItem
            Case Else
                OutputAxis = "S"
        End Select
        output += "( Picture Engrave )" + vbCrLf + _
                  "( File created: " + DateTime.Now.ToString("f") + " )" + vbCrLf + _
                  "( for Mach3 from IMG2GCODE )" + vbCrLf + _
                  "( Picture Size )" + vbCrLf + _
                  "( X= " + Format(outwidth, "0.000") + ", Y= " + Format(outheight, "0.000") + " )" + vbCrLf + _
                  "N100G00G21G17G90G40G49G80" + vbCrLf + _
                  "N110G71G91.1" + vbCrLf + _
                  "N120T1M06" + vbCrLf + _
                  "N130S0M03" + vbCrLf + _
                  "N140G94" + vbCrLf

        outLineCount = 150
        Dim GoRight As Boolean = True

        outLineCount += 10
        output += "N" + CStr(outLineCount) + "G92A0.000" + vbCrLf
        outLineCount += 10
        output += "N" + CStr(outLineCount) + "G01Y" + Format((yoffset + outheight) - (dotsize / 2), "0.000") + "X" + Format(xoffset + (dotsize / 2), "0.000") + "F" + Format(feedr, "0.0") + vbCrLf
        outLineCount += 10
        output += "N" + CStr(outLineCount) + "G91" + vbCrLf
        Dim dir As Integer = 1
        Dim XCounter As Double = 0
        For scanline = 0 To scanlines - 1

            Select Case GoRight
                Case True 'left to right
                    XCounter = 0
                    For scanrow = 0 To scanrows - 1
                        Dim col As Color = myBitmap.GetPixel(scanrow, scanline)
                        currentPower = col.GetBrightness * 0.0255
                        powerDiff = currentPower - previousPower
                        If powerDiff <> 0 Then
                            outLineCount += 10
                            output += "N" + CStr(outLineCount) + "G01X" + Format(XCounter, "0.000")
                            Select Case OutputAxis
                                Case "S"
                                    output += "S" + Format(currentPower * 10000, "0") + vbCrLf
                                Case "A", "B", "C"
                                    outLineCount += 10
                                    output += "N" + CStr(outLineCount) + "G01" + OutputAxis + Format(powerDiff, "0.0000") + vbCrLf
                            End Select
                            XCounter = 0
                        End If
                        XCounter += dotsize
                        previousPower = currentPower
                    Next
                    outLineCount += 10
                    output += "N" + CStr(outLineCount) + "G01X" + Format(XCounter, "0.000")
                    Select Case OutputAxis
                        Case "S"
                            output += "S" + Format(currentPower * 10000, "0") + vbCrLf
                        Case "A", "B", "C"
                            outLineCount += 10
                            output += "N" + CStr(outLineCount) + "G01" + OutputAxis + "0.0000" + vbCrLf
                    End Select
                    GoRight = False
                Case False 'right to left
                    XCounter = 0
                    For scanrow = 0 To scanrows - 1
                        Dim col As Color = myBitmap.GetPixel(scanrow, scanline)
                        currentPower = col.GetBrightness * 0.0255
                        powerDiff = currentPower - previousPower
                        If powerDiff <> 0 Then
                            outLineCount += 10
                            output += "N" + CStr(outLineCount) + "G01X" + Format(XCounter, "0.000")
                            Select Case OutputAxis
                                Case "S"
                                    output += "S" + Format(currentPower * 10000, "0") + vbCrLf
                                Case "A", "B", "C"
                                    output += OutputAxis + Format(powerDiff, "0.0000") + vbCrLf
                            End Select


                            XCounter = 0
                        End If
                        XCounter -= dotsize
                        previousPower = currentPower
                    Next
                    outLineCount += 10
                    output += "N" + CStr(outLineCount) + "G01X" + Format(XCounter, "0.000")
                    Select Case OutputAxis
                        Case "S"
                            output += "S" + Format(currentPower * 10000, "0") + vbCrLf
                        Case "A", "B", "C"
                            output += OutputAxis + "0.0000" + vbCrLf
                    End Select
                    GoRight = True
            End Select
            outLineCount += 10
            output += "N" + CStr(outLineCount) + "G01Y-" + Format(dotsize, "0.000")
            Select Case OutputAxis
                Case "S"
                    output += "S0" + vbCrLf
                Case "A", "B", "C"
                    output += OutputAxis + "0.0000" + vbCrLf
            End Select


        Next

        outLineCount += 10
        output += "N" + CStr(outLineCount) + "M05" + vbCrLf
        outLineCount += 10
        output += "N" + CStr(outLineCount) + "G90" + vbCrLf
        outLineCount += 10
        output += "N" + CStr(outLineCount) + "G00" + OutputAxis + "0" + vbCrLf
        outLineCount += 10
        output += "N" + CStr(outLineCount) + "G00X0.000Y0.000" + vbCrLf
        outLineCount += 10
        output += "N" + CStr(outLineCount) + "M30" + vbCrLf

        IO.File.WriteAllText(outputfile, output)

        MsgBox("Done!")

    End Sub

    Function ResizeBitmap(ByVal bitmapToResize As Bitmap, ByVal width As Integer, ByVal height As Integer) As Bitmap


        'make a blank bitmap the correct size
        Dim NewBitmap As New Bitmap(width, height)
        'make an instance of graphics that will draw on "NewBitmap"
        Dim BitmpGraphics As Graphics = Graphics.FromImage(NewBitmap)
        'work out the scale factor
        Dim scaleFactorX As Integer = bitmapToResize.Width / width
        Dim scaleFactorY As Integer = bitmapToResize.Height / width
        'resize the graphics
        BitmpGraphics.ScaleTransform(scaleFactorX, scaleFactorY)
        'draw the bitmap to NewBitmap
        BitmpGraphics.DrawImage(bitmapToResize, 0, 0)
        Return NewBitmap
    End Function

    Public Function ScaleImage(ByVal OldImage As Image, ByVal TargetHeight As Integer, ByVal TargetWidth As Integer) As System.Drawing.Image

        Dim NewHeight As Integer = TargetHeight
        Dim NewWidth As Integer = NewHeight / OldImage.Height * OldImage.Width

        If NewWidth > TargetWidth Then
            NewWidth = TargetWidth
            NewHeight = NewWidth / OldImage.Width * OldImage.Height
        End If

        Return New Bitmap(OldImage, NewWidth, NewHeight)

    End Function

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles X_Offset.TextChanged

    End Sub
End Class
