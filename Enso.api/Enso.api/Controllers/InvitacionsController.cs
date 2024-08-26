using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Enso.dal.DBContext;
using Enso.api.Services;
using Newtonsoft.Json;
using Enso.api.Utility;
using Microsoft.Extensions.Configuration;

namespace Enso.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitacionsController : ControllerBase
    {
        private readonly Db_EnsoContext _context;
        private IConfiguration _config;
        private IEmailService _emailRepository;
        private MailController mailController;

        public InvitacionsController(Db_EnsoContext context, IEmailService emailRepository, IConfiguration config)
        {
            _context = context;
            _config = config;
            mailController = new MailController(emailRepository);
        }

        // GET: api/Invitacions
        //[HttpGet]
        //public IActionResult GetInvitacions([FromQuery] int? id = null, [FromQuery]string email = null, [FromQuery]int? idStatus = null)
        //{
        //    var res = (from inv in _context.Invitacions
        //               select new { inv.Id, inv.IdUserInvite, inv.Nombre, inv.Email, inv.FechaEnvio, inv.FechaAceptacion, inv.IdStatus}
        //             ); ;

        //    if (id.HasValue) { res = res.Where(r => r.Id == id); }
        //    if (email != null) { res = res.Where(r => r.Email == email); }
        //    if (idStatus.HasValue) { res = res.Where(r => r.IdStatus == idStatus); }

        //    return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = res });
        //}

        // GET: api/Invitacions/GetUserInvitacions        
        [Route("GetUserInvitacions")]
        [HttpGet("GetUserInvitacions")]        
        public IActionResult GetUserInvitacions([FromQuery] int idUser, [FromQuery] int? statusInvitacion)
        {
            var res = (from inv in _context.Invitacions                       
                       where inv.IdUserInvite == idUser
                       select new { inv.Id, inv.IdUserInvite, inv.Nombre, inv.Email, inv.FechaEnvio, inv.FechaAceptacion, inv.IdStatus }
                     ); ;

            if(statusInvitacion.HasValue) { res = res.Where(r => r.IdStatus == statusInvitacion); }

            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = res });
        }

        // GET: api/Invitacions/GetUserInvitacionsDetail
        [Route("GetUserInvitacionsDetail")]
        [HttpGet("GetUserInvitacionsDetail")]
        public IActionResult GetUserInvitacionsDetail([FromQuery] int? idUser=null)
        {
            //var res = (from inv in _context.Invitacions
            //           join user1 in _context.Usuarios on inv.Email equals user1.Email                      
            //           join inv2 in _context.Invitacions on user1.Id equals inv2.IdUserInvite                       
            //           join user2 in _context.Usuarios on inv.IdUserInvite equals user2.Id                       
            //           select new { inv.Id, inv.Nombre, inv.Email, user1, user2 }
            //    //select new { inv.Id, inv.Nombre, inv.Email, user1 = new { user1.Nombre, user1.Materno }, user2 = new { user2.Nombre, user2.Materno } }
            //    );

            var res = (from user1 in _context.Usuarios
                       join inv in _context.Invitacions on user1.Id equals inv.IdUserInvite
                       join invitado in _context.Usuarios on inv.Email equals invitado.Email
                       //select new { inv.Id, inv.IdUserInvite, inv.Nombre, inv.Email, inv.FechaEnvio, inv.FechaAceptacion, inv.IdStatus, user1, invitado}
                       select new { inv.Id, inv.IdUserInvite, inv.Nombre, inv.Email, inv.FechaEnvio, inv.FechaAceptacion, inv.IdStatus, invitado }
                );

            if (idUser.HasValue) { res = res.Where(r => r.IdUserInvite == idUser); }

            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = res });
        }

        // GET: api/Invitacions/GetUserInvitacionsDetailAll
        [Route("GetUserInvitacionsDetailAll")]
        [HttpGet("GetUserInvitacionsDetailAll")]        
        public IActionResult GetUserInvitacionsDetailAll([FromQuery] int? idUser=null, [FromQuery] int? statusInvitacion=null, [FromQuery] bool? verificado = null)
        {
            //var res = (from inv in _context.Invitacions
            //           join user1 in _context.Usuarios on inv.Email equals user1.Email into u1
            //           from subu1 in u1.DefaultIfEmpty()
            //           join inv2 in _context.Invitacions on subu1.Id equals inv2.IdUserInvite into iinv2
            //           from subinv2 in iinv2.DefaultIfEmpty()
            //           join user2 in _context.Usuarios on inv.IdUserInvite equals user2.Id into u2
            //           from subu2 in u2.DefaultIfEmpty()
            //           group new { inv, subu1, subu2 } by new { inv, subu1, subu2 } into grupo
            //           from result in grupo.DefaultIfEmpty()
            //           select new { result.inv, result.subu1, result.subu2 }
            //    //select new { inv.Id, inv.Nombre, inv.Email, subu1, subu2 }                
            //    ); ;

            var res = (from inv in _context.Invitacions
                       join user1 in _context.Usuarios on inv.IdUserInvite equals user1.Id into u1
                       from subu1 in u1.DefaultIfEmpty()
                       join user2 in _context.Usuarios on inv.Email equals user2.Email into u2
                       from invitado in u2.DefaultIfEmpty()
                       //select new { inv.Id, inv.IdUserInvite, inv.Nombre, inv.Email, inv.FechaEnvio, inv.FechaAceptacion, inv.IdStatus, subu1, invitado }
                       select new { inv.Id, inv.IdUserInvite, inv.Nombre, inv.Email, inv.FechaEnvio, inv.FechaAceptacion, inv.IdStatus, invitado }
                ); ;
            
            if (idUser.HasValue) { res = res.Where(r => r.IdUserInvite == idUser); }
            if (statusInvitacion.HasValue) { res = res.Where(r => r.IdStatus == statusInvitacion); }
            if (verificado.HasValue) { res = res.Where(r => r.invitado.Verificado == verificado); }

            res = res.OrderBy(r => r.invitado.FechaVerificacion);            
            // Consultar limite máximo por UsuarioPerfil, y aplicar o no re.Take()            
            //int MaxInvAcept = 100;
            //res = res.Take(MaxInvAcept);

            var jsonRes = JsonConvert.SerializeObject(res);
            var encryptedResponse = Protection.Encrypt(jsonRes, _config);
            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = encryptedResponse });
        }

        // POST: api/Invitacions/SentInvite
        //[Route("SentInvite")]
        //[HttpPost("SentInvite")]
        //public IActionResult PostInvitacion(Invitacion invitacion)
        //{
        //    /////////// VALIDAR CANTIDAD INVITACIONES POR ESE USUARIO NO SEA MAYOR AL MaxInvAcept  --- MaxInvAcept FROM  CVars (Database) 
        //    //int MaxInvAcept = _context.Cvars.FirstOrDefault().MaxInvAcept;
        //    //int countInvitesUser = _context.Invitacions.Count(r => r.IdUserInvite == invitacion.IdUserInvite);
        //    //int countInvitesAceptUser = _context.Invitacions.Count(r => r.IdUserInvite == invitacion.IdUserInvite && r.IdStatus == 1);
        //    //if (countInvitesUser >= MaxInvAcept) { return Ok(new { result = "Error", detalle = "Max Invitations limit reached", item = countInvitesUser }); }

        //    // VALIDAR NO EXISTA UNA INVITACIÓN CON IdUserInvite Y EMAIL PREVIAMENTE            
        //    var res = _context.Invitacions.FirstOrDefault(r => r.Email == invitacion.Email && r.IdUserInvite == invitacion.IdUserInvite);
        //    if (res != null) { return Ok(new { result = "Error", detalle = "You already invite this email", item = res }); }

        //    // ESTE EMAIL YA ESTA REGISTRADO (A UN USUARIO)
        //    var res2 = _context.Usuarios.FirstOrDefault(r => r.Email == invitacion.Email);
        //    if (res2 != null) { return Ok(new { result = "Error", detalle = "User with this email found", item = res2 }); }

        //    // ALGUIEN MAS YA INVITO A ESTE EMAIL
        //    //var res3 = _context.Invitacions.FirstOrDefault(r => r.Email == invitacion.Email);
        //    //if (res3 != null) { return Ok(new { result = "Error", detalle = "Someone already invite this email", item = res3 }); }            

        //    _context.Invitacions.Add(invitacion);
        //    _context.SaveChanges();

        //    var usuario = _context.Usuarios.FirstOrDefault(r => r.Id == invitacion.IdUserInvite);
        //    mailController.SentEM_03(invitacion);
        //    return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = new { invitacion.Id, invitacion.Nombre, invitacion.Email, invitacion.FechaEnvio, invitacion.FechaAceptacion, invitacion.IdUserInvite, invitacion.IdStatus } });
        //}

        // PUT: api/Mail/ResentInvitation/{id}
        //[Route("ResentInvitation")]        
        //[HttpPut("{ResentInvitation}/{id}")]
        //public IActionResult ResentInvitation(int id)
        //{
        //    var invitacion = _context.Invitacions.Include(r => r.IdUserInviteNavigation).FirstOrDefault(r => r.Id == id);

        //    /////////////////////////////////  INVITACION NO EXISTE ////////////////////////////////////////////////////
        //    //if (invitacion != null) { return Ok(new { result = "Error", detalle = "Invitation Not Found", item = id }); }
        //    if (invitacion == null) { return NotFound(); }

        //    //////////////////////////////// INVITACION YA FUE ACEPTADA ////////////////////////////////////////////////
        //    if(invitacion != null && invitacion.IdStatus == 1) {
        //        return Ok(new { result = "Error", detalle = "Invitation already acepted", item = id });
        //    }

        //    try
        //    {
        //        mailController.SentEM_03(invitacion);
        //        return Ok(new { result = "Success", detalle = "Invitation sent", item = invitacion.Id });
        //    }
        //    catch (Exception e)
        //    {
        //        return Ok(new { result = "Error", detalle = "Error sending invitation", item = id });
        //    }            
        //}

        // GET: api/Invitacions/GetTest    
        //https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/retrieving-data-using-a-datareader
        //https://www.learnentityframeworkcore.com/raw-sql
        //https://stackoverflow.com/questions/5794529/how-to-use-executereader-method-to-retrieve-the-value-of-just-one-cell
        //[Route("GetTest")]
        //[HttpPut("GetTest")]
        //public IActionResult GetTest()
        //{
        //    string res = "";
        //    using (var command = _context.Database.GetDbConnection().CreateCommand())
        //    {

        //        //command.CommandText = "SELECT * From [db_enso].[dbo].[Usuario]";
        //        //command.CommandText = "SELECT dbo.Usuario.Email, dbo.Usuario.Paterno, dbo.Usuario.Materno, dbo.Usuario.Nombre, COUNT(Usuario_1.Email) AS Expr1 FROM dbo.Usuario LEFT OUTER JOIN dbo.Invitacion ON dbo.Usuario.Id = dbo.Invitacion.IdUserInvite LEFT OUTER JOIN dbo.Usuario AS Usuario_1 ON dbo.Invitacion.Email = Usuario_1.Email AND Usuario_1.Verificado = 1 AND Usuario_1.FechaVerificacion > '2019-07-02 00:00:00.0000000' WHERE(dbo.Usuario.Verificado = 1) AND(dbo.Usuario.Tipo = 1) AND(dbo.Usuario.FechaVerificacion > '2019-07-02 00:00:00.0000000') GROUP BY dbo.Usuario.Email, dbo.Usuario.Paterno, dbo.Usuario.Materno, dbo.Usuario.Nombre";                
        //        command.CommandText = @"SELECT dbo.Usuario.Email, dbo.Usuario.Paterno, dbo.Usuario.Materno, dbo.Usuario.Nombre, COUNT(Usuario_1.Email) AS Expr1
        //              FROM dbo.Usuario LEFT OUTER JOIN dbo.Invitacion ON dbo.Usuario.Id = dbo.Invitacion.IdUserInvite LEFT OUTER JOIN dbo.Usuario AS Usuario_1 ON dbo.Invitacion.Email = Usuario_1.Email AND Usuario_1.Verificado = 1 AND Usuario_1.FechaVerificacion > '2019-07-02 00:00:00.0000000' WHERE(dbo.Usuario.Verificado = 1) AND(dbo.Usuario.Tipo = 1) AND(dbo.Usuario.FechaVerificacion > '2019-07-02 00:00:00.0000000') GROUP BY dbo.Usuario.Email, dbo.Usuario.Paterno, dbo.Usuario.Materno, dbo.Usuario.Nombre";

        //        _context.Database.OpenConnection();

        //        using (var result = command.ExecuteReader())
        //        {
        //            if (result.HasRows)
        //            {
        //                while (result.Read())
        //                {
        //                    //Console.WriteLine("{0}\t{1}", result.GetString(0),result.GetString(1));
        //                    //res = ""+result.FieldCount;
        //                    res += " " + result.GetValue(0) + result.GetValue(1) + result.GetValue(2) + result.GetValue(3) + result.GetValue(4);                            
        //                }
        //            }
        //            else
        //            {
        //                Console.WriteLine("No rows found.");
        //            }
        //            result.Close();
        //        }
        //    }
        //    return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = res });
        //}        
    }
}
