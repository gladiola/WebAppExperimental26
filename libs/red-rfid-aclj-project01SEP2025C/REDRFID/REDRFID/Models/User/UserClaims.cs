using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Razor;

namespace REDRFID.Models.User
{
    public class UserClaims
    {

        /// <summary>
        /// Variable to hold user identification data from JWT claims for the class to work from.
        /// </summary>
        //public required IEnumerable<System.Security.Claims.Claim> Claims { get; set; }

        /// <summary>
        /// Value that represents a session from the sid claim.
        /// </summary>
        public required string Sid { get; set; }

        /// <summary>
        /// Represents a user's id from the oid claim.
        /// </summary>
        public required string Oid { get; set; }

        /// <summary>
        /// Represents a user's name from the name claim.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Represents a user's roles from the roles claim.
        /// </summary>
        public required string[] Roles { get; set; }

        /// <summary>
        /// Represents a user's email from the email claim.
        /// </summary>
        public required string Email { get; set; }


        /// <summary>
        /// Constructor to neutralize and load values from a User.Claims in Razor.
        /// Class to hold user identification data from JWT claims
        /// </summary>
        public UserClaims() {



        }



    }
}
