using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    //Authorize -  returning our users, make sure that anybody that accesses any methods in here is authorized before we return them any details about users
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
        public async Task<IActionResult> getUsers()
        {
            var users = await _repo.GetUsers();

            //mapper we want to use UserForListDto.cs getters/setters instead of User.cs
            var userToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

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


    }
}