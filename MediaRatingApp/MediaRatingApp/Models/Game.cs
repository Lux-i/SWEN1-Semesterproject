using MediaRatingApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class Game : Media
    {
        public List<string> Platforms;
        public List<string> Available; // e.g., Steam, Epic Games, etc.
        public string Developer;
        public string Publisher;
        public GameStatus Status;

        public Game()
        {
            Platforms = new List<string>();
            Available = new List<string>();
            string Developer = "";
            string Publisher = "";
            Status = GameStatus.Development;
        }
    }
}
