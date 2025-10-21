using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class Movie : Media
    {
        public int Duration; // Duration in minutes
        public string Director;
        public List<string> Actors;
    }
}
