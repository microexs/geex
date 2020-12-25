using System;
using System.Collections.Generic;
using System.Linq;

using CommonServiceLocator;

using Geex.Core.Authorization;
using Geex.Core.Users;
using Geex.Shared;
using Geex.Shared._ShouldMigrateToLib;
using Geex.Shared._ShouldMigrateToLib.Abstractions;
using Geex.Shared._ShouldMigrateToLib.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;

namespace Geex.Core.Authentication.Domain
{
    public class User : GeexEntity
    {
        /// <summary>
        ///     Gets or sets the username.
        /// </summary>
        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public UserClaim[] Claims { get; set; }
        [InverseSide] public Many<Org> Orgs { get; set; }

        [OwnerSide] public Many<Role> Roles { get; set; }
        public List<AppPermission> AuthorizedPermissions { get; set; }
        public User()
        {
            this.InitManyToMany(() => Roles, role => role.Users);
            this.InitManyToMany(() => Orgs, org => org.Users);
        }
        public User(string phoneOrEmail, string password, string? username = null)
        : this()
        {
            if (phoneOrEmail.IsValidEmail())
                Email = phoneOrEmail;
            else if (phoneOrEmail.IsValidPhoneNumber())
                PhoneNumber = phoneOrEmail;
            else
                throw new Exception("invalid input for phoneOrEmail");
            var passwordHasher = ServiceLocator.Current.GetService<IPasswordHasher<User>>();
            UserName = username ?? phoneOrEmail;
            CheckDuplicateUser();
            Password = passwordHasher.HashPassword(this, password);
        }


        private void CheckDuplicateUser()
        {
            var users = DB.Collection<User>().AsQueryable();
            if (users
                .Any(o => o.UserName == UserName || o.Email == Email || o.PhoneNumber == PhoneNumber))
                throw new UserFriendlyException("UserAlreadyExists");
        }

        public bool CheckPassword(string password)
        {
            var passwordHasher = ServiceLocator.Current.GetService<IPasswordHasher<User>>();
            return passwordHasher.VerifyHashedPassword(this, Password, password) == PasswordVerificationResult.Success;
        }
    }
}