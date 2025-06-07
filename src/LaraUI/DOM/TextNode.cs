﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System.Text;
using System.Web;

namespace Integrative.Lara
{
    /// <summary>
    /// A text node.
    /// </summary>
    /// <seealso cref="Node" />
    public sealed class TextNode : Node
    {
        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public override NodeType NodeType => NodeType.Text;

        private string? _data;

        /// <summary>
        /// Gets or sets the data of the node. This property gets and sets the raw (unencoded) HTML text for the node.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public string? Data
        {
            get => _data;
            set
            {
                if (_data == value) return;
                _data = value;
                TextModifiedDelta.Enqueue(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextNode"/> class.
        /// </summary>
        public TextNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextNode"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="encode">if set to <c>true</c> encode.</param>
        public TextNode(string? data, bool encode = true)
        {
            if (encode)
            {
                SetEncodedText(data);
            }
            else
            {
                _data = data;
            }
        }

        /// <summary>
        /// Appends text to the node
        /// </summary>
        /// <param name="text">Text to append</param>
        public void AppendText(string text)
        {
            AppendEncode(text, true);
        }

        /// <summary>
        /// Appends raw HTML to the node
        /// </summary>
        /// <param name="data">raw HTML</param>
        public void AppendData(string data)
        {
            AppendEncode(data, false);
        }

        internal void AppendEncode(string? text, bool encode)
        {
            if (encode)
            {
                text = HttpUtility.HtmlEncode(text);
            }
            if (string.IsNullOrEmpty(_data))
            {
                _data = text;
            }
            else
            {
                _data += text;
            }
        }

        internal override ContentNode GetContentNode()
        {
            return new ContentTextNode
            {
                Data = _data,
            };
        }

        /// <summary>
        /// Sets text for the node.
        /// </summary>
        /// <param name="unencoded">The unencoded text to encode.</param>
        public void SetEncodedText(string? unencoded)
        {
            Data = HttpUtility.HtmlEncode(unencoded);
        }

        internal override string GetNodeInnerText()
        {
            return Data == null ? string.Empty : HttpUtility.HtmlDecode(Data);
        }

        internal override void SetNodeInnerText(string? value)
        {
            SetEncodedText(value);
        }

        internal override void AppendNodeInnerText(StringBuilder builder)
        {
            builder.Append(GetNodeInnerText());
        }
    }
}
