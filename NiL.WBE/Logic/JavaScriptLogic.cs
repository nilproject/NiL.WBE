using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web;
using NiL.JS;
using NiL.JS.Core;
using System.IO;

namespace NiL.WBE.Logic
{
    public class JavaScriptLogic : LogicProvider
    {
        private static string defaultPath = null;

        private class LoadedScript
        {
            private long lastUpdate;
            private string path;
            private Script script;
            public Script Script
            {
                get
                {
                    var clu = new FileInfo(path).LastWriteTime.Ticks;
                    if (clu > lastUpdate)
                    {
                        using (var file = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            script = new Script(new System.IO.StreamReader(file).ReadToEnd());
                        }
                        lastUpdate = clu;
                    }
                    return script;
                }
            }

            public LoadedScript(string path)
            {
                this.path = validatePath(path);                
            }
        }

        private LoadedScript defaultScript;

        private Dictionary<string, object> routeMap;

        private static string validatePath(string path)
        {
            if (File.Exists(path))
                return path;
            else if (File.Exists(Global.RootDirectory + path))
                return Global.RootDirectory + path;
            else
                throw new FileNotFoundException("File \"" + path + "\" not found.");
        }

        private static JSObject loadTemplateRaw(Context context, JSObject args)
        {
            string templateName = args.GetField("0").ToString();
            var sect = (WebConfigurationManager.GetSection("templates") as Html.TemplateElementCollection)[templateName];
            templateName = sect.Path ?? templateName;
            templateName = validatePath(templateName);
            string templateText = "";
            var file = new FileStream(templateName, FileMode.Open, FileAccess.Read);
            templateText = new StreamReader(file).ReadToEnd();
            file.Close();
            return templateText;
        }

        private static JSObject loadTemplate(Context context, JSObject args)
        {
            string templateName = args.GetField("0").ToString();
            var sect = (WebConfigurationManager.GetSection("templates") as Html.TemplateElementCollection)[templateName];
            templateName = sect.Path ?? templateName;
            templateName = validatePath(templateName);
            string templateText = "";
            var file = new FileStream(templateName, FileMode.Open, FileAccess.Read);
            templateText = new StreamReader(file).ReadToEnd();
            file.Close();
            var template = NiL.WBE.Html.HtmlPage.Parse(templateText);
            return TypeProxy.Proxy(template);
        }

        static JavaScriptLogic()
        {            
            defaultPath = WebConfigurationManager.AppSettings["jsLogicDefault"] ?? "scripts/default.js";
            NiL.JS.Core.Context.GlobalContext.InitField("loadTemplate").Assign(new NiL.JS.Core.ExternalFunction(loadTemplate));
            NiL.JS.Core.Context.GlobalContext.InitField("loadTemplateRaw").Assign(new NiL.JS.Core.ExternalFunction(loadTemplateRaw));
            NiL.JS.Core.Context.GlobalContext.InitField("System").Assign(new NiL.JS.NamespaceProvider("System"));
            NiL.JS.Core.Context.GlobalContext.InitField("NiL").Assign(new NiL.JS.NamespaceProvider("NiL"));
        }

        private JSObject mapRoute(Context context, JSObject args)
        {
            if ((int)args.GetField("length").Value >= 2)
            {
                string name = args.GetField("0").ToString();
                object target = args.GetField("1").Value;
                if (target != null && !string.IsNullOrWhiteSpace(name))
                    routeMap[name] = target;
            }
            return null;
        }

        public JavaScriptLogic()
        {
            routeMap = new Dictionary<string, object>();
            NiL.JS.Core.Context.GlobalContext.InitField("mapRoute").Assign(new ExternalFunction(mapRoute));
            var initpath = WebConfigurationManager.AppSettings["jsLogicInit"];
            if (!string.IsNullOrWhiteSpace(initpath))
            {
                var initScript = new LoadedScript(initpath);
                initScript.Script.Invoke();
            }
        }

        public override void Process(HttpRequest request, HttpResponse response, HttpApplication application)
        {
            object processor = null;
            if (routeMap.TryGetValue(request.Path, out processor))
            {
                if (processor is string)
                {
                    processor = new LoadedScript(processor.ToString());
                    routeMap[request.Path] = processor;
                }
                if (processor is NiL.JS.Core.BaseTypes.Function)
                {
                    (processor as NiL.JS.Core.BaseTypes.Function).Invoke(new NiL.JS.Core.BaseTypes.Array(new object[] { request, response, application }));
                    return;
                }
                else if (processor is LoadedScript)
                {
                    var sc = (processor as LoadedScript).Script;
                    sc.Context.InitField("application").Assign(TypeProxy.Proxy(application));
                    sc.Context.InitField("request").Assign(TypeProxy.Proxy(request));
                    sc.Context.InitField("response").Assign(TypeProxy.Proxy(response));
                    sc.Invoke();
                    return;
                }
            }

            if (defaultScript == null)
            {
                try
                {
                    defaultScript = new LoadedScript(defaultPath);
                }
                catch
                {
                    response.BinaryWrite(System.Text.Encoding.Default.GetBytes(new Http.ErrorPage(Http.ResponseCode.SERVICE_UNAVAILABLE, "Service unavailable.").ToString()));
                    response.StatusCode = (int)Http.ResponseCode.SERVICE_UNAVAILABLE;
                }
            }
            lock (defaultScript)
            {
                var script = defaultScript.Script;
                script.Context.InitField("application").Assign(TypeProxy.Proxy(application));
                script.Context.InitField("request").Assign(TypeProxy.Proxy(request));
                script.Context.InitField("response").Assign(TypeProxy.Proxy(response));
                script.Invoke();
            }
        }
    }
}