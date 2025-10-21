using MediaRatingApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class Series : Media
    {
        public int Seasons;
        public int Episodes; // Number of episodes across all seasons
        public Dictionary<int, int> EpisodesPerSeason; // <Season, EpisodesCount>
        public SeriesStatus Status;

        public Series()
        {
            Seasons = 0;
            Episodes = 0;
            EpisodesPerSeason = new Dictionary<int, int>();
            Status = SeriesStatus.Ongoing;
        }
    }
}
