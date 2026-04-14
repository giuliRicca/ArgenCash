namespace ArgenCash.Infrastructure.Email;

public static class EmailTemplates
{
    public static string GetVerificationEmail(string verificationToken, string frontendUrl)
    {
        var verifyLink = $"{frontendUrl.TrimEnd('/')}/verify?token={Uri.EscapeDataString(verificationToken)}";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>ArgenCash</h1>
    </div>
    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='margin-top: 0; color: #333;'>Verifica tu correo electrónico</h2>
        <p>Gracias por registrarte en ArgenCash. Por favor, haz clic en el botón de abajo para verificar tu correo electrónico:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{verifyLink}' style='background: #667eea; color: white; padding: 14px 30px; text-decoration: none; border-radius: 5px; font-weight: 600; display: inline-block;'>Verificar correo</a>
        </div>
        <p style='font-size: 14px; color: #666;'>Este enlace expira en 15 minutos.</p>
        <p style='font-size: 14px; color: #666;'>Si no solicitaste este correo, puedes ignorarlo.</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
        <p style='font-size: 12px; color: #999; margin: 0;'>© 2026 ArgenCash</p>
    </div>
</body>
</html>";
    }
}