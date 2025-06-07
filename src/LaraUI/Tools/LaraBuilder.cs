﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Integrative.Lara
{
    /// <summary>
    /// Class to build pages more easily
    /// </summary>
    public sealed class LaraBuilder
    {
        #region Constructor

        private readonly Stack<Element> _stack;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaraBuilder"/> class.
        /// </summary>
        /// <param name="startingElement">The starting element to build on.</param>
        public LaraBuilder(Element startingElement)
        {
            startingElement = startingElement ?? throw new ArgumentNullException(nameof(startingElement));
            _stack = new Stack<Element>();
            _stack.Push(startingElement);
        }

        #endregion

        #region Pushing and popping elements

        /// <summary>
        /// Adds a new element and positions the builder into it.
        /// </summary>
        /// <param name="tagName">Element's tag name.</param>
        /// <param name="className">Element's class name</param>
        /// <param name="id">element's identifier.</param>
        /// <returns>This instance</returns>
        public LaraBuilder Push(string tagName, string? className = null, string? id = null)
        {
            return Push(tagName, className, id, out _);
        }

        /// <summary>
        /// Adds a new element and positions the builder into it.
        /// </summary>
        /// <param name="tagName">Element's tag name.</param>
        /// <param name="className">Element's class name</param>
        /// <param name="id">The identifier.</param>
        /// <param name="added">The element added.</param>
        /// <returns>This instance</returns>
        public LaraBuilder Push(string tagName, string? className, string? id, out Element added)
        {
            added = Element.Create(tagName);
            if (!string.IsNullOrEmpty(id))
            {
                added.Id = id;
            }
            if (!string.IsNullOrEmpty(className))
            {
                added.Class = className;
            }
            return Push(added);
        }

        /// <summary>
        /// Adds a new namespace-specific element (e.g. 'svg') and positions the builder into it.
        /// </summary>
        /// <param name="ns">The namespace of the element.</param>
        /// <param name="tagName">The element's tag name</param>
        /// <returns>This instance</returns>
        // ReSharper disable once InconsistentNaming
        public LaraBuilder PushNS(string ns, string tagName)
        {
            var added = Element.CreateNS(ns, tagName);
            return Push(added);
        }

        /// <summary>
        /// Adds an element and positions the builder into it.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>This instance</returns>
        public LaraBuilder Push(Element element)
        {
            element = element ?? throw new ArgumentNullException(nameof(element));
            _stack.Push(element);
            return this;
        }

        /// <summary>
        /// Adds an element and positions the builder into it.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>This instance</returns>
        public LaraBuilder Push(Element element, string? className)
        {
            className = className ?? throw new ArgumentNullException(nameof(className));
            Push(element);
            element.Class = className;
            return this;
        }

        /// <summary>
        /// Positions the builder one position back on the element stack.
        /// </summary>
        /// <returns>This instance</returns>
        /// <exception cref="InvalidOperationException">Too many Pop() calls.</exception>
        public LaraBuilder Pop()
        {
            if (_stack.Count <= 1)
            {
                throw new InvalidOperationException(Resources.TooManyPops);
            }
            var pop = _stack.Pop();
            if (_stack.Count <= 0) return this;
            var current = _stack.Peek();
            current.AppendChild(pop);
            return this;
        }

        /// <summary>
        /// Returns the current element in the out variable
        /// </summary>
        /// <param name="element">Current element</param>
        /// <returns>This instance</returns>
        public LaraBuilder GetCurrent(out Element element)
        {
            element = _stack.Peek();
            return this;
        }

        #endregion

        #region Adding nodes

        /// <summary>
        /// Adds a text node.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="encode">if set to <c>true</c> [encode].</param>
        /// <returns>This instance</returns>
        [Obsolete("Use AppendText() or AppendData() instead of AddTextNode")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder AddTextNode(string text, bool encode = true)
        {
            return AddTextNode(new TextNode(text, encode));
        }

        /// <summary>
        /// Adds a text node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>This instance</returns>
        [Obsolete("Use AppendText() or AppendData() instead of AddTextNode")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder AddTextNode(TextNode node)
        {
            node = node ?? throw new ArgumentNullException(nameof(node));
            return AddNode(node);
        }

        /// <summary>
        /// Appends text to the current element
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <returns>This instance</returns>
        public LaraBuilder AppendText(string text)
        {
            return AppendEncode(text, true);
        }

        /// <summary>
        /// Appends raw HTML to the current element
        /// </summary>
        /// <param name="data">raw HTML</param>
        /// <returns>This instance</returns>
        public LaraBuilder AppendData(string data)
        {
            return AppendEncode(data, false);
        }

        private LaraBuilder AppendEncode(string value, bool encode)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _stack.Peek().AppendEncode(value, encode);
            }
            return this;
        }

        /// <summary>
        /// Clears all child elements and replaces them with a text node
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>This instance</returns>
        public LaraBuilder InnerText(string text)
        {
            return InnerEncode(text, true);
        }

        /// <summary>
        /// Clears all child elements and replaces them with a text node
        /// </summary>
        /// <param name="data">raw HTML code</param>
        /// <returns>This instance</returns>
        public LaraBuilder InnerData(string data)
        {
            return InnerEncode(data, false);
        }

        private LaraBuilder InnerEncode(string data, bool encode)
        {
            if (!string.IsNullOrEmpty(data))
            {
                _stack.Peek().SetInnerEncode(data, encode);
            }
            return this;
        }

        /// <summary>
        /// Adds a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>This instance</returns>
        public LaraBuilder AddNode(Node node)
        {
            node = node ?? throw new ArgumentNullException(nameof(node));
            var current = _stack.Peek();
            current.AppendChild(node);
            return this;
        }

        /// <summary>
        /// Adds a collection of nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>This instance</returns>
        public LaraBuilder AddNodes(IEnumerable<Node> nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            var current = _stack.Peek();
            foreach (var node in nodes)
            {
                current.AppendChild(node);
            }
            return this;
        }

        /// <summary>
        /// Adds a collection of elements.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>This instance</returns>
        public LaraBuilder AddNodes(IEnumerable<Element> nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            var current = _stack.Peek();
            foreach (var node in nodes)
            {
                current.AppendChild(node);
            }
            return this;
        }

        /// <summary>
        /// Adds the elements generated by the executing the specified handler.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>This instance</returns>
        public LaraBuilder Add(Action<LaraBuilder> action)
        {
            action = action ?? throw new ArgumentNullException(nameof(action));
            action(this);
            return this;
        }

        #endregion

        #region Current element attributes

        /// <summary>
        /// Sets an attribute on the current element.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value.</param>
        /// <returns>Thsi instance</returns>
        public LaraBuilder Attribute(string attribute, string value)
        {
            var current = _stack.Peek();
            current.SetAttribute(attribute, value);
            return this;
        }

        /// <summary>
        /// Creates an ID for the current element if it doesn't have one
        /// </summary>
        /// <returns>This instance</returns>
        [Obsolete("Not needed anymore")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder EnsureElementId()
        {
            return this;
        }

        /// <summary>
        /// Sets a flag attribute value.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>This instance</returns>
        public LaraBuilder FlagAttribute(string attribute, bool value)
        {
            var current = _stack.Peek();
            current.SetFlagAttribute(attribute, value);
            return this;
        }

        /// <summary>
        /// Adds a class to the current element
        /// </summary>
        /// <param name="className">class to add</param>
        /// <returns>This instance</returns>
        public LaraBuilder AddClass(string className)
        {
            _stack.Peek().AddClass(className);
            return this;
        }

        /// <summary>
        /// Removes a class from the current element
        /// </summary>
        /// <param name="className">Class to remove</param>
        /// <returns>This instance</returns>
        public LaraBuilder RemoveClass(string className)
        {
            _stack.Peek().RemoveClass(className);
            return this;
        }

        /// <summary>
        /// Adds or removes a class from the current element
        /// </summary>
        /// <param name="className">Class to toggle</param>
        /// <returns>This instamce</returns>
        public LaraBuilder ToggleClass(string className)
        {
            _stack.Peek().ToggleClass(className);
            return this;
        }

        /// <summary>
        /// Adds or removes a class from the current element
        /// </summary>
        /// <param name="className">Class to toggle</param>
        /// <param name="value">true to add, false to remove</param>
        /// <returns>This instance</returns>
        public LaraBuilder ToggleClass(string className, bool value)
        {
            _stack.Peek().ToggleClass(className, value);
            return this;
        }

        #endregion

        #region Current element events

        /// <summary>
        /// Associates an event handler with the current element.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>This instance</returns>
        public LaraBuilder On(EventSettings settings)
        {
            _stack.Peek().On(settings);
            return this;
        }

        /// <summary>
        /// Associates an event handler with the current element.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>This instance</returns>
        public LaraBuilder On(string eventName, Func<Task> handler)
        {
            _stack.Peek().On(eventName, handler);
            return this;
        }

        /// <summary>
        /// Associates an event handler with the current element.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="handler">Action to execute</param>
        /// <returns>This instance</returns>
        public LaraBuilder On(string eventName, Action handler)
        {
            _stack.Peek().On(eventName, () =>
            {
                handler();
                return Task.CompletedTask;
            });
            return this;
        }

        /// <summary>
        /// Listens to an event, only purpose to flush data from the client to the server.
        /// This forces the server to read input values on the client on the given event name.
        /// </summary>
        /// <param name="eventName">event to listen</param>
        /// <returns>This instance</returns>
        public LaraBuilder FlushEvent(string eventName)
        {
            return On(eventName, () => Task.CompletedTask);
        }

        #endregion

        #region Bindings

        /// <summary>
        /// Executes code whenever a source object triggers the PropertyChanged event
        /// </summary>
        /// <param name="source"></param>
        /// <param name="onSourceChange"></param>
        /// <returns></returns>
        public LaraBuilder Bind(INotifyPropertyChanged source, Action<Element> onSourceChange)
        {
            BindingExtensions.Bind(_stack.Peek(), source, onSourceChange);
            return this;
        }

        /// <summary>
        /// Executes code whenever the element triggers the PropertyChanged event
        /// </summary>
        /// <param name="onChange"></param>
        /// <returns></returns>
        public LaraBuilder BindBack(Action<Element> onChange)
        {
            BindingExtensions.BindBack(_stack.Peek(), onChange);
            return this;
        }

        /// <summary>
        /// Updates the element's children collection based on an observable collection
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="childFactory"></param>
        /// <returns></returns>
        public LaraBuilder BindChildren<TValue>(ObservableCollection<TValue> source, Func<TValue, Element> childFactory)
        {
            BindingExtensions.BindChildren(_stack.Peek(), source, childFactory);
            return this;
        }

        #endregion

        #region Current element bindings

        /// <summary>
        /// Adds bindings for an attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source's property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindAttribute<T>(string attribute, T instance, Func<string> property)
            where T : class, INotifyPropertyChanged
        {
            return BindAttribute(new BindAttributeOptions<T>
            {
                Attribute = attribute,
                BindObject = instance,
                Property = _ => property()
            });
        }

        /// <summary>
        /// Adds bindings for a flag attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete("Use BindToggleAttribute() instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindFlagAttribute<T>(string attribute, T instance, Func<bool> property)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleAttribute(attribute, instance, property);
        }

        /// <summary>
        /// Adds bindings for a flag attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindToggleAttribute<T>(string attribute, T instance, Func<bool> property)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleAttribute(new BindFlagAttributeOptions<T>
            {
                Attribute = attribute,
                BindObject = instance,
                Property = _ => property()
            });
        }

        /// <summary>
        /// Adds bindings for toggling an element class
        /// </summary>
        /// <typeparam name="T">Data type for the data source</typeparam>
        /// <param name="className">Class name</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindToggleClass<T>(string className, T instance, Func<bool> property)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleClass(new BindToggleClassOptions<T>
            {
                ClassName = className,
                BindObject = instance,
                Property = _ => property()
            });
        }

        /// <summary>
        /// Adds bindings for an attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindAttribute<T>(BindAttributeOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().BindAttribute(options);
            return this;
        }

        /// <summary>
        /// Adds bindings for a flag attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete("Use BindToggleAttribute() instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindFlagAttribute<T>(BindFlagAttributeOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleAttribute(options);
        }

        /// <summary>
        /// Adds bindings for a flag attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindToggleAttribute<T>(BindFlagAttributeOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().BindToggleAttribute(options);
            return this;
        }

        /// <summary>
        /// Adds bindings for toggling classes
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindToggleClass<T>(BindToggleClassOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().BindToggleClass(options);
            return this;
        }

        /// <summary>
        /// Adds bindings for an attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source's property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindAttribute<T>(string attribute, T instance, Func<T, string> property)
            where T : class, INotifyPropertyChanged
        {
            return BindAttribute(new BindAttributeOptions<T>
            {
                Attribute = attribute,
                BindObject = instance,
                Property = property
            });
        }

        /// <summary>
        /// Adds bindings for a flag attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete("Use instad the BindToggleAttribute() method.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindFlagAttribute<T>(string attribute, T instance, Func<T, bool> property)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleAttribute(attribute, instance, property);
        }

        /// <summary>
        /// Adds bindings for a flag attribute
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindToggleAttribute<T>(string attribute, T instance, Func<T, bool> property)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleAttribute(new BindFlagAttributeOptions<T>
            {
                Attribute = attribute,
                BindObject = instance,
                Property = property
            });
        }

        /// <summary>
        /// Adds bindings for toggling a class
        /// </summary>
        /// <typeparam name="T">Data type for data source instance</typeparam>
        /// <param name="className">Class name</param>
        /// <param name="instance">Data source instance</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindToggleClass<T>(string className, T instance, Func<T, bool> property)
            where T : class, INotifyPropertyChanged
        {
            return BindToggleClass(new BindToggleClassOptions<T>
            {
                ClassName = className,
                BindObject = instance,
                Property = property
            });
        }

        /// <summary>
        /// Adds bindings for inner text
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="instance">Data source</param>
        /// <param name="property">Data source's property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindInnerText<T>(T instance, Func<string> property)
            where T : class, INotifyPropertyChanged
        {
            return BindInnerText(new BindInnerTextOptions<T>
            {
                BindObject = instance,
                Property = _ => property()
            });
        }

        /// <summary>
        /// Adds bindings for inner text
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="instance">Data source</param>
        /// <param name="property">Data source's property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindInnerText<T>(T instance, Func<T, string> property)
            where T : class, INotifyPropertyChanged
        {
            return BindInnerText(new BindInnerTextOptions<T>
            {
                BindObject = instance,
                Property = property
            });
        }

        /// <summary>
        /// Adds bindings for inner text
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="options">Inner tetx binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindInnerText<T>(BindInnerTextOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().BindInnerText(options);
            return this;
        }

        /// <summary>
        /// Adds bindings for the children collection
        /// </summary>
        /// <typeparam name="T">Type of elements in the ovservable collection</typeparam>
        /// <param name="collection">Observable collection</param>
        /// <param name="creator">Handler to create elements</param>
        /// <returns>This instance</returns>
        public LaraBuilder BindChildren<T>(ObservableCollection<T> collection, Func<Element> creator)
        {
            BindingExtensions.BindChildren(_stack.Peek(), collection, _ => creator());
            return this;
        }

        /// <summary>
        /// Adds bindings for the children collection
        /// </summary>
        /// <typeparam name="T">Type of elements in observable collection</typeparam>
        /// <param name="options">Children bindings options</param>
        /// <returns>This instance</returns>
        [Obsolete("Use BindChildren(source, factory) instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindChildren<T>(BindChildrenOptions<T> options)
        {
            _stack.Peek().BindChildren(options);
            return this;
        }

        /// <summary>
        /// Adds a two-way binding for an attribute (e.g. 'value' attribute)
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindInput<T>(BindInputOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().BindInput(options);
            return this;
        }

        /// <summary>
        /// Adds a two-way binding for an attribute (e.g. 'value' attribute)
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="data">Data source</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindInput<T>(string attribute, T data, Expression<Func<T, string?>> property)
            where T : class, INotifyPropertyChanged
        {
            return BindInput(new BindInputOptions<T>
            {
                Attribute = attribute,
                BindObject = data,
                Property = property
            });
        }

        /// <summary>
        /// Adds a two-way binding for a flag attribute (e.g. 'checked' attribute)
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindFlagInput<T>(BindFlagInputOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().BindFlagInput(options);
            return this;
        }

        /// <summary>
        /// Adds a two-way binding for a flag attribute (e.g. 'checked' attribute)
        /// </summary>
        /// <typeparam name="T">Type of data source</typeparam>
        /// <param name="attribute">Attribute</param>
        /// <param name="data">Data source</param>
        /// <param name="property">Data source property</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder BindFlagInput<T>(string attribute, T data, Expression<Func<T, bool>> property)
            where T : class, INotifyPropertyChanged
        {
            return BindFlagInput(new BindFlagInputOptions<T>
            {
                Attribute = attribute,
                BindObject = data,
                Property = property
            });
        }

        /// <summary>
        /// Associates the current element with a data source and an action to update the element whenever the source is modified
        /// </summary>
        /// <typeparam name="T">Type of the data source</typeparam>
        /// <param name="instance">Data source</param>
        /// <param name="action">Action to update the element</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder Bind<T>(T instance, Action action)
            where T : class, INotifyPropertyChanged
        {
            return Bind(new BindHandlerOptions<T>
            {
                ModifiedHandler = (_, _) => action(),
                BindObject = instance
            });
        }

        /// <summary>
        /// Associates the current element with a data source and an action to update the element whenever the source is modified
        /// </summary>
        /// <typeparam name="T">Type of the data source</typeparam>
        /// <param name="instance">Data source</param>
        /// <param name="action">Action to update the element</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder Bind<T>(T instance, Action<T, Element> action)
            where T : class, INotifyPropertyChanged
        {
            return Bind(new BindHandlerOptions<T>
            {
                ModifiedHandler = action,
                BindObject = instance
            });
        }

        /// <summary>
        /// Associates the current element with a data source and an action to update the element whenever the source is modified
        /// </summary>
        /// <typeparam name="T">Type of the data source</typeparam>
        /// <param name="options">Binding options</param>
        /// <returns>This instance</returns>
        [Obsolete(ObsoleteElement.BindObsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LaraBuilder Bind<T>(BindHandlerOptions<T> options)
            where T : class, INotifyPropertyChanged
        {
            _stack.Peek().Bind(options);
            return this;
        }

        #endregion
    }
}
