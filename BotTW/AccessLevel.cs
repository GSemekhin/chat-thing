using System;
using System.Collections.Generic;
using System.Text;

namespace BotTW
{

    [Flags]
    public enum Role
    {
        None = 0,
        Admin = 2,
        Owner = 4,
        Moderator = 8,
        TrustedUser = 16,
        PayingUser = 32,
        User = 64,
        NewUser = 128,
        Any = 256
    }

    class AccessLevel
    {

        public AccessLevel()
        {


        }

        public Role GetRole(string username)
        {
            Role role = Role.None;
            //me
            if (username.ToLower() == "i_am_d0br0")
            {
                role = Role.Admin | Role.Any | Role.Moderator | Role.NewUser | Role.PayingUser | Role.TrustedUser | Role.User | Role.Owner;
            }
            else if (username.ToLower() == "boouche")
            {
                role = Role.Admin | Role.Any | Role.Moderator | Role.NewUser | Role.PayingUser | Role.TrustedUser | Role.User | Role.Owner;
            }
            else
            {
                role = Role.Any;
            }

            return role;
        }

    }
}
