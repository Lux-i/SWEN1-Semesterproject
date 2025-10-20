using MediaRatingApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Data.Repositories
{
    class MediaRepository
    {
    }

    public class FakeMediaRepository
    {
        private static ConcurrentDictionary<int, Media> _media = new();
        private static int _nextId = 1;

        public Task<Media?> GetByIdAsync(int id)
        {
            _media.TryGetValue(id, out var media);
            return Task.FromResult(media);
        }

        public Task<List<Media>> GetAllAsync()
        {
            return Task.FromResult(_media.Values.ToList());
        }

        public Task<int> CreateAsync(Media media)
        {
            int id = Interlocked.Increment(ref _nextId);
            media._Id = id;
            _media[id] = media;
            return Task.FromResult(id);
        }

        public Task<bool> UpdateAsync(Media media)
        {
            if (_media.ContainsKey(media._Id))
            {
                _media[media._Id] = media;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_media.TryRemove(id, out _));
        }
    }
}
