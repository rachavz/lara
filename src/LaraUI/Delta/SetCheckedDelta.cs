﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System.Runtime.Serialization;

namespace Integrative.Lara
{
    [DataContract]
    internal sealed class SetCheckedDelta : BaseDelta
    {
        [DataMember]
        public string ElementId { get; set; } = string.Empty;

        [DataMember]
        public bool Checked { get; set; }

        public SetCheckedDelta() : base(DeltaType.SetChecked)
        {
        }

        public static void Enqueue(Element element, bool value)
        {
            if (element.TryGetQueue(out var document))
            {
                document.Enqueue(new SetCheckedDelta
                {
                    ElementId = element.Id,
                    Checked = value
                });
            }
        }
    }
}
