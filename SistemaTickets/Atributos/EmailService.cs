using MailKit.Net.Smtp;
using MimeKit;
namespace SistemaTickets.Atributos
{
    public class EmailService
    {
        public void EnviarCorreoBienvenida(string correoDestino, string contraseñaTemporal)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Soporte Tickets", "gerson7803@gmail.com"));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = "Bienvenido a TicketsTechnology";

            string cuerpoHtml = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                        <h2 style='color: #4c38d5;'>¡Bienvenido a Tickets Technology!</h2>
                        <p>Hola,</p>
                        <p>Tu cuenta ha sido creada correctamente. A continuación, te proporcionamos los datos de acceso:</p>
                        <ul style='line-height: 1.8;'>
                            <li><strong>Correo:</strong> {correoDestino}</li>
                            <li><strong>Contraseña temporal:</strong> {contraseñaTemporal}</li>
                        </ul>
                        <p style='margin-top: 20px;'>Por favor, inicia sesión con esta contraseña y cámbiala lo antes posible desde la sección de perfil de usuario.</p>
                        <p>Si no solicitaste esta cuenta, por favor ignora este correo.</p>
                        <br />
                        <p style='color: #888;'>Atentamente,<br/>Equipo de Tickets Technology</p>
                    </div>
                </body>
                </html>
                ";

            mensaje.Body = new TextPart("html")
            {
                Text = cuerpoHtml
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("gerson7803@gmail.com", "rlsi csng hryr kqiy");
                smtp.Send(mensaje);
                smtp.Disconnect(true);
            }
        }

        public void EnviarCorreoCambioEstado(string correoDestino, string nombreUsuario, int ticketId, string estadoAnterior, string nuevoEstado)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Tickets Technology", "gerson7803@gmail.com"));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = $"Ticket #{ticketId} actualizado a '{nuevoEstado}'";

            string cuerpoHtml = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
              <div style='max-width: 600px; margin: auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                 <h2 style='color: #4c38d5;'>Actualización de Estado de Ticket</h2>
                    <p>Hola {nombreUsuario},</p>
                 <p>Queremos informarte que el estado del <strong>Ticket #{ticketId}</strong> ha sido actualizado.</p>
                 <ul style='line-height: 1.8;'>
                      <li><strong>Estado anterior:</strong> {estadoAnterior}</li>
                       <li><strong>Nuevo estado:</strong> {nuevoEstado}</li>
                    </ul>
                   <p>Por favor, revisa tu panel para más detalles.</p>
                   <br />
                    <p style='color: #888;'>Atentamente,<br/>Equipo de Tickets Technology</p>
              </div>
            </body>
            </html>";

            mensaje.Body = new TextPart("html")
            {
                Text = cuerpoHtml
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("gerson7803@gmail.com", "rlsi csng hryr kqiy"); 
                smtp.Send(mensaje);
                smtp.Disconnect(true);
            }
        }

        public void EnviarCorreoCreacionTicket(string correoDestino, string nombreUsuario, int ticketId, string asunto, string descripcion)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Tickets Technology", "gerson7803@gmail.com")); 
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = $"[Ticket #{ticketId}] Creación Exitosa - {asunto}";

            string cuerpoHtml = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                    <h2 style='color: #4c38d5;'>Ticket creado exitosamente</h2>
                    <p>Hola <strong>{nombreUsuario}</strong>,</p>
                    <p>Tu ticket ha sido creado exitosamente en <strong>Tickets Technology</strong>.</p>
                    <p><strong>Detalles:</strong></p>
                    <ul style='line-height: 1.7;'>
                        <li><strong>ID del Ticket:</strong> #{ticketId}</li>
                        <li><strong>Asunto:</strong> {asunto}</li>
                        <li><strong>Estado Inicial:</strong> Abierto</li>
                        <li><strong>Descripción:</strong> {descripcion}</li>
                        <li><strong>Fecha:</strong> {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</li>
                    </ul>
                    <p>Puedes consultar el estado del ticket desde tu cuenta.</p>
                    <br />
                    <p style='color: #777;'>Gracias por utilizar nuestro sistema,<br/>El equipo de Tickets Technology</p>
                </div>
            </body>
            </html>";

            mensaje.Body = new TextPart("html")
            {
                Text = cuerpoHtml
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("gerson7803@gmail.com", "rlsi csng hryr kqiy");
                smtp.Send(mensaje);
                smtp.Disconnect(true);
            }
        }

        public void EnviarCorreoAsignacionTicket(string correoDestino, string nombreTecnico, int ticketId, string asunto, string descripcion, DateTime fecha)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Tickets Technology", "gerson7803@gmail.com")); 
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = $"[Asignación de Ticket #{ticketId}] - {asunto}";

            string cuerpoHtml = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background: white; padding: 25px; border-radius: 8px; box-shadow: 0 0 8px rgba(0,0,0,0.1);'>
                    <h2 style='color: #4c38d5;'>Nuevo ticket asignado</h2>
                    <p>Hola <strong>{nombreTecnico}</strong>,</p>
                    <p>Se te ha asignado un nuevo ticket en <strong>Tickets Technology</strong>.</p>
                    <p><strong>Detalles del Ticket:</strong></p>
                    <ul style='line-height: 1.6;'>
                        <li><strong>ID:</strong> #{ticketId}</li>
                        <li><strong>Asunto:</strong> {asunto}</li>
                        <li><strong>Descripción:</strong> {descripcion}</li>
                        <li><strong>Fecha de asignación:</strong> {fecha:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    <p>Por favor, ingresa al sistema para atender este ticket lo antes posible.</p>
                    <br />
                    <p style='color: #888;'>Gracias,<br />Equipo de Tickets Technology</p>
                </div>
            </body>
            </html>";

            mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("gerson7803@gmail.com", "rlsi csng hryr kqiy"); 
                smtp.Send(mensaje);
                smtp.Disconnect(true);
            }
        }



    }
}
