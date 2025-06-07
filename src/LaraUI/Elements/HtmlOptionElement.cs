﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System;
using System.ComponentModel;

namespace Integrative.Lara
{
    /// <summary>
    /// Option element
    /// </summary>
    [Obsolete("Use HtmlOptionElement instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class OptionElement : HtmlOptionElement
    {
    }

    /// <summary>
    /// The 'option' HTML5 element.
    /// </summary>
    /// <seealso cref="Element" />
    public class HtmlOptionElement : Element
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlOptionElement"/> class.
        /// </summary>
        public HtmlOptionElement() : base("option")
        {
        }

        internal override void NotifyValue(ElementEventValue entry)
        {
            BeginUpdate();
            base.NotifyValue(entry);
            NotifySelected(entry.Checked);
            EndUpdate();
        }

        /// <summary>
        /// Gets or sets the 'disabled' HTML5 attribute.
        /// </summary>
        public bool Disabled
        {
            get => HasAttributeLower("disabled");
            set => SetFlagAttributeLower("disabled", value);
        }

        /// <summary>
        /// Gets or sets the 'label' HTML5 attribute.
        /// </summary>
        public string? Label
        {
            get => GetAttributeLower("label");
            set => SetAttributeLower("label", value);
        }

        /// <summary>
        /// Gets or sets the 'selected' HTML5 attribute.
        /// </summary>
        public bool Selected
        {
            get => HasAttributeLower("selected");
            set => SetFlagAttributeLower("selected", value);
        }

        /// <summary>
        /// Gets or sets the 'value' HTML5 attribute.
        /// </summary>
        public string? Value
        {
            get => GetAttributeLower("value");
            set => SetAttributeLower("value", value);
        }

        internal void NotifyAdded(string parentValue)
        {
            if (parentValue == Value)
            {
                Selected = true;
            }
        }
    }
}
