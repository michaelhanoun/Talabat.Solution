namespace Talabat.Core.Services.Contract
{
    public interface ISendEmail
    {
         Task SendMail(string recipients , string subject, string body);
    }
}
