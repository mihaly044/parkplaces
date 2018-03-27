using System;
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

        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// User id
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Holds the Id of the user who has had created 
        /// this one
        /// </summary>
        public int CreatorId;

        /// <summary>
        /// Indicates which group the user belong to
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
        /// Indicates it is a newly created user
        /// </summary>
        public bool IsNewUser => Id == 0;

        /// <summary>
        /// Holds the user's password.
        /// Shall only be used for editing
        /// </summary>
        public string Password { get; internal set; }

        public override string ToString()
        {
            return $"{UserName} ({GroupRole})";
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
    }
}
