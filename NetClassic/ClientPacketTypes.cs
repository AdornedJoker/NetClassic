using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public enum ClientPacketTypes
    {
        PlayerIdentification = 0x00,
        SetBlock = 0x05,
        PositionOrientation = 0x08,
        Message = 0x0d
    }
}