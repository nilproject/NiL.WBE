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

        private static JSObject validateFileName(Context context, JSObject args)
        {
            string fileName = args.GetField("0").ToString();
            return validatePath(fileName);
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

        private static JSObject loadSnippet(Context context, JSObject args)
        {
            string templateName = args.GetField("0").ToString();
            var sect = (WebConfigurationManager.GetSection("templates") as Html.TemplateElementCollection)[templateName];
            templateName = sect.Path ?? templateName;
            templateName = validatePath(templateName);
            string templateText = "";
            var file = new FileStream(templateName, FileMode.Open, FileAccess.Read);
            templateText = new StreamReader(file).ReadToEnd();
            file.Close();
            var template = NiL.WBE.Html.HtmlElement.Parse(templateText);
            return TypeProxy.Proxy(template);
        }

        static JavaScriptLogic()
        {
            defaultPath = WebConfigurationManager.AppSettings["jsLogicDefault"] ?? "scripts/default.js";
            NiL.JS.Core.Context.GlobalContext.InitField("ipsum").Assign(ipsum);
            NiL.JS.Core.Context.GlobalContext.InitField("loadTemplate").Assign(new NiL.JS.Core.ExternalFunction(loadTemplate));
            NiL.JS.Core.Context.GlobalContext.InitField("loadSnippet").Assign(new NiL.JS.Core.ExternalFunction(loadSnippet));
            NiL.JS.Core.Context.GlobalContext.InitField("loadTemplateRaw").Assign(new NiL.JS.Core.ExternalFunction(loadTemplateRaw));
            NiL.JS.Core.Context.GlobalContext.InitField("validateFileName").Assign(new NiL.JS.Core.ExternalFunction(validateFileName));
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

        private const string ipsum = @"Jelly cookie powder cotton candy chocolate cake lollipop. Tootsie roll pastry halvah pudding chocolate bar candy canes applicake sweet. Topping cookie chocolate wafer dessert. Applicake jelly-o bear claw powder. Caramels sweet roll cookie. Sesame snaps tart I love macaroon bonbon cheesecake. Macaroon cookie pudding. I love tiramisu powder applicake soufflé biscuit. Macaroon bear claw danish powder sesame snaps candy canes. Sweet I love dessert jelly-o I love sesame snaps. Danish pie donut. Apple pie bear claw macaroon danish candy macaroon chupa chups chocolate.
            I love chocolate donut ice cream ice cream gingerbread donut croissant. Sugar plum caramels lemon drops topping. Topping I love jelly beans caramels brownie. Wafer macaroon tiramisu. Gingerbread dragée cheesecake chocolate sesame snaps marzipan chocolate bar. Fruitcake jelly beans oat cake candy canes oat cake lollipop powder. Dragée caramels gummi bears unerdwear.com dragée I love cheesecake icing I love. I love tootsie roll toffee fruitcake jelly jujubes I love jelly-o topping. Halvah marzipan cotton candy oat cake soufflé I love. Tiramisu halvah I love jelly beans marzipan. Ice cream applicake cake cookie applicake soufflé. Liquorice fruitcake jelly wafer oat cake donut croissant I love chocolate cake. Toffee cotton candy candy chocolate bar.
            Gummies marzipan croissant gummies oat cake. Oat cake jelly-o marzipan wafer muffin. Gummi bears pie jelly beans. Tiramisu I love pudding tart topping cupcake biscuit toffee oat cake. Sweet roll topping croissant. Brownie cookie I love I love chocolate. I love bonbon toffee powder oat cake brownie. Marshmallow halvah pie sesame snaps bonbon. Chupa chups bear claw jelly topping pastry candy I love. Tootsie roll chocolate bar dessert macaroon cake jelly beans carrot cake lemon drops pie. Icing fruitcake tootsie roll icing tart I love. Powder bear claw I love sesame snaps jujubes marshmallow jelly beans pie. Jelly-o carrot cake pudding biscuit tootsie roll carrot cake cotton candy dragée. I love donut marshmallow marzipan liquorice pudding candy canes danish topping.
            Jelly beans sweet roll dessert muffin cotton candy cheesecake powder unerdwear.com tootsie roll. Fruitcake gummi bears halvah tart. Candy canes halvah topping carrot cake. Sweet I love bear claw croissant toffee I love dessert. Gingerbread gummies applicake. Brownie sesame snaps caramels soufflé I love. Soufflé jelly beans topping cookie sesame snaps. Jujubes croissant fruitcake brownie candy chocolate bar toffee. Unerdwear.com unerdwear.com candy canes marzipan. I love jelly I love ice cream pudding soufflé I love gummies gingerbread. I love toffee sweet roll gummies ice cream. Pudding icing jelly beans candy. Applicake toffee cake ice cream.
            Macaroon dragée I love tiramisu sweet. Unerdwear.com halvah jelly I love halvah I love. Pie topping dragée fruitcake. Ice cream candy bonbon gummies cookie. I love sweet roll halvah. Gingerbread fruitcake pudding. Lemon drops tart fruitcake caramels liquorice I love chocolate cake. Soufflé dessert biscuit. Topping apple pie I love topping marshmallow danish donut dessert. Donut chocolate cupcake icing muffin I love lemon drops I love. Tiramisu chocolate I love toffee gummi bears chocolate bar chocolate bar applicake. Marshmallow caramels chocolate. Chocolate cake I love tart.
            Halvah pie cotton candy cheesecake. Toffee unerdwear.com caramels icing lollipop candy canes bonbon applicake cotton candy. Jelly-o cake lollipop I love lemon drops bear claw pie. Caramels candy I love. Cookie I love I love jelly carrot cake dragée tootsie roll. Muffin dragée pudding brownie chocolate cake unerdwear.com chocolate bar. Sesame snaps lollipop I love jujubes I love pastry lemon drops oat cake. Caramels cookie gummies. Soufflé topping ice cream caramels unerdwear.com. Chocolate cake liquorice apple pie I love. Carrot cake I love I love cookie chocolate I love marshmallow cheesecake cotton candy. I love toffee pastry unerdwear.com cheesecake jelly jelly beans cupcake. Soufflé I love jelly beans fruitcake. Candy canes candy canes liquorice candy canes brownie dessert pastry I love tootsie roll.
            Chocolate cake I love gingerbread wafer tootsie roll pie powder. Dessert jelly-o fruitcake I love pastry toffee sweet roll marshmallow muffin. Chocolate tiramisu jelly-o tiramisu donut sugar plum. Gingerbread I love gingerbread. Jelly I love tootsie roll chupa chups chocolate cake I love. I love I love cake cookie. I love I love biscuit dessert lemon drops lemon drops. Oat cake caramels candy canes macaroon sweet chocolate fruitcake bonbon. Sugar plum sugar plum croissant muffin lollipop marshmallow lemon drops. Bear claw icing chocolate I love. Tart chocolate cake soufflé unerdwear.com donut croissant. Toffee I love bonbon muffin sesame snaps bear claw applicake cupcake.
            Topping jujubes lemon drops marshmallow gummies pastry cotton candy. Applicake donut apple pie lollipop pie. I love toffee caramels apple pie cotton candy tart. Bear claw sesame snaps apple pie marzipan I love tiramisu marzipan applicake. Lollipop bear claw marshmallow tiramisu gummies tiramisu tootsie roll. Tootsie roll I love jujubes icing applicake fruitcake candy canes. Topping bear claw dessert cookie bear claw pie halvah oat cake ice cream. Liquorice lollipop ice cream cookie sesame snaps. I love I love applicake marshmallow caramels. Donut cookie muffin I love candy. Jelly beans croissant muffin fruitcake brownie biscuit powder. Gingerbread sweet candy sweet cupcake unerdwear.com chocolate cake tiramisu chocolate bar.
            Fruitcake lemon drops I love jujubes lollipop toffee. Ice cream bear claw I love halvah. Croissant marshmallow candy canes sesame snaps. I love caramels dragée. Cheesecake oat cake brownie soufflé I love. Lollipop lemon drops oat cake I love ice cream bear claw toffee jelly. Cotton candy topping I love. Jelly-o donut brownie. Topping soufflé marzipan gummies powder sesame snaps candy. Tiramisu caramels pudding sugar plum topping lemon drops caramels. Chupa chups fruitcake jelly-o tart jelly-o tootsie roll. Topping chupa chups applicake candy dragée. I love croissant tart I love sugar plum I love oat cake unerdwear.com liquorice.
            I love powder toffee gingerbread pastry cupcake chocolate bar macaroon soufflé. Dessert sugar plum gingerbread candy gingerbread dessert chocolate cake bear claw. Marzipan chocolate biscuit liquorice. Marzipan bear claw tootsie roll chocolate sweet macaroon candy canes lollipop. I love sweet pudding gummi bears I love ice cream pie I love. Carrot cake I love donut dessert cake I love pastry bonbon. Sweet macaroon cupcake. Bonbon bonbon cheesecake brownie icing. Bonbon I love marshmallow. Unerdwear.com I love gummi bears icing fruitcake jelly-o. Pie I love macaroon gummies carrot cake sugar plum caramels chocolate cake. Topping ice cream tart applicake biscuit. Unerdwear.com I love toffee gummi bears oat cake.
            Candy canes tootsie roll soufflé cheesecake topping halvah ice cream jelly-o. Chocolate jelly beans tootsie roll cotton candy gingerbread sugar plum sweet roll applicake tart. Sesame snaps icing tart liquorice biscuit powder cookie. Gummi bears cookie soufflé I love tiramisu sweet danish cookie apple pie. Gummi bears apple pie apple pie apple pie. Sesame snaps croissant gingerbread. Marshmallow I love donut macaroon. Tiramisu gummi bears chocolate cake cookie I love applicake. Cheesecake sesame snaps soufflé sweet roll soufflé. Candy canes lemon drops powder gummi bears gummies I love. Danish tiramisu apple pie tiramisu jelly macaroon gummi bears bonbon. Fruitcake applicake ice cream sweet roll. Chocolate cake gingerbread danish cupcake. I love sugar plum marshmallow apple pie.
            Chocolate tootsie roll jujubes icing chupa chups caramels. Liquorice tootsie roll lemon drops dessert caramels cheesecake brownie gingerbread. Lollipop tiramisu croissant. Sugar plum wafer I love brownie unerdwear.com tootsie roll I love I love. Sesame snaps jujubes fruitcake. Oat cake tootsie roll dessert toffee I love gummies dessert. Cotton candy cake I love jelly-o halvah. Pastry lemon drops sweet roll apple pie chocolate bar. Bonbon applicake cookie. Sweet unerdwear.com pie gummies jelly-o. Jelly-o brownie cake. Soufflé dessert gummi bears. Apple pie I love muffin halvah marshmallow cake sugar plum. Danish bear claw unerdwear.com cotton candy I love.
            Cake lemon drops halvah sweet roll. Jujubes caramels I love. Lemon drops toffee I love sweet sweet roll I love. Cake chupa chups gummi bears jujubes. Cake chupa chups dragée. Tart cookie marshmallow unerdwear.com caramels I love cotton candy macaroon. Wafer jelly beans chocolate bar oat cake cheesecake chocolate bar. I love oat cake gummies sesame snaps. Candy canes pie pastry. Gummies dragée jelly-o. Danish icing halvah. Cake sugar plum halvah cupcake halvah gummies. Sweet roll cookie I love cheesecake jelly beans croissant.
            Chocolate cake ice cream pie jelly beans. Jujubes brownie icing halvah. Donut cookie applicake candy canes fruitcake chocolate bar. Jelly carrot cake carrot cake. Liquorice jelly-o macaroon biscuit tiramisu danish oat cake chupa chups unerdwear.com. Lollipop danish topping cheesecake. Muffin lollipop I love chupa chups soufflé I love topping sesame snaps. Toffee toffee muffin jujubes cheesecake jelly beans lemon drops cotton candy. Powder unerdwear.com powder gingerbread unerdwear.com jelly. Pastry wafer danish. Cheesecake candy marshmallow bonbon I love. Cheesecake cupcake I love caramels jelly beans jujubes sugar plum dessert unerdwear.com. Candy canes halvah fruitcake powder pudding candy muffin chupa chups. I love pastry gingerbread chocolate I love bonbon.
            Croissant croissant jujubes marzipan dragée. Caramels topping ice cream. Gingerbread gingerbread tiramisu gummi bears bear claw tootsie roll tart. Cotton candy carrot cake candy icing pie I love fruitcake. Carrot cake jelly beans soufflé I love pie cotton candy I love halvah. Sesame snaps bonbon tootsie roll cotton candy. Tart pastry cake I love biscuit I love. Cheesecake croissant chocolate cake bear claw jelly marzipan macaroon wafer. Brownie jelly-o chupa chups powder. I love tart gummies. Dragée croissant I love brownie oat cake candy canes unerdwear.com. Jujubes I love chupa chups icing tiramisu liquorice lollipop.
            Wafer halvah sugar plum caramels halvah. Candy canes cookie halvah wafer I love fruitcake chupa chups. Carrot cake lemon drops gingerbread fruitcake dragée lollipop cake unerdwear.com. Apple pie toffee macaroon dessert unerdwear.com caramels I love. Powder marshmallow carrot cake. Sweet roll oat cake candy bonbon donut. Cupcake macaroon donut dessert jelly cupcake croissant soufflé chocolate. Donut toffee oat cake fruitcake croissant. Cheesecake jujubes I love candy. Liquorice danish fruitcake soufflé oat cake sweet oat cake lemon drops gingerbread. Fruitcake I love jelly I love. Jelly-o toffee sugar plum soufflé cake unerdwear.com chocolate cake croissant chupa chups. Powder pudding cupcake tootsie roll candy. Gummies I love applicake bonbon topping marzipan cotton candy tart.
            Fruitcake sugar plum gummies sweet soufflé pie ice cream marzipan donut. Macaroon gummies ice cream unerdwear.com chocolate bar bonbon I love. Chocolate cake I love I love chocolate bar cookie I love bonbon I love. Jelly-o pastry I love. Applicake candy caramels cheesecake icing pudding. Danish chocolate cake. Applicake gingerbread sweet macaroon carrot cake I love. Danish toffee tootsie roll icing. Gummies jelly-o wafer unerdwear.com brownie cheesecake apple pie. Tootsie roll carrot cake marshmallow donut I love marzipan. I love sesame snaps jelly-o. Powder oat cake pastry applicake. Topping cake gingerbread soufflé tootsie roll gingerbread gummies.
            Gingerbread powder sesame snaps tart pastry marshmallow unerdwear.com sugar plum sugar plum. Sugar plum candy ice cream jelly liquorice marshmallow danish. Cupcake fruitcake wafer powder I love bonbon fruitcake. Jujubes bear claw lemon drops gummi bears apple pie jelly beans. Sugar plum tiramisu jujubes chocolate candy canes jelly beans sweet roll I love candy canes. Ice cream donut jelly beans gummies sugar plum jelly beans liquorice pie. Sugar plum I love pastry I love. Cake dessert applicake. Sweet roll jujubes sesame snaps I love cupcake I love unerdwear.com candy I love. Jelly-o toffee wafer sesame snaps chocolate bar. Cupcake powder lollipop I love candy cake I love. Croissant sugar plum jelly beans. Sugar plum sweet roll sweet chocolate cake pastry. Unerdwear.com topping donut cupcake marshmallow ice cream candy canes.
            Cotton candy pudding jelly beans topping muffin. Gummi bears pie cupcake I love bonbon. Lollipop chocolate macaroon I love toffee. Lollipop toffee sweet roll chocolate bar chocolate bar. Marshmallow bear claw donut cupcake fruitcake. Sugar plum soufflé lollipop. Apple pie bear claw cupcake icing powder applicake I love gummies. Croissant dragée toffee candy canes. Pudding marzipan sweet roll sweet roll gingerbread. Dragée I love liquorice. Liquorice chocolate lollipop. Pastry lollipop topping wafer donut. I love caramels lollipop danish bear claw dessert.
            Caramels tootsie roll tootsie roll caramels. Cheesecake chocolate bar jelly marshmallow dessert lollipop oat cake powder. Jelly beans I love oat cake donut oat cake jujubes bear claw pastry. Caramels cake I love. Toffee pudding cake I love pudding I love tart. Cake I love icing icing. Pudding ice cream marzipan brownie lemon drops oat cake toffee. Carrot cake tiramisu apple pie croissant chocolate bar candy canes tiramisu sweet roll. Jelly beans brownie jujubes. Tootsie roll gummies I love jujubes pastry I love tart halvah apple pie. Biscuit cupcake chocolate bar icing I love sesame snaps. Cookie tootsie roll toffee danish chocolate bar wafer I love soufflé gingerbread.
            Ice cream dessert donut chocolate cake pudding cheesecake. Lemon drops tart unerdwear.com donut chocolate cake danish muffin biscuit. I love lemon drops I love I love cupcake. Unerdwear.com danish apple pie pastry tootsie roll lollipop cheesecake. Applicake liquorice candy soufflé dragée. Sesame snaps soufflé ice cream danish. Pudding lemon drops I love carrot cake jelly unerdwear.com. Carrot cake I love tart brownie sesame snaps sweet croissant I love candy. Sugar plum unerdwear.com I love. Chocolate danish apple pie sweet jujubes. Dragée chocolate cake donut ice cream macaroon lollipop sesame snaps. Macaroon jelly-o bonbon sweet unerdwear.com. Powder pastry bonbon cupcake.
            Sesame snaps danish ice cream cake. Applicake croissant caramels I love donut apple pie brownie jujubes. Pie cake unerdwear.com. Chocolate bar soufflé oat cake. Halvah icing apple pie chupa chups. Chocolate bar caramels muffin pastry cookie brownie fruitcake macaroon. Donut brownie chocolate bar chupa chups gingerbread jujubes gummies gingerbread. Cookie jujubes jelly-o I love toffee caramels. Chupa chups I love carrot cake donut icing. Ice cream marshmallow wafer. Halvah unerdwear.com jujubes chocolate topping sesame snaps jelly-o marshmallow. Toffee applicake I love brownie apple pie caramels tootsie roll pudding.
            Candy sesame snaps apple pie apple pie I love chocolate bar gummi bears ice cream macaroon. Sesame snaps I love pudding sweet I love topping gingerbread. I love lemon drops sesame snaps. Unerdwear.com carrot cake toffee cotton candy powder. Icing candy canes pastry soufflé ice cream. Soufflé jujubes tart bonbon. Danish bear claw cookie. Sweet I love chupa chups. Tart chupa chups lollipop sweet liquorice. I love candy canes fruitcake dragée jelly beans unerdwear.com. I love tart I love dragée fruitcake unerdwear.com. I love cake cookie cotton candy donut tart topping. Brownie apple pie candy tootsie roll danish.
            I love cupcake cake marshmallow donut candy oat cake jujubes. I love tiramisu lemon drops sesame snaps toffee sugar plum. Halvah brownie ice cream tiramisu gummies tart sugar plum. Chupa chups croissant soufflé croissant. I love chocolate cheesecake icing dragée tart lemon drops. Halvah cheesecake chupa chups I love. Gummies pastry sweet roll dessert cake toffee. Pie cookie I love applicake powder cupcake. Jelly beans pie pastry I love sesame snaps jelly-o marzipan tootsie roll carrot cake. Jelly gummi bears I love. Wafer icing toffee sweet. Candy canes croissant candy canes I love tiramisu I love I love. Candy canes ice cream sweet.
            Candy cupcake applicake pudding bear claw. Tiramisu unerdwear.com sweet. Lollipop croissant toffee lemon drops. Sesame snaps chocolate cake lemon drops marshmallow gingerbread lollipop I love. Oat cake gingerbread croissant wafer gingerbread chocolate marzipan gingerbread. Oat cake sesame snaps dragée I love topping chocolate bar jelly-o marzipan wafer. I love bonbon bonbon jujubes jelly liquorice cotton candy cake. Icing I love gummies I love powder oat cake dessert cheesecake. Chocolate bar cake I love jelly-o bonbon cookie pudding. Icing sweet tiramisu unerdwear.com sugar plum biscuit. Unerdwear.com tart jelly-o chocolate marzipan I love dessert. Oat cake dessert macaroon. I love chocolate cake caramels I love jelly beans apple pie danish. Toffee I love dessert donut biscuit pie chocolate toffee.
            Chocolate bar sweet I love. Bear claw sugar plum muffin tootsie roll gummi bears chupa chups macaroon. Macaroon gummi bears cookie liquorice jelly beans. I love marshmallow jelly-o cheesecake jelly. Muffin caramels chupa chups halvah carrot cake. I love brownie fruitcake fruitcake cupcake. Pastry pudding cotton candy unerdwear.com. Brownie gummies gummi bears macaroon sesame snaps chocolate cake. Dessert pastry carrot cake. I love jelly beans cheesecake. Cookie chocolate bar fruitcake toffee chocolate cake soufflé halvah tiramisu pie. Cookie dessert chupa chups gummies danish I love pastry candy unerdwear.com. Apple pie sweet sweet roll chocolate bar dragée cupcake macaroon. I love cookie cupcake dessert dragée sugar plum chocolate tart jelly-o.
            Bonbon pie cheesecake I love pudding. Pudding halvah jelly beans I love cupcake biscuit jujubes brownie. Pie biscuit soufflé I love dessert cookie brownie icing. Sesame snaps sweet muffin applicake sugar plum soufflé ice cream. Applicake cake croissant liquorice carrot cake tart. Gummies jelly fruitcake gummies. Bear claw chocolate bar chocolate bar icing. Gummi bears macaroon chupa chups. Caramels chocolate I love. Unerdwear.com apple pie jelly beans unerdwear.com gingerbread fruitcake ice cream croissant wafer. Cake carrot cake fruitcake marshmallow cheesecake unerdwear.com fruitcake pastry. Croissant I love jelly.
            Donut candy chocolate cake. Macaroon I love I love I love sweet roll. Tart chocolate cake pie halvah gummi bears. Marshmallow jelly beans macaroon. Muffin I love chocolate I love sesame snaps. Bonbon gingerbread liquorice applicake gingerbread chocolate cake marzipan macaroon sweet. Sweet chupa chups cheesecake I love. Sweet topping icing I love pie oat cake marshmallow halvah I love. Soufflé unerdwear.com cupcake fruitcake cake gummi bears marzipan. Halvah fruitcake carrot cake macaroon toffee jelly beans marshmallow. Caramels halvah marzipan apple pie marshmallow tart soufflé macaroon dragée. Cupcake marzipan pastry donut unerdwear.com. Dessert donut I love ice cream jelly I love.
            Chocolate bar marzipan danish chocolate donut bear claw wafer I love tootsie roll. I love chocolate cake pie I love gummi bears. Powder soufflé cake marshmallow. Jelly-o danish macaroon sweet danish cotton candy I love I love jelly beans. I love icing cupcake jelly applicake I love. Lemon drops carrot cake pie croissant I love croissant dessert marshmallow chocolate. Candy lemon drops dragée croissant candy canes. Macaroon tiramisu I love pudding brownie donut I love apple pie candy canes. Jelly beans carrot cake cupcake. I love bear claw ice cream halvah gummies sweet sesame snaps caramels soufflé. Sesame snaps soufflé I love tart pudding powder macaroon wafer jelly beans. Cotton candy pastry I love jelly beans. Danish jelly beans muffin. Brownie apple pie bear claw oat cake dragée chocolate bar I love cotton candy oat cake.
            Dessert I love chocolate cake. Cookie unerdwear.com I love unerdwear.com apple pie marzipan sugar plum. I love carrot cake applicake chupa chups jujubes carrot cake marshmallow unerdwear.com. I love fruitcake tiramisu candy canes powder jujubes. I love sweet pudding unerdwear.com lollipop sweet I love. Applicake croissant halvah. Tootsie roll ice cream topping. Lemon drops caramels muffin muffin cake. Chupa chups lemon drops macaroon chupa chups biscuit sweet roll candy canes muffin. Brownie apple pie pie powder fruitcake. Lemon drops lemon drops pudding I love cookie sweet I love. Gummies powder candy canes marshmallow powder dragée.
            Biscuit powder tiramisu fruitcake gummi bears I love pudding cake. Brownie tiramisu applicake gingerbread sweet roll muffin icing. Dragée ice cream lemon drops soufflé I love gingerbread. Biscuit muffin lollipop. Pie danish pie gummi bears soufflé pie pudding liquorice jelly. I love tiramisu croissant. Liquorice caramels lemon drops. Pudding chocolate bar marshmallow bonbon chocolate cake. I love caramels pudding. I love sweet roll I love fruitcake. I love applicake gummies bonbon chupa chups sweet roll chocolate bar. Soufflé cheesecake carrot cake sesame snaps cotton candy liquorice tiramisu. Ice cream tart candy marzipan.
            Liquorice I love sesame snaps sesame snaps apple pie. Bonbon I love icing soufflé. Liquorice applicake toffee. Sweet lemon drops gummi bears sweet I love macaroon. Ice cream cake sugar plum toffee bear claw applicake dessert caramels. Bear claw jelly beans oat cake. Chocolate oat cake cupcake tootsie roll carrot cake. I love topping fruitcake. Cookie lemon drops cookie bear claw sesame snaps soufflé sesame snaps dragée. Powder donut pastry gummi bears I love gummi bears. Pudding candy sesame snaps applicake I love dessert. Pastry sugar plum oat cake jelly beans apple pie. I love apple pie unerdwear.com jelly-o donut cheesecake marshmallow.
            Jelly chocolate bar tiramisu chocolate cake I love gingerbread cookie ice cream. Cupcake I love soufflé lollipop. Powder oat cake I love sugar plum soufflé pudding dragée sweet roll sweet. Unerdwear.com danish oat cake bonbon. Muffin I love muffin topping. Powder fruitcake pastry jujubes. Powder apple pie I love cheesecake bear claw lemon drops croissant cake apple pie. Cheesecake tootsie roll ice cream sesame snaps I love liquorice I love bear claw. I love croissant soufflé jujubes. I love chocolate liquorice I love macaroon wafer sugar plum. Dragée sugar plum tart marshmallow jelly-o. Carrot cake cupcake chupa chups gingerbread chocolate cake sweet brownie marzipan dessert. Chocolate jelly croissant I love cheesecake pie. Tart chocolate carrot cake jelly.
            Muffin marzipan I love bear claw topping icing. Tootsie roll toffee sugar plum icing I love jelly. Tootsie roll bonbon pie ice cream caramels I love macaroon soufflé. Sweet gingerbread sesame snaps marzipan biscuit candy cotton candy chocolate bar. Applicake marzipan unerdwear.com chocolate jelly beans dragée cookie. Chocolate bar jelly-o gummi bears croissant lollipop bear claw. Cookie cookie jelly-o donut chocolate. Marshmallow topping cheesecake donut pudding I love unerdwear.com. Tiramisu cheesecake lollipop. Candy canes I love icing. Cheesecake jelly-o I love jelly sesame snaps sweet roll. Marshmallow sesame snaps danish.
            Pastry bear claw I love I love. Tiramisu pudding bear claw danish halvah jujubes. Sesame snaps lemon drops candy canes chocolate cake gummies liquorice I love gingerbread muffin. Marzipan sesame snaps dessert marzipan apple pie lollipop cheesecake. Carrot cake sweet bear claw macaroon applicake. Danish I love I love I love. Oat cake tootsie roll tootsie roll cookie. Croissant carrot cake cheesecake donut carrot cake. Pie applicake oat cake caramels. Croissant bear claw fruitcake soufflé I love cupcake carrot cake. Jelly beans fruitcake caramels. Chocolate dessert I love toffee sweet roll sweet roll. I love candy caramels toffee carrot cake cupcake lollipop. Oat cake candy I love oat cake marzipan.
            Ice cream wafer I love candy canes jelly beans lollipop croissant. Donut gummies I love candy canes apple pie unerdwear.com. Pudding bonbon candy marshmallow halvah ice cream oat cake jelly oat cake. I love oat cake cookie bonbon. Dessert topping cupcake bonbon lollipop. Cotton candy dessert I love gummi bears dragée soufflé I love I love. Danish jelly beans cupcake sweet cheesecake I love liquorice brownie pudding. Tart marshmallow I love jelly-o marshmallow. Halvah lollipop sweet roll powder danish topping. Pie candy jelly halvah liquorice jelly beans. Gummies lemon drops oat cake chocolate bar lemon drops I love. Carrot cake wafer chocolate bar I love sweet roll I love danish biscuit tart. Cotton candy toffee sesame snaps pudding muffin. Gummies cheesecake danish I love lemon drops chocolate cake cotton candy.
            I love sesame snaps halvah muffin jelly beans. Topping jelly beans candy candy. Dessert powder croissant liquorice marshmallow topping tiramisu icing muffin. Brownie cheesecake toffee cheesecake applicake marshmallow I love tiramisu. Cake halvah gummi bears croissant applicake toffee icing tootsie roll. Pastry sweet brownie topping macaroon tiramisu powder sesame snaps ice cream. Macaroon tiramisu gummi bears chocolate bar topping jelly-o. I love tiramisu jujubes wafer cookie powder I love cheesecake. Apple pie tart croissant. Bear claw jujubes marshmallow tiramisu I love dessert soufflé. Bear claw candy canes I love fruitcake bonbon marshmallow chocolate I love. I love jelly tiramisu. Apple pie gummi bears tart I love jelly I love.
            Cheesecake liquorice cotton candy toffee donut donut cookie. Marshmallow donut toffee candy canes topping oat cake lollipop. Lollipop sugar plum brownie jelly beans apple pie liquorice brownie chocolate cake. Liquorice gummi bears icing. Candy canes unerdwear.com danish. Applicake marzipan I love oat cake icing jelly-o donut. Sugar plum marshmallow donut carrot cake soufflé I love dessert bonbon chocolate cake. Jelly-o pudding gummies marshmallow. I love applicake ice cream. Bonbon sweet roll dessert powder cupcake jujubes dessert cake croissant. Bear claw halvah oat cake cupcake jelly cupcake jujubes. Candy donut sweet roll I love jelly beans jujubes lollipop. Pastry chocolate gummies applicake jelly-o gingerbread.
            Bonbon brownie I love pudding unerdwear.com. Chocolate biscuit croissant jelly. Liquorice jelly dragée I love fruitcake lemon drops cupcake dessert candy canes. Cotton candy pie soufflé jelly marzipan chocolate cake. Wafer dragée tootsie roll bear claw. I love sugar plum muffin candy canes. Pastry cotton candy I love wafer sweet topping lollipop. Candy I love sweet roll I love. I love danish bonbon gingerbread cheesecake chupa chups apple pie liquorice. Jelly beans bonbon candy cupcake gummies I love bonbon. Pudding jujubes halvah chocolate cake gummies brownie chupa chups. Biscuit candy carrot cake danish halvah sugar plum cookie carrot cake muffin.
            Candy jelly-o pie lemon drops fruitcake cake. I love powder muffin pastry wafer candy canes sweet roll I love liquorice. Fruitcake icing candy gummies jujubes. Biscuit icing I love. Muffin jelly-o chocolate bar bonbon lemon drops. Tart wafer cheesecake chupa chups unerdwear.com macaroon gummi bears sesame snaps. Sweet I love bonbon tiramisu. I love caramels chupa chups jujubes I love pastry lollipop sweet candy. Marshmallow bonbon sweet roll wafer. Pudding cupcake unerdwear.com cotton candy marzipan applicake. Dessert sesame snaps cotton candy ice cream sweet dessert gingerbread. Jelly pie carrot cake cotton candy carrot cake cotton candy cookie powder chocolate. Icing biscuit croissant fruitcake cupcake. Lollipop gingerbread powder sugar plum pudding.
            Bear claw donut I love bonbon cheesecake pastry. Gummies marshmallow wafer. Jelly tootsie roll muffin. Jelly dessert apple pie marzipan. Cake jelly donut bonbon sweet donut jujubes. Halvah I love I love I love ice cream chocolate bar jelly beans gummies. I love tootsie roll pie. Candy candy canes candy canes chupa chups tiramisu. Chocolate bar chocolate bar toffee. Liquorice chupa chups ice cream jelly beans cotton candy I love tootsie roll cupcake. Powder chocolate ice cream tart. Cake fruitcake I love. Gummi bears pudding wafer carrot cake. Croissant donut tart pudding I love dessert powder candy.
            Chocolate chupa chups I love topping biscuit gingerbread topping. Sugar plum applicake caramels sweet roll. Oat cake applicake sugar plum muffin icing. Gingerbread chupa chups bear claw wafer oat cake chocolate. I love muffin brownie pie. Pastry candy sweet roll carrot cake lemon drops toffee lemon drops macaroon carrot cake. Danish muffin apple pie biscuit fruitcake marzipan tootsie roll. Halvah cake oat cake marshmallow I love apple pie gingerbread. Oat cake chupa chups I love. Caramels cotton candy gummi bears tart bonbon. Donut jelly beans bonbon pastry. Cookie marshmallow ice cream ice cream liquorice sesame snaps chocolate bar applicake.
            Pudding muffin sesame snaps. I love jelly I love candy canes marshmallow. Macaroon danish toffee chupa chups jelly. Dragée liquorice sweet roll croissant macaroon sugar plum I love gummi bears apple pie. Sweet roll cake candy danish tiramisu. Topping dragée cupcake lemon drops biscuit powder macaroon danish. Sweet lemon drops wafer lollipop pie. Icing brownie unerdwear.com caramels I love cake gingerbread. Cake liquorice jelly carrot cake ice cream brownie ice cream tiramisu cookie. Gingerbread soufflé cotton candy sugar plum soufflé fruitcake toffee. Chupa chups tiramisu tootsie roll gingerbread chocolate cake bear claw I love chocolate bar. Candy canes lemon drops fruitcake pastry chocolate bar marzipan carrot cake applicake.
            Donut muffin soufflé chocolate icing. Icing tootsie roll chocolate tiramisu toffee ice cream. Marzipan toffee chocolate bar. Jujubes ice cream liquorice biscuit. Gingerbread I love wafer cupcake jelly liquorice bear claw oat cake pastry. Topping gingerbread pudding cheesecake jelly-o cupcake powder. Sugar plum marshmallow sweet roll pudding jelly beans. Sugar plum I love tootsie roll liquorice wafer jelly-o jelly jelly-o. Pastry bear claw icing muffin sesame snaps. Pie chupa chups jelly beans. Gingerbread wafer carrot cake danish chupa chups chocolate cake. Bear claw caramels I love soufflé soufflé muffin dessert chocolate cake.
            Cake chupa chups chocolate bar toffee chocolate icing caramels oat cake. Danish applicake ice cream. Dragée cake dessert liquorice cotton candy cupcake croissant unerdwear.com toffee. Lollipop sesame snaps pudding caramels brownie cake topping danish pastry. Macaroon cheesecake I love. Gingerbread gummies macaroon. Macaroon tart cupcake caramels halvah lemon drops applicake. Brownie soufflé I love soufflé dragée cheesecake marzipan cotton candy croissant. Chupa chups muffin jelly-o I love bear claw tiramisu ice cream I love. Bear claw danish icing caramels jujubes applicake biscuit. Icing wafer I love gingerbread soufflé jelly beans I love halvah sweet. Dessert I love cake chupa chups macaroon chocolate I love gummies. Brownie carrot cake lemon drops unerdwear.com jelly. I love bear claw muffin toffee halvah gingerbread sweet lollipop sesame snaps.
            Dessert applicake tootsie roll. Jelly marzipan oat cake halvah unerdwear.com cotton candy sweet jelly beans. Sugar plum liquorice jelly beans toffee lollipop sesame snaps cake dragée brownie. Cotton candy cookie soufflé cheesecake dragée tootsie roll fruitcake candy canes dessert. Chocolate bar cake dessert candy bear claw jelly-o. Apple pie donut chocolate caramels. Brownie liquorice cake lollipop sugar plum. Biscuit gummies oat cake croissant gummi bears. Sweet roll liquorice carrot cake gummies jelly beans bear claw gummies. Caramels pastry tootsie roll chupa chups tootsie roll. I love sesame snaps dragée cheesecake gummi bears. Ice cream bonbon liquorice gummi bears unerdwear.com pastry jujubes.
            Chupa chups chocolate lemon drops. Liquorice I love jelly biscuit macaroon topping gummi bears I love biscuit. Sweet halvah tart croissant apple pie I love I love dessert jujubes. Applicake topping cupcake I love I love donut ice cream. Gummi bears chocolate lollipop. Tart cookie croissant apple pie tootsie roll I love. Jelly-o biscuit carrot cake bonbon jelly. Cheesecake unerdwear.com sesame snaps lemon drops I love candy canes. Lemon drops I love cupcake wafer lollipop jelly-o. Marzipan marzipan jelly-o wafer fruitcake sugar plum bonbon. Pie chupa chups toffee jujubes marzipan sweet roll topping sweet soufflé. Sweet roll jelly applicake jelly macaroon. Cheesecake jelly-o dessert. Sugar plum soufflé I love pie pie caramels I love chocolate.
            Carrot cake jelly lollipop candy canes. Caramels jujubes brownie gummi bears icing topping. Macaroon pastry fruitcake cookie gummies oat cake candy canes. Pie caramels candy powder sweet sweet roll chocolate cake liquorice. Bonbon ice cream oat cake caramels oat cake chupa chups. Sweet wafer cotton candy cotton candy cupcake I love unerdwear.com I love icing. Pastry apple pie I love jelly tiramisu apple pie. Carrot cake pie I love chocolate gingerbread. Jelly-o candy canes croissant chupa chups jujubes I love. Dessert wafer biscuit chocolate cake tootsie roll I love jelly. Dragée muffin toffee marshmallow icing. Donut pastry cheesecake chocolate cake topping. Tiramisu unerdwear.com cupcake gummi bears lollipop cake candy. Candy oat cake caramels ice cream macaroon gummi bears I love bonbon pudding.
            Muffin fruitcake donut I love candy canes ice cream cotton candy cookie. Lemon drops powder icing. Marzipan biscuit pastry sweet powder tiramisu caramels. Halvah I love jujubes. Sesame snaps I love apple pie pudding pie tootsie roll ice cream. I love dragée gummi bears icing topping gummi bears. Marshmallow sugar plum chocolate cake candy. Pudding liquorice I love cotton candy sweet caramels cupcake pie. Oat cake chupa chups sugar plum topping unerdwear.com. Bear claw topping tootsie roll apple pie icing soufflé chocolate cake dessert. I love bonbon muffin lemon drops. Liquorice gingerbread jelly beans chocolate I love. Gingerbread jelly-o topping I love pastry chocolate cake. Candy canes gummi bears gummies marshmallow applicake.
            Sweet halvah tart muffin lollipop. Candy canes halvah I love I love cotton candy pie cheesecake. Candy jelly beans liquorice bear claw fruitcake. I love cupcake apple pie candy I love tiramisu fruitcake I love liquorice. Pudding marshmallow I love tart chocolate bar marshmallow. Jelly beans I love candy canes donut. Jujubes pudding sesame snaps macaroon oat cake halvah wafer chocolate bar. I love sesame snaps tart caramels jelly beans jelly macaroon pudding bear claw. Toffee candy chocolate muffin toffee candy chocolate cotton candy. Carrot cake jelly-o wafer liquorice applicake. Chupa chups chocolate halvah marzipan pastry topping. Pastry muffin I love unerdwear.com pie apple pie chocolate. Sweet marshmallow danish carrot cake chocolate bar danish bonbon.
            ";
    }
}