using MediaRatingApp.Controllers.Api;
using MediaRatingApp.Controllers.Models;
using WebServer.Routing;

namespace MediaRatingApp.Controllers
{
    class BaseController : Controller
    {
        private readonly ApiController _apiController;

        private readonly AuthController _authController;

        public BaseController(ApiController apiController, AuthController authController)
        {
            _apiController = apiController;
            _authController = authController;
        }

        public override void ConfigureSubRouters()
        {
            Router.Use("/api", _apiController.BuildRouter());
            Router.Use("/auth", _authController.BuildRouter());
        }
    }
}
