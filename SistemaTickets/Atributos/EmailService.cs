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
    }
}
