using Integrative.Lara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Bulma
{
    public static class BulmaLoader
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/lib/bulma/bulma.min.css"
            });
        }
    }
}
