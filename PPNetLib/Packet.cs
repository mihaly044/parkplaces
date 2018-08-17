using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using PPNetLib.Contracts;

namespace PPNetLib
{
    [ProtoContract]
    [ProtoInclude(10, typeof(LoginAck))]
    [ProtoInclude(11, typeof(LoginReq))]
    public class Packet
    {
        [ProtoMember(1)]
        public Protocols PacketID { get; set; }

        public Packet()
        {
            
        }
    }
}
