using Integrative.Lara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Pages
{
    [LaraPage("/counter")]
    public class CounterPage : IPage
    {
        //int counter = 0;

        //public Task OnGet()
        //{
        //    var button = Element.Create("button");

        //    LaraUI.Page.Document.Body.AppendChild(button);

        //    button.InnerText = "Click me";
        //    button.On("click", () =>
        //    {
        //        counter++;
        //        button.InnerText = $"Clicked {counter} times";
        //        return Task.CompletedTask;
        //    });
        //    return Task.CompletedTask;
        //}

        CounterData data;

        public Task OnGet()
        {
            data = new CounterData();
            data.Counter = 12;

            var document = LaraUI.Page.Document;
            //SampleAppBootstrap.AppendTo(document.Head);
            var builder = new LaraBuilder(document.Body);

            builder
                .Push("div")
                    .Push("span")
                        .BindInnerText(data, x => x.Counter.ToString())
                    .Pop()
                    .Push("button")
                        .On("click", () => data.Counter++)
                        .AppendText("increase")
                    .Pop()
                .Pop();

            return Task.CompletedTask;
        }

        public Task OnIncrease()
        {
            data.Counter++;
            //int.TryParse(_number.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
            //number++;
            //_number.Value = number.ToString(CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        }
    }

    internal class CounterData : BindableBase
    {
        private int _counter;

        public int Counter
        {
            get => _counter;
            set => SetProperty(ref _counter, value);
        }
    }
}
