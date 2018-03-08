using System;
using ParkPlaces.IO;

namespace ParkPlaces.Misc
{

    /// <summary>
    /// Access levels
    /// </summary>
    public enum GroupRole
    {
        None = 0,
        Guest = 1,
        User = 2,
        Editor = 3,
        Admin = 4
    }

    public class User
    {
        protected bool Equals(User other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// User id
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Indicates which group the user belongs to
        /// </summary>
        public GroupRole GroupRole;

        /// <summary>
        /// Contains an username
        /// </summary>
        public string UserName;

        /// <summary>
        /// Contains the time the user has last logged in
        /// </summary>
        public DateTime LastLogin;

        /// <summary>
        /// Indicates whether the user has got the appropriate rights
        /// to use the application (> Guest)
        /// </summary>
        public bool IsAuthenticated;

        /// <summary>
        /// Holds the user's password.
        /// Shall only be used for editing
        /// </summary>
        public string Password { get; internal set; }

        public static bool operator ==(User user1, User user2)
        {
            return user1.Id == user2.Id;
        }

        public static bool operator !=(User user1, User user2)
        {
            return user1.Id != user2.Id;
        }

        public override string ToString()
        {
            return UserName;
        }

        public User(string userName, int id)
        {
            UserName = userName;
            Id = id;
        }

        public User(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Attempts to log in an user and retuns an User type object.
        /// The GroupRole field will be None (0) if the user cannot be 
        /// logged in.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>An User type object</returns>
        public static User Login(string userName, string password)
        {
            var sql = new Sql();
            var groupRole = sql.AuthenticateUser(userName, password);

            // TODO: Specify an ID for the logged in user
            return new User(userName, 0)
            {
                GroupRole = groupRole,
                IsAuthenticated = groupRole > GroupRole.Guest
            };
        }
    }
}
