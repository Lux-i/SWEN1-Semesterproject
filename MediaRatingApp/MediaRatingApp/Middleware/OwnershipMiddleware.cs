using MediaRatingApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Models;

namespace MediaRatingApp.Middleware
{
    public static class OwnershipMiddleware
    {
        public static MiddlewareCallback IsMediaOwner(IMediaService mediaService, string idParamName)
        {
            return async (req, res, next) =>
            {
                // This middleware expects that AuthMiddleware.IsAuth() has already been executed.
                int? userId = req.CustomData.UserId;
                if(userId == null)
                {
                    res.Status(401).SendJson(new { error = "Unauthorized: User not logged in" });
                    return;
                }

                int mediaId;

                // This function does not know if the media ID is in the path or query parameters.
                // So it checks both.
                string? mediaIdStr = null;
                if(req.HasPathParam(idParamName))
                {
                    mediaIdStr = req.GetPathParam(idParamName);
                }
                else if(req.HasQueryParam(idParamName))
                {
                    mediaIdStr = req.GetQueryParam(idParamName);
                }
                else
                {
                    res.Status(400).SendJson(new { error = $"Bad Request: Missing media ID parameter '{idParamName}'" });
                    return;
                }

                if(!int.TryParse(mediaIdStr, out mediaId))
                {
                    res.Status(400).SendJson(new { error = "Bad Request: Invalid media ID format" });
                    return;
                }

                bool isOwner = await mediaService.IsOwner(mediaId, userId.Value);

                if (!isOwner)
                {
                    res.Status(403).SendJson(new { error = "Forbidden: User is not the owner of the media item" });
                    return;
                }

                await next();
            };
        }
    }
}
