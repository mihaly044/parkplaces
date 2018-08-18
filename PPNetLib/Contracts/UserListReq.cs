using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class UserListReq: Packet
    {
        public UserListReq()
        {
            PacketId = Protocols.USERLIST_REQ;
        }
    }
}
