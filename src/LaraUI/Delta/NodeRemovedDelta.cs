﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System.Runtime.Serialization;

namespace Integrative.Lara
{
    [DataContract]
    internal sealed class NodeRemovedDelta : BaseDelta
    {
        [DataMember]
        public string ParentId { get; set; } = string.Empty;

        [DataMember]
        public int ChildIndex { get; set; }

        public NodeRemovedDelta() : base(DeltaType.Remove)
        {
        }

        public static void Enqueue(Element parent, int index)
        {
            if (parent.TryGetQueue(out var document))
            {
                document.Enqueue(new NodeRemovedDelta
                {
                    ParentId = parent.Id,
                    ChildIndex = index,
                });
            }
        }
    }
}
