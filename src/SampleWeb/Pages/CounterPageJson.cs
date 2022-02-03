using Integrative.Lara;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SampleWeb.Bulma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWeb.Pages
{
    [LaraPage("/counterjson")]
    public class CounterPageJson : IPage
    {
        public CounterData2 data { get; set; }

        public Dictionary<string, JToken> DeclareUI()
        {
            var json = @"{
    'tag': 'div',
    'class': 'columns',
    'children': [
        {
            'tag': 'span',
            ':text': 'data.Counter'
        },
        {
            'tag': 'br',
        },
        {
            'tag': 'button',
            'text': 'Click me',
            '@click': 'OnCliked'
        },
        {
            'tag': 'br',
        },
        {
            'tag': 'span',
            'text': 'Simple text',
        },
        {
            'tag': 'br',
        },
        {
            'tag': 'input',
            'placeholder': 'Test',
            '#value': 'data.SCounter',
            '@change': 'OnChanged'
        }
    ]
}";

            JObject jo = JsonConvert.DeserializeObject<JObject>(json);

            Dictionary<string, JToken> jsonDictionary = jo.ToObject<Dictionary<string, JToken>>();

            return jsonDictionary;
        }

        public Task OnGet()
        {
            data = new CounterData2();

            BulmaLoader.AppendTo(LaraUI.Page.Document.Head);

            RaElementFactory.CreateUI(LaraUI.Page.Document.Body, DeclareUI(), LaraUI.Page.Document.Body);

            return Task.CompletedTask;
        }

        public Task OnCliked(Element el)
        {
            data.Counter++;
            el.InnerText = $"Clicked {data.Counter} times";
            return Task.CompletedTask;
        }

        public Task OnChanged(Element el)
        {
            return Task.CompletedTask;
        }
    }

    public static class RaElementFactory
    {
        public static Element CreateUI(Element parentElement, Dictionary<string, JToken> jsonDictionary, Element component)
        {
            Element element = Element.Create(jsonDictionary["tag"].ToString());

            if (component != null)
            {
                component.EnsureElementId();
            }

            var parentElementId = component?.Id ?? parentElement.Id;

            parentElement.AppendChild(element);

            foreach (var pp in jsonDictionary)
            {
                if (pp.Key == "tag")
                {
                    continue;
                }
                else if (pp.Key.StartsWith(":")) // one bind
                {
                    var attribute = pp.Key.Replace(":", "");
                    var at_value = pp.Value.ToString().Split(".");

                    var DataName = at_value[0];
                    var PropertyName = at_value[1];

                    if (component is WebComponent)
                    {
                        RaBinder(component, DataName, PropertyName, element, attribute, false);
                    }
                    else
                    {
                        RaBinder(component.Document.Page, DataName, PropertyName, element, attribute, false);
                    }

                }
                else if (pp.Key.StartsWith("#")) //two bind
                {
                    var attribute = pp.Key.Replace("#", "");
                    var at_value = pp.Value.ToString().Split(".");

                    var DataName = at_value[0];
                    var PropertyName = at_value[1];

                    if (component is WebComponent)
                    {
                        RaBinder(component, DataName, PropertyName, element, attribute, true);
                    }
                    else
                    {
                        RaBinder(component.Document.Page, DataName, PropertyName, element, attribute, true);
                    }
                }
                else if (pp.Key.StartsWith("@")) //bind event
                {
                    var attribute = pp.Key.Replace("@", "");
                    var at_value = pp.Value.ToString();

                    MethodInfo methodInfo = null;
                    if (component is WebComponent)
                    {
                        methodInfo = component.GetType().GetMethod(at_value);
                    }
                    else
                    {
                        methodInfo = component.Document.Page.GetType().GetMethod(at_value);
                    }
                    element.On(attribute, () =>
                    {
                        if (component is WebComponent)
                        {
                            methodInfo?.Invoke(component, new object[] { element });
                        }
                        else
                        {
                            methodInfo?.Invoke(component.Document.Page, new object[] { element });
                        }
                        return Task.CompletedTask;
                    });
                }
                else if (pp.Key == "text")
                {
                    element.InnerText = pp.Value.ToString();
                }
                else if (pp.Key == "children")
                {
                    foreach (var jsonElement in jsonDictionary[pp.Key])
                    {
                        Dictionary<string, JToken> rs = jsonElement.ToObject<Dictionary<string, JToken>>();
                        CreateUI(element, rs, component);
                    }
                }
                else
                {
                    element.SetAttribute(pp.Key, pp.Value.ToString());
                }
            }

            //foreach (var child in element.Children)
            //{
            //    //_element.AppendChild(CreateUI(_element, (Element)child, component));
            //    CreateUI(element, (Element)child, component);
            //}

            return element;
        }

        public static void RaBinder(object component, string DataName, string PropertyName, Element _element, string attribute, bool twoWayBind)
        {
            var _DataProperty = component.GetType().GetProperty(DataName);
            var _DataType = _DataProperty.PropertyType;
            var _Property = _DataType.GetProperty(PropertyName);

            if (!twoWayBind)
            {
                BindableBase _DataObject = (BindableBase)_DataProperty.GetValue(component, null);

                _DataObject.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == PropertyName)
                    {
                        string _PropertyValue = _Property.GetValue(_DataObject, null).ToString();

                        if (attribute == "text" || attribute == "innerText" || attribute == "html")
                        {
                            _element.InnerText = _PropertyValue;
                        }
                        else
                        {
                            //_element.RemoveAttribute(key);
                            _element.SetAttribute(attribute, _PropertyValue);
                        }
                    }
                };
            }
            if (twoWayBind)
            {
                var _DataObjectValue = _DataProperty.GetValue(component, null);

                var parameter = Expression.Parameter(_DataProperty.PropertyType, "bb");
                var comparison = Expression.Property(parameter, _Property);
                var tt = Expression.Lambda(comparison, parameter);

                Type d1 = typeof(BindInputOptions<>);
                Type[] typeArgs = { _DataProperty.PropertyType };
                Type constructed = d1.MakeGenericType(typeArgs);

                object _BindInputOptions = Activator.CreateInstance(constructed);
                _BindInputOptions.GetType().GetProperty("Attribute").SetValue(_BindInputOptions, attribute);
                _BindInputOptions.GetType().GetProperty("BindObject").SetValue(_BindInputOptions, _DataObjectValue);
                _BindInputOptions.GetType().GetProperty("Property").SetValue(_BindInputOptions, tt);

                var pp = _element.GetType().GetMethod("BindInput");

                var pp_g = pp.MakeGenericMethod(typeArgs);

                pp_g?.Invoke(_element, new object[] { _BindInputOptions });
            }
        }
    }

    public class CounterData2 : BindableBase
    {
        private int _counter;

        public int Counter
        {
            get => _counter;
            set => SetProperty(ref _counter, value);
        }

        public string SCounter
        {
            get => _counter.ToString();
            set { Counter = int.Parse(value); }
        }
    }

}
