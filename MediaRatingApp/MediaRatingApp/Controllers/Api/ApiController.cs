using MediaRatingApp.Controllers.Interfaces;
using MediaRatingApp.Controllers.Models;
using MediaRatingApp.Middleware;
using MediaRatingApp.Services.Interfaces;

namespace MediaRatingApp.Controllers.Api
{
    class ApiController : Controller
    {
        private readonly IReadOnlyDictionary<string, IApiVersionController> _versionControllers;

        private readonly IAuthenticationService _authService;

        public ApiController(
            IReadOnlyDictionary<string, IApiVersionController> versionControllers,
            IAuthenticationService authService
            )
        {
            _versionControllers = versionControllers;
            _authService = authService;
        }

        public override void ConfigureMiddlewares()
        {
            var authMw = AuthMiddleware.IsAuth(_authService);
            Router.Use(authMw);
        }

        public override void ConfigureSubRouters()
        {
            foreach (var (path, controller) in _versionControllers)
            {
                Router.Use(path, controller.BuildRouter());
            }
        }
    }
}
