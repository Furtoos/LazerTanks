using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazerTanks
{
    enum GameState
    {
        GameServer,
        GameClient,
        Menu,
        ExpectationClient,
        ExpectationServer,
        ConnectedClient,
        Countdown
    }
}
