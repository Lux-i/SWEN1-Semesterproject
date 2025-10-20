using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class Movie : Media
    {
        int Duration; // Duration in minutes
        string Director;
        List<string> Actors;
    }
}
