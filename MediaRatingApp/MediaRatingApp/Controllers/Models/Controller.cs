using MediaRatingApp.Controllers.Interfaces;
using WebServer.Routing;

namespace MediaRatingApp.Controllers.Models
{
    public abstract class Controller : IController
    {
        protected readonly Router Router = new();

        // Configure() functions are virtual and not abstract, so child classes to not need to implement empty overrides if they do not need one.
        public virtual void ConfigureMiddlewares() { }

        public virtual void ConfigureRoutes() { }

        public virtual void ConfigureSubRouters() { }

        public Router BuildRouter()
        {
            ConfigureMiddlewares();
            ConfigureRoutes();
            ConfigureSubRouters();
            return Router;
        }
    }
}
