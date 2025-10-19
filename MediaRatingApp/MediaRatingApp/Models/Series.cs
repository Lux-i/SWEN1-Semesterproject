using MediaRatingApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    class Series
    {
        int Seasons;
        int Episodes; // Number of episodes across all seasons
        Dictionary<int, int> EpisodesPerSeason; // <Season, EpisodesCount>
        SeriesStatus Status;
    }
}
