using System;
using System.Text;
using SuperSimpleTcp;

namespace NetClassic
{
    public enum ServerPacketTypes
    {
        Ping = 0x01,
        LevelInitialize = 0x02,
        LevelDataChunk = 0x03,
        LevelFinalize = 0x04,
        SetBlock = 0x06,
        SpawnPlayer = 0x07,
        PlayerTeleport = 0x08,
        PositionAndOrientation = 0x09,
        PositionUpdate = 0x0a,
        OrientationUpdate = 0x0b,
        DespawnPlayer = 0x0c,
        Message = 0x0d,
        DisconnectPlayer = 0x0e,
        UpdateUserType = 0x0f,
    }
}