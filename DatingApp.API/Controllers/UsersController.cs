using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    //Authorize -  returning our users, make sure that anybody that accesses any methods in here is authorized before we return them any details about users
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // bring in the repository
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> getUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            //mapper we want to use UserForListDto.cs getters/setters instead of User.cs
            var userToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(userToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> getUser(int id)
        {
            var user = await _repo.GetUser(id);

            //mapper we want to use UserForDetailedDto.cs getters/setters instead of User.cs
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        // this method checks to see if the user is the current user thats passed the token up to the server thats attempting to access this route
        // http put is used for updating a resourcein an API
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto) 
        {
            // we need to compare the id of the path to the users id thats being passed in as part of their token
            // if the user id doesnt match the id in the token then an unauthrorized is given
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized();
            }

            // get user from repo
            var userFromRepo = await _repo.GetUser(id);

            // then we need to take the information thats in our userForUpdateDto and map it into userFromRepo
            // this is going to execute the mapping and effectively updates the values from userForUpdateDto and write them into our user from repo
            _mapper.Map(userForUpdateDto, userFromRepo);

            // save our changes, and if this is successful then what we'll return from a update method is a no content
            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {id} failed on save");
        }

        // recipient Id is the id the user likes
        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            // we need to compare the id of the path to the users id thats being passed in as part of their token
            // if the user id doesnt match the id in the token then an unauthrorized is given
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized();
            }

            // attempt to get a like from the repo
            var like = await _repo.GetLike(id, recipientId);

            // if like is not null, this means theres already a like in place for this user
            if (like != null) 
                return BadRequest("You already liked this user");

            // check if recipient of the like exists
            if (await _repo.GetUser(recipientId) == null) 
                return NotFound();

            like = new Like 
            {
                LikerId = id,
                LikeeId = recipientId
            };

            // synchronous call, stored in memory (our repo), not in database yet
            _repo.Add<Like>(like);

            // asynchronous, saving to database
            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");



        }

    }
}