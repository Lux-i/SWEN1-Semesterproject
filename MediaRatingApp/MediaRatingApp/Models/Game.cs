using MediaRatingApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    class Game
    {
        List<string> Platforms;
        List<string> Available; // e.g., Steam, Epic Games, etc.
        string Developer;
        string Publisher;
        GameStatus Status;
    }
}
