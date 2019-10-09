using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac.Features.Metadata;
using FB.Annotations;
using FB.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace GroupChess
{
    public class FrontendController : Controller
    {
        private IPathResolver _rootResolver;

        public FrontendController(IEnumerable<Meta<IPathResolver>> resolvers)
        {
            _rootResolver = resolvers.First(x => (WellKnownDirectory) x.Metadata[nameof(WellKnownDirectory)] == WellKnownDirectory.WebDeployment).Value;
        }

        [Route("/")]
        public ActionResult Index()
        {
            var info = _rootResolver.Resolve(Path.Combine("Content", "index.html"));
            return new PhysicalFileResult(info.FullName,"text/html; charset=utf-8");
        }
        [Route("/{path:regex(\\.(js|html|css)$)}", Order = int.MaxValue)]
        public IActionResult StaticContent(string path) {
            var info = _rootResolver.Resolve(Path.Combine("Content", path));
            if (info.Exists && info.FullName.StartsWith(_rootResolver.Resolve("Content").FullName)) {
                var ctype = "application/octet-stream";
                switch (info.Extension.ToLower()) {
                    case ".cshtml":
                        return NotFound();
                    case ".js":
                        ctype = "text/javascript; charset=utf-8";
                        break;
                    case ".css":
                        ctype = "text/css; charset=utf-8";
                        break;
                    case ".html":
                        ctype = "text/html; charset=utf-8";
                        break;
                }

                return new PhysicalFileResult(info.FullName, ctype);
            }

            return NotFound();
        }
    }
}