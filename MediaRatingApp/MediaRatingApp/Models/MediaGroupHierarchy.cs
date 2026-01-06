using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatingApp.Models
{
    public class MediaGroupHierarchy
    {
        public int ParentGroupId { get; set; }
        public int ChildGroupId { get; set; }

        public MediaGroup? Parent;
        public MediaGroup? Child;
    }
}
