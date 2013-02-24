﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace MOUSE.Core
{
    [DataContract]
    public abstract class MessageHeader
    {
        public static MessageHeader Deserialize(BinaryReader reader)
        {
            byte headerId = reader.ReadByte();
            switch (headerId)
            {
                case 1: return new TransportHeader(reader);
                case 2: return new OperationHeader(reader);
                case 3: return new ActorHeader(reader);
                default: throw new Exception("Not supported header id:" + headerId);
            }
        }

        public static void Serialize(MessageHeader header, BinaryWriter writer)
        {
            if      (header is TransportHeader) writer.Write((byte)1);
            else if (header is OperationHeader) writer.Write((byte)2);
            else if (header is ActorHeader) writer.Write((byte)3);
            else
                throw new Exception("Not supported header id:" + header);

            header.Serialize(writer);
        }

        public abstract void Serialize(BinaryWriter writer);
    }

    [DataContract]
    public class TransportHeader : MessageHeader
    {
        [DataMember]
        public readonly ulong SourceNodeId;
        [DataMember]
        public readonly ulong? RoutedNodeId;

        public TransportHeader(ulong sourceNodeId, ulong? routedNodeId)
        {
            SourceNodeId = sourceNodeId;
            RoutedNodeId = routedNodeId;
        }

        public TransportHeader(BinaryReader reader)
        {
            SourceNodeId = reader.ReadUInt64();
            bool hasValue = reader.ReadBoolean();
            if (hasValue)
                RoutedNodeId = reader.ReadUInt64();
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(SourceNodeId);
            writer.Write(RoutedNodeId.HasValue);
            if (RoutedNodeId.HasValue)
                writer.Write(RoutedNodeId.Value);
        }
    }

    [DataContract]
    public class OperationHeader : MessageHeader
    {
        [DataMember]
        public readonly int RequestId;
        [DataMember]
        public readonly OperationType Type;
        
        public OperationHeader(int requestId, OperationType type)
        {
            RequestId = requestId;
            Type = type;
        }

        public OperationHeader(BinaryReader reader)
        {
            RequestId = reader.ReadInt32();
            Type = (OperationType)reader.ReadByte();
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(RequestId);
            writer.Write((byte)Type);
        }
    }

    public enum OperationType
    {
        Request,
        Reply
    }

    
    [DataContract]
    public class ActorHeader : MessageHeader
    {
        [DataMember]
        public readonly uint ActorLocalId;

        public ActorHeader(uint actorLocalId)
        {
            ActorLocalId = actorLocalId;
        }

        public ActorHeader(BinaryReader reader)
        {
            ActorLocalId = reader.ReadUInt32(); ;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ActorLocalId);
        }
    }
}