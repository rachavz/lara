using Integrative.Lara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Pages
{
    [LaraPage("/")]
    public class HomePage : IPage
    {
        int counter = 0;

        public Task OnGet()
        {
            //BulmaLoader.AppendTo(LaraUI.Page.Document.Head);
            //Element.CreateFromJson(LaraUI.Page.Document.Body, json, null);

            var document = LaraUI.Page.Document;
            //SampleAppBootstrap.AppendTo(document.Head);
            var builder = new LaraBuilder(document.Body);

            builder.Push("div", "container")
                .Push("table", "table table-hover")
                    .Push("thead", "thead-light")
                        .Push("tr")
                            .Push("th")
                                .Attribute("scope", "col")
                                .AppendText("First name")
                            .Pop()
                            .Push("th")
                                .Attribute("scope", "col")
                                .AppendText("Last name")
                            .Pop()
                        .Pop()
                        .Push("tbody")
                            .Push("tr")
                                .Push("td").AppendText("John").Pop()
                                .Push("td").AppendText("Jones").Pop()
                            .Pop()
                            .Push("tr")
                                .Push("td").AppendText("Amy").Pop()
                                .Push("td").AppendText("Smith").Pop()
                            .Pop()
                        .Pop()
                    .Pop()
                .Pop()
            .Pop();

            return Task.CompletedTask;
        }

        private Task OnIncrease()
        {
            //int.TryParse(_number.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
            //number++;
            //_number.Value = number.ToString(CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        }
    }
}
