using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace XLibrary.UI.Panels
{
    public partial class AboutPanel : UserControl
    {
        [BrowsableAttribute(true)]
        public Orientation Orientation
        {
            set
            {
                splitContainer1.Orientation = value;
            }
            get
            {
                return splitContainer1.Orientation;
            }
        }
        
        public AboutPanel()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            // either version of running builder, or verseion that built this viewer
            VersionLabel.Text = "Version: " + XRay.BuilderVersion;
        }

        public void Init()
        {
            NewsBrowser.Navigate("about:blank");
            HtmlDocument doc = NewsBrowser.Document;
            doc.Write(String.Empty);
            SetCopyrightText();
        }

        private void AboutPanel_Resize(object sender, EventArgs e)
        {
            int distance = (Orientation == Orientation.Horizontal) ? panel1.Height : panel1.Width;

            splitContainer1.SplitterDistance = distance;
        }

        private void CodePerspectiveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/swax/CodePerspective") { UseShellExecute = true });
        }

        private void SetCopyrightText()
        {
            NewsBrowser.DocumentText = @"
<html>
<head>
    <style type=""text/css"">
        body {
            font-family:Arial,Helvetica,sans-serif;
            font-size:12px;
            margin:0px;
            background:White;
        }
    </style>
</head>
<body>
<h3>Code Perspective</h3>
Copyright (C) 2009-2023 John Marshall<br />
<br />
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
<br />
<br />

<h3>Cecil</h3>
Author: Jb Evain (jbevain@gmail.com)<br />
Copyright (c) 2008 - 2011 Jb Evain<br />
<br />
Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
""Software""), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:<br />
<br />
The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.<br />
<br />
THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTITWARE.<br />
<br />
<br />

<h3>ICSharpCode.Decompiler</h3>
Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team<br />
<br />
Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the ""Software""), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
to whom the Software is furnished to do so, subject to the following conditions:<br />
<br />
The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.<br />
<br />
THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.<br />
<br />
<br />

<h3>OpenTK</h3>
Copyright (c) 2006 - 2009 The Open Toolkit library.<br />
<br />
Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the ""Software""), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions<br />
<br />
The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.<br />
<br />
THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.<br />
<br />
";
        }

    }
}
