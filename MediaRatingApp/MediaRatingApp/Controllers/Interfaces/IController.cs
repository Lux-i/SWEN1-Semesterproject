using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Routing;

namespace MediaRatingApp.Controllers.Interfaces
{
    public interface IController
    {
        Router BuildRouter();
    }
}
