﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quark_Backend.DAL;
using Quark_Backend.Entities;
using Quark_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Quark_Backend.Services;

namespace Quark_Backend.Controllers
{
    //[Authorize(Policy = "PermissionLevel1")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LoginController : Controller
    {
        private readonly QuarkDbContext _dbContext;
        private readonly ISecurityService _securityService;

        public LoginController(QuarkDbContext context, ISecurityService securityService)
        {
            _dbContext = context;
            _securityService = securityService;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            // Find the user by email in the database
            var _user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (_user == null)
            {
                return Unauthorized("Invalid email or password");
            }
            // Version for open password 
            if (model.Password == _user.Password)
            {
                string token = _securityService.GenerateToken(_user.Email, _user.PermissionLevel.ToString());
                var user = new
                {
                    _user.Email,
                    _user.FirstName,
                    _user.LastName,
                    _user.Username,
                    _user.SelfDescription,
                    _user.PictureUrl
                };
                var response = new 
                {
                    user, 
                    token
                };
                return Ok(response);
            }
            else
            {
                return Unauthorized("Invalid email or password");
            }
            // Version for hashed password
            //var hashedPasswordFromDatabase = user.Password; // Retrieve the hashed password from the database
            //bool passwordMatch = BCrypt.Net.BCrypt.Verify(model.Password, hashedPasswordFromDatabase);
            //if (!passwordMatch)
            //{
            //   return Unauthorized("Invalid email or password");
            //}
            //return Ok(GenerateToken(user.Email, user.PermissionLevel.ToString()));
        }
    }
}