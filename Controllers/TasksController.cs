using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TodoContext _context;

         private readonly ILogger _logger;
        public TasksController(TodoContext context, ILogger<TasksController> logger)
        {
            _context = context;

            _logger = logger;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResponseDTO>>> GetTarea()
        {
          if (_context.Tarea == null)
          {
              return NotFound();
          }
            var headers = Request.Headers.Authorization.ToString().Split(' ', '\t')[1];
            var jwtreader = new JwtSecurityTokenHandler();
            var token = jwtreader.ReadJwtToken(headers).Claims.ElementAt(0).ToString().Split(' ', '\t')[1];
            
            var usuario = await _context.Users.Include(b => b.Tareas).Where(b => b.Email == token).FirstOrDefaultAsync();
            if(usuario == null){
                return NotFound();
            }
            var tareas =  await _context.Tarea.Where(a => a.UserId == usuario.Id).ToListAsync();
            
            var resObject = from tarea in tareas select new ResponseDTO {Id = tarea.Id, Descripcion =tarea.Descripcion, Foto = tarea.Foto };
           
            
            return Ok(resObject);
            

        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tarea>> GetTarea(long id)
        {
          if (_context.Tarea == null)
          {
              return NotFound();
          }
            var tarea = await _context.Tarea.FindAsync(id);

            if (tarea == null)
            {
                return NotFound();
            }

            return tarea;
        }

        // PUT: api/Tasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTarea(long id, Tarea tarea)
        {
            if (id != tarea.Id)
            {
                return BadRequest();
            }

            _context.Entry(tarea).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TareaExists(id))
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

        // POST: api/Tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Tarea>> PostTarea([FromForm] TareaDTO tareaDTO)
        {
          if (_context.Tarea == null)
          {
              return Problem("Entity set 'TodoContext.Tarea'  is null.");
          }
            var tarea = new Tarea {  Descripcion = tareaDTO.Descripcion };
            var formCollection = await Request.ReadFormAsync();
            _logger.LogInformation("headers {DT} ",  formCollection.ToString());
            if (tareaDTO.FormFile != null)
            {
                var file = tareaDTO.FormFile;
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

                tarea.Foto = fileName;

            }
        

            //_context.Tarea.Add(tarea);
            
            
            var headers = Request.Headers.Authorization.ToString().Split(' ', '\t')[1];
            var jwtreader = new JwtSecurityTokenHandler();
            var token = jwtreader.ReadJwtToken(headers).Claims.ElementAt(0).ToString().Split(' ', '\t')[1];
            var user = await _context.Users.Where(b => b.Email == token).FirstOrDefaultAsync();

            if (user == null)
        {
            return NotFound();
        }
            //tarea.Foto = tareaDTO.FormFile != null ? tareaDTO.FormFile.FileName.ToString() : null  ;
            tarea.User = user;
            tarea.UserId = user.Id;
            _logger.LogInformation("headers {DT} ",  tarea.ToString());
            user.Tareas.Add(tarea);
            
            await _context.SaveChangesAsync();

            var tareaResponse = new ResponseDTO {Id = tarea.Id, Descripcion =tarea.Descripcion, Foto = tarea.Foto };

            return Ok(tareaResponse);
            //return CreatedAtAction("GetTarea", new { id = tarea.Id }, tarea);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarea(long id)
        {
            if (_context.Tarea == null)
            {
                return NotFound();
            }
            var tarea = await _context.Tarea.FindAsync(id);
            if (tarea == null)
            {
                return NotFound();
            }

            _context.Tarea.Remove(tarea);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TareaExists(long id)
        {
            return (_context.Tarea?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private String randonNumber(){

            var randomGenerator = new Random();
            var a = randomGenerator.Next(10000, 100000).ToString();

            return a;
        }
    }
}
