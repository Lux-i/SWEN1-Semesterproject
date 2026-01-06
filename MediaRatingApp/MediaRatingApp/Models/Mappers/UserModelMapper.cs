namespace MediaRatingApp.Models.Mappers
{
    public static class UserModelMapper
    {
        public static User ToUser(UserOut userOut)
        {
            User user = new User
            {
                Id = userOut.Id,
                Username = userOut.Username,
                CreatedAt = userOut.CreatedAt,
                Profile = userOut.Profile,
            };
            return user;
        }

        public static UserOut ToUserOut(User user)
        {
            UserOut userOut = new UserOut
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                Profile = user.Profile,
            };
            return userOut;
        }
    }
}
