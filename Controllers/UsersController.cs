using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models;


namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly ILogger _logger;

        public UsersController(TodoContext context, ILogger<UsersController> logger)
        {
            _context = context;

            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<User>> GetUser()
        {
          if (_context.Users == null)
          {
              return NotFound();
          } 
            var headers = Request.Headers.Authorization.ToString().Split(' ', '\t')[1];
            _logger.LogInformation("headers {DT} ",  headers);
            var jwtreader = new JwtSecurityTokenHandler();
            var token = jwtreader.ReadJwtToken(headers).Claims.ElementAt(0).ToString().Split(' ', '\t')[1];
              _logger.LogInformation("Token data {DT}",  token);
              
            var usuario = await _context.Users.Where(b => b.Email == token).FirstOrDefaultAsync();
            
            if (usuario == null)
            {
                return NotFound();
            }
              var resObject =  new ResponseUserDTO {Name = usuario.Name, Descripcion = usuario.Descripcion, Username = usuario.Username, ProfilePic = usuario.ProfilePic };
                 

            return Ok(resObject);
        }

        [HttpPost("profilePic")]
        public async Task<ActionResult<User>> AddProfilePic([FromForm] ProfilePicDTO profile){
            if (_context.Users == null) {
                return NotFound();
            }
            var headers = Request.Headers.Authorization.ToString().Split(' ', '\t')[1];
            _logger.LogInformation("headers {DT} ",  headers);
            var jwtreader = new JwtSecurityTokenHandler();
            var token = jwtreader.ReadJwtToken(headers).Claims.ElementAt(0).ToString().Split(' ', '\t')[1];
            _logger.LogInformation("Token data {DT}",  token);
           
            var usuario = await _context.Users.Where(b => b.Email == token).FirstOrDefaultAsync();

             if (usuario == null || profile.FormFile == null)
            {
                return NotFound();
            }
            
                
                var file = profile.FormFile;
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                
                fileName =  randonNumber() + fileName ;
                
                var fullPath = Path.Combine(pathToSave, fileName);

                //var dbPath = Path.Combine(folderName, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                
                usuario.ProfilePic  = fileName;
                usuario.Descripcion = profile.Descripcion;
                await _context.SaveChangesAsync();

            return Ok();

        }
        // GET: api/Users/5
        /*
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }*/

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        */
        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        [HttpPost("login")]
        public async Task<ActionResult<User>> GetToken(LoginDTO usuario){
        if (_context.Users == null)
          {
              return Problem("Entity set 'TodoContext.Users'  is null.");
          }
             var user = await _context.Users.Where(b => b.Email == usuario.email).FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest("Invalid user request!!!");
            }
             if( ! (usuario.password == user.Password)){
               
                return Unauthorized();
            }   
                var claims = new List<Claim>(); 
                var claim = new Claim("email", user.Email.ToString());
                claims.Add(claim);
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(6),
                    signingCredentials: signinCredentials
                );
                
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new JWTTokenResponse { Token = tokenString });
        }
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserDTO usuario)
        {
          if (_context.Users == null)
          {
              return Problem("Entity set 'TodoContext.Users'  is null.");
          }
            
            var user  = new User{ Name=usuario.Name, Email=usuario.Email, Password=usuario.Password, Username= usuario.Username };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }
/*
        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private String randonNumber(){

            var randomGenerator = new Random();
            var a = randomGenerator.Next(10000, 100000).ToString();

            return a;
        }
    }
}
