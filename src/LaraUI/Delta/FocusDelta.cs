﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System.Runtime.Serialization;

namespace Integrative.Lara
{
    [DataContract]
    internal sealed class FocusDelta : BaseDelta
    {
        [DataMember]
        public string ElementId { get; set; } = string.Empty;

        public FocusDelta() : base(DeltaType.Focus)
        {
        }

        public static void Enqueue(Element element)
        {
            element.Document?.Enqueue(new FocusDelta
            {
                ElementId = element.Id
            });
        }
    }
}
