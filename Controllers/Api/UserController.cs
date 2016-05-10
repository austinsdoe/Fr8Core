﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using AutoMapper;
using AutoMapper.Internal;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8Data.DataTransferObjects;
using Hub.Managers;
using Hub.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Utilities;

namespace HubWeb.Controllers
{
    [DockyardAuthorize]
    public class UserController : ApiController
    {
        private readonly IMappingEngine _mappingEngine;
        private readonly ISecurityServices _securityServices;

        public UserController()
        {
            _securityServices = ObjectFactory.GetInstance<ISecurityServices>();
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>();
        }

        #region API Endpoints 
        
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (_securityServices.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO)))
                {
                    Expression<Func<Fr8AccountDO, bool>> predicate = x => true;
                    return Ok(GetUsers(uow, predicate));
                }

                int? organizationId;
                if (_securityServices.UserHasPermission(PermissionType.ManageInternalUsers, nameof(Fr8AccountDO)))
                {
                    var currentUser = _securityServices.GetCurrentAccount(uow);
                    organizationId = currentUser.OrganizationId;

                    Expression<Func<Fr8AccountDO, bool>> predicate = x => x.OrganizationId == organizationId;
                    return Ok(GetUsers(uow, predicate));
                }

                //todo: show not authorized messsage in activityStream
                return Ok();
            }
        }

        [HttpGet]
        public IHttpActionResult GetProfiles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //for now only return profiles that are protected. Those are all default Fr8 core profiles
                var profiles = uow.ProfileRepository.GetQuery().Where(x => x.Protected).Select(x => new ProfileDTO() { Id = x.Id.ToString(), Name = x.Name });

                //only users with permission 'Manage Fr8 Users' need to be able to use 'Fr8 Administrator' profile
                if (!_securityServices.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO)))
                {
                    //remove from list that profile  
                    profiles = profiles.Where(x => x.Name != DefaultProfiles.Fr8Administrator);
                }

                return Ok(profiles.ToList());
            }
        }

        [DockyardAuthorize(Roles = Roles.Admin)]
        public IHttpActionResult Get(string id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.Id == id);
                var userDTO = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                userDTO.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository
                    .GetRoles(userDTO.Id).Select(r => r.Name).ToArray());
                return Ok(userDTO);
            }
        }

        //[Route("api/user/getCurrent")]
        [HttpGet]
        public IHttpActionResult GetCurrent()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.EmailAddress.Address == User.Identity.Name);
                var userDTO = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                userDTO.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(userDTO.Id).Select(r => r.Name).ToArray());
                return Ok(userDTO);
            }
        }
        //[Route("api/user/getUserData?id=")]
        [HttpGet]
        public IHttpActionResult GetUserData(string id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.Id == id);
                return Ok(new UserDTO { FirstName = user.FirstName, LastName = user.LastName });
            }
        }


        [HttpPost]
        public IHttpActionResult UpdatePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(oldPassword))
                throw new Exception("Old password is required.");
            if (!string.Equals(newPassword, confirmPassword, StringComparison.OrdinalIgnoreCase))
                throw new Exception("New password and confirm password did not match.");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.EmailAddress.Address == User.Identity.Name);

                Fr8Account fr8Account = new Fr8Account();
                if (fr8Account.IsValidHashedPassword(user, oldPassword))
                {
                    fr8Account.UpdatePassword(uow, user, newPassword);
                    uow.SaveChanges();
                }
                else
                    throw new Exception("Invalid current password.");
            }

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult UpdateUserProfile(string userId, Guid profileId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                bool hasChanged = false;
                var user = uow.UserRepository.FindOne(u => u.Id == userId);

                if (_securityServices.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO)))
                {
                    user.ProfileId = profileId;
                    uow.SaveChanges();
                    return Ok();
                }

                if (_securityServices.UserHasPermission(PermissionType.ManageInternalUsers, nameof(Fr8AccountDO)))
                {
                    //security check if user is from same organization
                    user.ProfileId = profileId;
                    hasChanged = true;
                }

                if(hasChanged)
                    uow.SaveChanges();
            }

            return Ok();
        }

        #endregion

        #region Helper Methods

        private List<UserDTO> GetUsers(IUnitOfWork uow, Expression<Func<Fr8AccountDO, bool>> predicate)
        {
            var users = uow.UserRepository.GetQuery().Where(predicate).ToList();
            return users.Select(user =>
            {
                var dto = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                dto.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(user.Id).Select(r => r.Name).ToArray());
                return dto;
            }).ToList();
        }

        public static string GetCallbackUrl(string providerName)
        {
            return GetCallbackUrl(providerName, Server.ServerUrl);
        }

        public static string GetCallbackUrl(string providerName, string serverUrl)
        {
            if (String.IsNullOrEmpty(serverUrl))
                throw new ArgumentException("Server Url is empty", "serverUrl");

            return String.Format("{0}{1}AuthCallback/IndexAsync", serverUrl.Replace("www.", ""), providerName);
        }

        public ICollection<IdentityUserRole> ConvertRoleStringToRoles(string selectedRole)
        {
            List<IdentityUserRole> userNewRoles = new List<IdentityUserRole>();
            string[] userRoles = { };
            switch (selectedRole)
            {
                case Roles.Admin:
                    userRoles = new[] { Roles.Admin, Roles.Booker, Roles.Customer };
                    break;
                case Roles.Booker:
                    userRoles = new[] { Roles.Booker, Roles.Customer };
                    break;
                case Roles.Customer:
                    userRoles = new[] { Roles.Customer };
                    break;
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.AspNetRolesRepository.GetQuery().Where(e => userRoles.Contains(e.Name))
                    .Each(e => userNewRoles.Add(new IdentityUserRole()
                    {
                        RoleId = e.Id
                    }));
            }
            return userNewRoles;
        }

        private string ConvertRolesToRoleString(String[] userRoles)
        {
            if (userRoles.Contains(Roles.Admin))
                return Roles.Admin;
            else if (userRoles.Contains(Roles.Booker))
                return Roles.Booker;
            else if (userRoles.Contains(Roles.Customer))
                return Roles.Customer;
            else
                return "";
        }

        //Update DockYardAccount Status from user details view valid states are "Active" and "Deleted"
        public void UpdateStatus(string userId, int status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO curDockyardAccount = uow.UserRepository.GetQuery().Where(user => user.Id == userId).FirstOrDefault();

                if (curDockyardAccount != null)
                {
                    curDockyardAccount.State = status;
                    uow.SaveChanges();
                }
            }
        }

        #endregion
    }
}