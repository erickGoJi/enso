using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Enso.api.Models;
using Enso.api.Services;
using Enso.dal.DBContext;

namespace Enso.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private IEmailService _emailRepository;
        //private string URL = "http://localhost:4200/";
        private string URL = "https://tarjeta.joinenso.com/";

        public MailController(IEmailService emailRepository)
        {            
            _emailRepository = emailRepository;
        }       

        // GET: api/Mail
        [HttpGet]
        public IActionResult SentMail()
        {
            var path = Path.GetFullPath("TemplateMail/VerifyMail.html");
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/VerifyMail.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", "Hola");
            body = body.Replace("{pass}", "passsss");

            try
            {
                Email newmail = new Email();
                newmail.To = "xuxysmile@hotmail.com,ursula@minimalist.mx,jorge@minimalist.mx,israelon_83@hotmail.com,omar.velasco@minimalist.mx";
                newmail.Subject = "AvisoPorRegistro08";
                newmail.Body = body;
                newmail.IsBodyHtml = true;
                _emailRepository.SendEmail(newmail);

                return Ok("Correo enviado");
            }
            catch (Exception ex)
            {
                return Ok("Ocurrio un problema");
            }            
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////                       

        public void SentEM_01(Usuario usuario)
        {
            string URLVerify = URL+"confirmregister/" + usuario.Codigo;

            var path = Path.GetFullPath("TemplateMail/EM_01_Verify.html");
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EM_01_Verify.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", usuario.Nombre);
            body = body.Replace("{URLVerifiy}", URLVerify);

            try
            {
                Email newmail = new Email();
                newmail.To = usuario.Email;
                //newmail.To = "miguel@minimalist.mx";
                newmail.Subject = "Confirma tu correo";
                newmail.Body = body;
                newmail.IsBodyHtml = true;
                _emailRepository.SendEmail(newmail);                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void SentEM_02(Usuario usuario)
        {            
            string URLStatus = URL + "status/" + usuario.Codigo;
            string URLInvite = URL + "registro/" + usuario.Codigo;

            var path = Path.GetFullPath("TemplateMail/EM_02_Codigo.html");
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EM_02_Codigo.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", usuario.Nombre);
            body = body.Replace("{code}", usuario.Codigo);
            body = body.Replace("{URLStatus}", URLStatus);
            body = body.Replace("{URLInvite}", URLInvite);

            try
            {
                Email newmail = new Email();
                newmail.To = usuario.Email;
                //newmail.To = "miguel@minimalist.mx";
                newmail.Subject = "¡Ya tienes código! Compártelo y gana $500 pesos";
                newmail.Body = body;
                newmail.IsBodyHtml = true;
                _emailRepository.SendEmail(newmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void SentEM_03(Invitacion invitacion)
        {
            //http:/localhost:4200/registro/AAA?email=mike@gmail.com&name=mike
            string URLInvite = URL + "registro/" + invitacion.IdUserInviteNavigation.Codigo;
            URLInvite = URLInvite + "?email=" + invitacion.Email;
            URLInvite = URLInvite + "&name=" + invitacion.Nombre;

            var path = Path.GetFullPath("TemplateMail/EM_03_Invitacion.html");
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EM_03_Invitacion.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            //body = body.Replace("{username}", invitacion.Email);
            body = body.Replace("{username}", invitacion.Nombre);
            body = body.Replace("{inviterName}", invitacion.IdUserInviteNavigation.Nombre);
            body = body.Replace("{URLInvite}", URLInvite);

            try
            {
                Email newmail = new Email();
                newmail.To = invitacion.Email;
                //newmail.To = "miguel@minimalist.mx";
                newmail.Subject = "Invitation";
                newmail.Body = body;
                newmail.IsBodyHtml = true;
                _emailRepository.SendEmail(newmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void SentEM_04(Usuario usuario)
        {
            string URLVerify = URL + "confirmregister/" + usuario.Codigo;

            var path = Path.GetFullPath("TemplateMail/EM_04_Verify.html");
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EM_04_Verify.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", usuario.Nombre);
            body = body.Replace("{URLVerifiy}", URLVerify);

            try
            {
                Email newmail = new Email();
                newmail.To = usuario.Email;
                //newmail.To = "miguel@minimalist.mx";
                newmail.Subject = "Confirma tu correo";
                newmail.Body = body;
                newmail.IsBodyHtml = true;
                _emailRepository.SendEmail(newmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void SentEM_06(AvisoRegistro aviso)
        {
            string username = aviso.username.Nombre;
            string userinvites = aviso.userinvites.Nombre;
            string code = aviso.userinvites.Codigo;
            string URLInvite = URL + "registro/" + aviso.userinvites.Codigo;

            var path = Path.GetFullPath("TemplateMail/EM_06_AvisoPorRegistro.html");
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EM_06_AvisoPorRegistro.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", username);
            body = body.Replace("{userinvites}", userinvites);
            body = body.Replace("{code}", code);
            body = body.Replace("{URLInvite}", URLInvite);

            try
            {
                Email newmail = new Email();
                newmail.To = aviso.userinvites.Email;
                //newmail.To = "miguel@minimalist.mx";
                newmail.Subject = "Tu amig@ se registró con tu código";
                newmail.Body = body;
                newmail.IsBodyHtml = true;
                _emailRepository.SendEmail(newmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}