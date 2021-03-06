﻿using ProtoBuf;
using System;

namespace PPNetLib.Prototypes
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

    [ProtoContract]
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
        [ProtoMember(1)]
        public readonly int Id;

        /// <summary>
        /// Holds the Id of the user who has had created 
        /// this one
        /// </summary>
        [ProtoMember(2)]
        public int CreatorId;

        /// <summary>
        /// Indicates which group the user belong to
        /// </summary>
        [ProtoMember(3)]
        public GroupRole GroupRole;

        /// <summary>
        /// Contains an username
        /// </summary>
        [ProtoMember(4)]
        public string UserName;

        /// <summary>
        /// Contains the time the user has last logged in
        /// </summary>
        [ProtoMember(5)]
        public DateTime LastLogin;

        /// <summary>
        /// Indicates whether the user has got the appropriate rights
        /// to use the application (> Guest)
        /// </summary>
        [ProtoMember(6)]
        public bool IsAuthenticated;

        /// <summary>
        /// Indicates it is a newly created user
        /// </summary>
        public bool IsNewUser => Id == 0;

        /// <summary>
        /// Holds the user's password.
        /// Shall only be used for editing
        /// </summary>
        [ProtoMember(7)]
        public string Password { get; set; }

        [ProtoMember(8)]
        public bool Monitor { get; set; }

        [ProtoMember(9)]
        public string IpPort { get; set; }

        public override string ToString()
        {
            return $"{UserName} ({GroupRole})";
        }

        public User()
        {
            Id = -1;
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
