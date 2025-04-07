using System;
using System.Collections.Generic;

namespace GalaxyWiki.Core.Entities
{
    public class User
    {
        public virtual string GoogleSub { get; protected set; } = string.Empty;
        public virtual string Email { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;

        protected User() { }

        public User(string googleSub, string email, string displayName)
        {
            if (string.IsNullOrWhiteSpace(googleSub))
                throw new ArgumentException("Google Subject ID cannot be empty", nameof(googleSub));
            
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name cannot be empty", nameof(displayName));

            GoogleSub = googleSub;
            Email = email;
            DisplayName = displayName;
        }
    }
} 