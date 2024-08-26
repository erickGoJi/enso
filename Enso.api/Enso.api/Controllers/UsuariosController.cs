using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Enso.dal.DBContext;
using Enso.api.Services;
using Enso.api.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Enso.api.Utility;

namespace Enso.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly Db_EnsoContext _context;
        private IConfiguration _config;
        private IEmailService _emailRepository;
        private MailController mailController;

        public UsuariosController(Db_EnsoContext context, IEmailService emailRepository, IConfiguration config)
        {
            _context = context;
            _config = config;
            mailController = new MailController(emailRepository);
        }

        // GET: api/Usuarios
        [HttpGet]
        public IActionResult GetUsuarios([FromQuery] int? id = null, [FromQuery]string email = null, [FromQuery]string codigo = null, [FromQuery]bool? verificado = null)
        {
            // At least one search parameter
            if (!id.HasValue && email == null && codigo == null && !verificado.HasValue) {
                return BadRequest();
            }
           
            var res = _context.Usuarios.Select(r => new
            {
                r.Id, r.Nombre, r.Paterno, r.Phone, r.Email, r.Tipo, r.FechaRegistro, r.Codigo, r.Verificado, r.UsuarioPerfil.Monto, UsuarioPerfilId = r.UsuarioPerfil.Id
            });

            if (id.HasValue) { res = res.Where(r => r.Id == id); }
            if (email != null) { res = res.Where(r => r.Email == email); }                        
            if (codigo != null) { res = res.Where(r => r.Codigo == codigo); }
            if (verificado.HasValue) { res = res.Where(r => r.Verificado == verificado); }

            var jsonRes = JsonConvert.SerializeObject(res);
            var encryptedResponse = Protection.Encrypt(jsonRes, _config);            
            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = encryptedResponse });
        }
        
        // GET: api/Usuarios/IsSuscrito
        [Route("IsSuscrito")]
        [HttpGet("{IsSuscrito}")]
        public IActionResult IsSuscrito([FromQuery] string code)
        {
            bool res = _context.Usuarios.Any(r => r.Codigo == code && r.Verificado);
            //bool isSuscrito = (res.FirstOrDefault().Codigo != null && res.FirstOrDefault().Verificado);
            bool isSuscrito = true;
            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = isSuscrito });
        }


        // GET: api/Usuarios/GetReferidos
        [Route("GetReferidos")]
        [HttpGet("{GetReferidos}")]
        public IActionResult GetReferidos([FromQuery] int idUser)
        {
            var res = (from u in _context.Usuarios
                       join inv in _context.Invitacions on u.Email equals inv.Email
                       where inv.IdStatus == 1 && inv.IdUserInvite == idUser && u.Verificado   // Status 1 = Invitacion Usada                                    
                       select new { u.Id, u.Nombre, u.Paterno, u.Materno, u.Phone, u.Email, u.Tipo, u.FechaRegistro, u.Codigo, u.Verificado }
                     ); ;

            if (res.Count() <= 0) { return NotFound(); }

            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = res });
        }

        // GET: api/Usuarios/CanInvite
        [Route("CanInvite")]
        [HttpGet("CanInvite")]
        public IActionResult CanInvite([FromQuery] int? idUser = null, [FromQuery] string codigo = null)
        {
            Usuario usuario = null;
            if (codigo != null && !idUser.HasValue) { usuario = _context.Usuarios.FirstOrDefault(r => r.Codigo == codigo); }
            if (idUser.HasValue) { usuario = _context.Usuarios.FirstOrDefault(r => r.Id == idUser); }

            if (usuario == null) { return NotFound(); }

            int MaxInvAcept = _context.Cvars.FirstOrDefault().MaxInvAcept;
            int countInvitesUser = _context.Invitacions.Count(r => r.IdUserInvite == usuario.Id);
            //int countInvitesAceptUser = _context.Invitacions.Count(r => r.IdUserInvite == idUserInvite && r.IdStatus == 1);
            if (countInvitesUser >= MaxInvAcept) { return Ok(new { result = "Error", detalle = "Max Invitations limit reached", item = new { usuario, canInvite = false } }); }
            
            return Ok(new { result = "Success", detalle = "Consulta realizada con éxito", item = new { usuario, canInvite = true} });
        }

        // POST: api/Usuarios/{tipo} = 0 SIN CÓDIGO/INVITACIÓN, 1 TRAS INVITACIÓN/CÓDIGO     
        // POST: api/Usuarios/{codigo}
        [HttpPost("{codigo?}")]
        public IActionResult PostUsuario(string codigo, [FromBody] Usuario usuario)
        {            
            ///////////////// FORMA DE REGISTRO NO VALIDA ////////////////////////////////////            
            int tipo = usuario.Tipo;
            if (tipo != 0 && tipo != 1) { return NotFound(); }            

            ///////////////// Intento registrar un usuario ya registrado ////////////////////////////////////
            if (_context.Usuarios.Count(r=> r.Email == usuario.Email) > 0) {
                var user = _context.Usuarios.FirstOrDefault(r => r.Email == usuario.Email);

                // Fix loop when serializing for UsuarioPerfil
                Usuario userFix = new Usuario();
                userFix.Id = user.Id;
                userFix.Codigo = user.Codigo;
                userFix.Nombre = user.Nombre;
                userFix.Paterno = user.Paterno;
                userFix.Materno = user.Materno;
                userFix.Tipo = user.Tipo;
                userFix.Phone = user.Phone;
                userFix.FechaRegistro = user.FechaRegistro;
                userFix.FechaVerificacion = user.FechaVerificacion;
                userFix.Verificado = user.Verificado;
                userFix.UsuarioPerfilId = user.UsuarioPerfilId;
                userFix.Email = user.Email;
                var jsonRes = JsonConvert.SerializeObject(userFix);
                var encryptedResponse = Protection.Encrypt(jsonRes, _config);
                return Ok(new { result = "Error", detalle = "Usuario ya registrado", item = encryptedResponse });
            }

            usuario.UsuarioPerfilId = 3; // PERFIL NORMAL = 3
            //usuario.Tipo = tipo;
            //usuario.Codigo = Guid.NewGuid().ToString();
            /* El código se genera directamente en el INSERT de la columna Codigo de la tabla Usuarios de la BD
             * con la función ([dbo].[fnBase36]((1)))
             */

            if (tipo == 0)  // SIN CÓDIGO/INVITACIÓN
            {
                _context.Usuarios.Add(usuario);                
            }
            else if(tipo == 1)  // TRAS CÓDIGO/INVITACIÓN
            {
                ///////////////// VALIDAR CODIGO INVITACIÓN EXISTE ////////////////////////////////////
                if (_context.Usuarios.Count(r => r.Codigo == codigo) <= 0) {
                    return Ok(new { result = "Error", detalle = "Invitation Code not found", item = codigo });
                }
                
                var codigoUserInvite = codigo;
                int idUserInvite = _context.Usuarios.FirstOrDefault(r => r.Codigo == codigoUserInvite).Id;
                var invitacion = _context.Invitacions.FirstOrDefault(r => r.IdUserInvite == idUserInvite && r.Email == usuario.Email);

                /////////// VALIDAR CANTIDAD INVITACIONES POR ESE USUARIO NO SEA MAYOR AL MaxInvAcept  --- MaxInvAcept FROM  CVars (Database)                 
                int MaxInvAcept = _context.Usuarios.Include(r => r.UsuarioPerfil).FirstOrDefault(r => r.Codigo == codigo).UsuarioPerfil.InviteLimit;
                //int countInvitesUser = _context.Invitacions.Count(r => r.IdUserInvite == idUserInvite);
                int countInvitesUser = (from u in _context.Usuarios
                           join inv in _context.Invitacions on u.Email equals inv.Email
                           where inv.IdStatus == 1 && inv.IdUserInvite == idUserInvite && u.Verificado   // Status 1 = Invitacion Usada                                    
                           select u.Id
                     ).Count();                
                if (countInvitesUser >= MaxInvAcept && MaxInvAcept != -1) { return Ok(new { result = "Error", detalle = "Max Invitations limit reached", item = "" }); }

                if (invitacion == null)             // REGISTRO CON CODIGO PERO NO SE HABIA HECHO UNA INVITACIÓN DENTRO DEL SITIO
                {
                    Invitacion newInvitacion = new Invitacion();                    
                    newInvitacion.IdUserInvite = idUserInvite;
                    newInvitacion.Nombre = usuario.Nombre;
                    newInvitacion.Email = usuario.Email;
                    newInvitacion.FechaAceptacion = DateTime.Now;
                    newInvitacion.IdStatus = 1;
                    _context.Invitacions.Add(newInvitacion);
                    _context.Usuarios.Add(usuario);
                }
                else                                // REGISTRO CON CODIGO Y YA SE HABIA HECHO INVITACIÓN DENTRO DEL SITIO
                {
                    if (invitacion.IdStatus == 1)       // Invitación Usada
                    {
                        var user = _context.Usuarios.FirstOrDefault(r => r.Email == usuario.Email);
                        // Fix loop when serializing for UsuarioPerfil
                        Usuario userFix = new Usuario();
                        userFix.Id = user.Id;
                        userFix.Codigo = user.Codigo;
                        userFix.Nombre = user.Nombre;
                        userFix.Paterno = user.Paterno;
                        userFix.Materno = user.Materno;
                        userFix.Tipo = user.Tipo;
                        userFix.Phone = user.Phone;
                        userFix.FechaRegistro = user.FechaRegistro;
                        userFix.FechaVerificacion = user.FechaVerificacion;
                        userFix.Verificado = user.Verificado;
                        userFix.UsuarioPerfilId = user.UsuarioPerfilId;
                        userFix.Email = user.Email;

                        var jsonRes = JsonConvert.SerializeObject(userFix);
                        //var jsonRes = JsonConvert.SerializeObject(user);
                        var encryptedResponse = Protection.Encrypt(jsonRes, _config);
                        return Ok(new { result = "Error", detalle = "Usuario ya registrado", item = encryptedResponse });
                    }                    

                    if (invitacion.IdStatus == 0)    // Invitación Enviada
                    {
                        invitacion.FechaAceptacion = DateTime.Now;
                        invitacion.IdStatus = 1;
                        _context.Invitacions.Update(invitacion);
                        _context.Usuarios.Add(usuario);
                    }
                }                
            }

            try
            {
                _context.SaveChanges();                

                /* Generación del codigo directo en servicio con base en el ID de Usuario, 
                 * en vez de con la funcion seteada en el deafault en la columna Codigo en la BD */
                //usuario.Codigo = Base36.Encode(usuario.Id + 50000);
                //_context.Usuarios.Update(usuario);
                //_context.SaveChanges();

                mailController.SentEM_01(usuario);
                //mailController.SentEM_02(usuario);                

                // Fix loop when serializing for UsuarioPerfil
                Usuario userFix = new Usuario();
                userFix.Id = usuario.Id;
                userFix.Codigo = usuario.Codigo;
                userFix.Nombre = usuario.Nombre;
                userFix.Paterno = usuario.Paterno;
                userFix.Materno = usuario.Materno;
                userFix.Tipo = usuario.Tipo;
                userFix.Phone = usuario.Phone;
                userFix.FechaRegistro = usuario.FechaRegistro;
                userFix.FechaVerificacion = usuario.FechaVerificacion;
                userFix.Verificado = usuario.Verificado;
                userFix.UsuarioPerfilId = usuario.UsuarioPerfilId;
                userFix.Email = usuario.Email;                

                var jsonRes = JsonConvert.SerializeObject(userFix);
                //var jsonRes = JsonConvert.SerializeObject(usuario);
                var encryptedResponse = Protection.Encrypt(jsonRes, _config);
                return Ok(new { result = "Success", detalle = "Usuario Registrado Exitosamente", item = encryptedResponse });
            } catch (Exception e)
            {
                return Ok(new { result = "Error", detalle = "User Register Fail", item = e });
            }
            
        }

        //[Route("VerifyUser")]
        [Route("VerifyUser")]
        [HttpPut("VerifyUser/{code}")]        
        //public IActionResult VerifyUser(string code, [FromBody] string response)
        public IActionResult VerifyUser([FromRoute] string code)
        {
            var user = _context.Usuarios.FirstOrDefault(r => r.Codigo == code);

            ////////////////////////// No existe ningun usuario registrado con ese código //////////////////////////////////////
            if (user == null) {
                return NotFound();
            }

            ////////////////////////// Usuario intenta verificar y ya esta verificado //////////////////////////////////////////
            if (user.Verificado) {

                Usuario userFix = new Usuario();
                userFix.Id = user.Id;
                userFix.Codigo = user.Codigo;
                userFix.Nombre = user.Nombre;
                userFix.Paterno = user.Paterno;
                userFix.Materno = user.Materno;
                userFix.Tipo = user.Tipo;
                userFix.Phone = user.Phone;
                userFix.FechaRegistro = user.FechaRegistro;
                userFix.FechaVerificacion = user.FechaVerificacion;
                userFix.Verificado = user.Verificado;
                userFix.UsuarioPerfilId = user.UsuarioPerfilId;
                userFix.Email = user.Email;

                var jsonRes = JsonConvert.SerializeObject(userFix);
                //var jsonRes = JsonConvert.SerializeObject(user);
                var encryptedResponse = Protection.Encrypt(jsonRes, _config);
                return Ok(new { result = "Error", detalle = "Already Verified", item = encryptedResponse });
            }

            ////////////////////////// Fallo en la respuesta de verificación //////////////////////////////////////////
            //if (response != user.Email) {
            //    return Ok(new { result = "Error", detalle = "Verification question wrong", item = 0 });
            //}

            /////////// VALIDAR CANTIDAD INVITACIONES POR ESE USUARIO NO SEA MAYOR AL MaxInvAcept  --- MaxInvAcept FROM  CVars (Database) 
            if (user.Tipo == 1) // TRAS CÓDIGO/INVITACIÓN
            {
                var idUserInvite = _context.Invitacions.FirstOrDefault(r => r.Email == user.Email).IdUserInvite;                
                int MaxInvAcept = _context.Usuarios.Include(r => r.UsuarioPerfil).FirstOrDefault(r => r.Id == idUserInvite).UsuarioPerfil.InviteLimit;
                int countInvitesUser = (from u in _context.Usuarios
                                        join inv in _context.Invitacions on u.Email equals inv.Email
                                        where inv.IdStatus == 1 && inv.IdUserInvite == idUserInvite && u.Verificado   // Status 1 = Invitacion Usada                                    
                                        select u.Id
                     ).Count();

                if (countInvitesUser >= MaxInvAcept && MaxInvAcept != -1) {
                    var invitation = _context.Invitacions.FirstOrDefault(r => r.Email == user.Email);
                    _context.Invitacions.Remove(invitation);
                    _context.Usuarios.Remove(user);

                    try
                    {
                        _context.SaveChanges();
                        return Ok(new { result = "Error", detalle = "Max Invitations limit reached" });
                    }
                    catch { return Ok(new { result = "Error", detalle = "User Verification Fail"}); }
                }                
            }

            user.Verificado = true;
            user.FechaVerificacion = DateTime.Now;
            _context.Usuarios.Update(user);
            
            try
            {
                _context.SaveChanges();
                mailController.SentEM_02(user);

                // Enviar Correo a la persona que me invito, notificando que ya me registre
                if (user.Tipo == 1) // Usuario por Invitación/Código
                {
                    int userInviteId = _context.Invitacions.FirstOrDefault(r => r.Email == user.Email).IdUserInvite;
                    Usuario userinvites = _context.Usuarios.Find(userInviteId); // Usuario quien me invito
                    AvisoRegistro aviso = new AvisoRegistro(user, userinvites);
                    mailController.SentEM_06(aviso);
                }

                Usuario userFix = new Usuario();
                userFix.Id = user.Id;
                userFix.Codigo = user.Codigo;
                userFix.Nombre = user.Nombre;
                userFix.Paterno = user.Paterno;
                userFix.Materno = user.Materno;
                userFix.Tipo = user.Tipo;
                userFix.Phone = user.Phone;
                userFix.FechaRegistro = user.FechaRegistro;
                userFix.FechaVerificacion = user.FechaVerificacion;
                userFix.Verificado = user.Verificado;
                userFix.UsuarioPerfilId = user.UsuarioPerfilId;
                userFix.Email = user.Email;

                var jsonRes = JsonConvert.SerializeObject(userFix);
                //var jsonRes = JsonConvert.SerializeObject(user);
                var encryptedResponse = Protection.Encrypt(jsonRes, _config);
                return Ok(new { result = "Success", detalle = "Usuario Verificado", item = encryptedResponse });
            } catch (Exception e)
            {
                return Ok(new { result = "Error", detalle = "User Verification Fail", item = 0 });
            }            
        }

        // PUT: api/Usuarios/ResentVerification/{code}
        [Route("ResentVerification")]
        [HttpPut("ResentVerification/{id}")]
        public IActionResult ResentVerification(int id)
        {
            var user = _context.Usuarios.FirstOrDefault(r => r.Id == id);

            ////////////////////////// No existe ningun usuario registrado con ese código //////////////////////////////////////
            if (user == null) { return NotFound(); }

            ////////////////////////// Usuario intenta verificar y ya esta verificado //////////////////////////////////////////
            if (user.Verificado) { return Ok(new { result = "Error", detalle = "Already Verified", item = 0 }); }

            try
            {
                mailController.SentEM_01(user);
                return Ok(new { result = "Success", detalle = "Verification Resent", item = id });
            }
            catch (Exception e)
            {
                return Ok(new { result = "Error", detalle = "Error sending verification", item = id });
            }
        }       
    }
}
