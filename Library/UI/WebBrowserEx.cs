using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeOps.Interface.Views
{
    public class WebBrowserEx : WebBrowser
    {
        bool MonoRunning;

        string OriginalDoc = "";


        public WebBrowserEx()
        {
            MonoRunning = IsRunningOnMono();

            if(MonoRunning)
                Stop(); // apparently makes .Document not null
        }

        public bool IsRunningOnMono()
        {
            return (Type.GetType("Mono.Runtime") != null);//&& Properties.Settings.Default.MonoHelp);
        }

        public new bool ScrollBarsEnabled
        {
            get
            {
                if (!MonoRunning)
                    return base.ScrollBarsEnabled;
                else
                    return false;
            }
            set
            {
                if (!MonoRunning)
                    base.ScrollBarsEnabled = value;
            }
        }

        public new string DocumentText
        {
            get
            {
                if (!MonoRunning)
                    return base.DocumentText;
                else
                    return OriginalDoc;
            }
            set
            {
                if (!MonoRunning)
                    base.DocumentText = value;

                else
                {

                    OriginalDoc = value;

                    if (Document != null)
                    {
                        Document.Write(value);
                        Stop(); // gets rid of hour glass
                    }
                }
            }
        }

        Dictionary<string, string> Tags = new Dictionary<string, string>();

        public void SafeInvokeScript(string scriptName, object[] args)
        {
            if (Document == null)
                return;

            if (!MonoRunning)
            {
                Document.InvokeScript(scriptName, args);
                return;
            }

            if (args.Length < 2)
                return;

            string element = args[0] as string;
            string value = args[1] as string;

            Tags[element] = value;

            string doc = OriginalDoc;

            foreach (KeyValuePair<string, string> pair in Tags)
                doc = doc.Replace("<?=" + pair.Key + "?>", pair.Value);

            if (Document != null)
            {
                Document.Write(doc);
                Stop(); // gets rid of hour glass
            }
        }

        public void SetDocNoClick(string html)
        {
            if (IsRunningOnMono())
                DocumentText = html;
            else
            {
                Hide();
                DocumentText = html;
                Show();
            }
        }
    }
}
