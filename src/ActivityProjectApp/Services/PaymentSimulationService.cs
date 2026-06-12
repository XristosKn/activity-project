using ActivityProjectApp.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ActivityProjectApp.Services
{
    public class PaymentSimulationService
    {
        private readonly HttpListener _httpListener = new HttpListener();

        private bool _isRunning = false;

        public event Action<int, EnrollmentStatus>? PaymentStatusChanged;

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            if (_httpListener.Prefixes.Count == 0)
            {
                _httpListener.Prefixes.Add("http://localhost:5055/");
            }

            try
            {
                _httpListener.Start();
                _isRunning = true;

                Task.Run(ListenForRequests);
            }
            catch (HttpListenerException)
            {
                _isRunning = false;
            }
        }

        public void OpenPaymentPage(int enrollmentId)
        {
            Start();

            string paymentUrl =
                $"http://localhost:5055/payment?enrollmentId={enrollmentId}&t={DateTime.Now.Ticks}";

            OpenUrl(paymentUrl);
        }

        private void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);
            }
            catch
            {
                try
                {
                    Process.Start("xdg-open", url);
                }
                catch
                {
                    // Ignore browser open errors for demo mode.
                }
            }
        }

        private async Task ListenForRequests()
        {
            while (_isRunning)
            {
                try
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();

                    Task.Run(() => HandleRequest(context));
                }
                catch
                {
                    _isRunning = false;
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url?.AbsolutePath ?? string.Empty;
            string enrollmentIdText = context.Request.QueryString["enrollmentId"] ?? string.Empty;

            if (!int.TryParse(enrollmentIdText, out int enrollmentId))
            {
                SendHtmlResponse(context, CreateErrorPage("Invalid enrollment id."));
                return;
            }

            if (path == "/payment")
            {
                SendHtmlResponse(context, CreatePaymentPage(enrollmentId));
                return;
            }

            if (path == "/payment-confirmed")
            {
                AppServices.EnrollmentRepository.MarkAsConfirmed(enrollmentId);

                PaymentStatusChanged?.Invoke(enrollmentId, EnrollmentStatus.Confirmed);

                SendHtmlResponse(
                    context,
                    CreateResultPage(
                        "Payment Confirmed",
                        "Your payment was confirmed successfully.",
                        "#16A34A"));

                return;
            }

            if (path == "/payment-cancelled")
            {
                AppServices.EnrollmentRepository.CancelEnrollment(enrollmentId);

                PaymentStatusChanged?.Invoke(enrollmentId, EnrollmentStatus.Cancelled);

                SendHtmlResponse(
                    context,
                    CreateResultPage(
                        "Payment Cancelled",
                        "Your payment was cancelled.",
                        "#DC2626"));

                return;
            }

            SendHtmlResponse(context, CreateErrorPage("Page not found."));
        }

        private void SendHtmlResponse(HttpListenerContext context, string html)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(html);

            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;

            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private string CreatePaymentPage(int enrollmentId)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Payment Simulation</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background: #F3F6FA;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }}

        .card {{
            background: white;
            padding: 36px;
            border-radius: 18px;
            width: 440px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.12);
            text-align: center;
        }}

        h1 {{
            color: #111827;
        }}

        p {{
            color: #4B5563;
        }}

        .buttons {{
            margin-top: 28px;
            display: flex;
            gap: 14px;
            justify-content: center;
        }}

        a {{
            text-decoration: none;
            color: white;
            padding: 13px 18px;
            border-radius: 10px;
            font-weight: bold;
        }}

        .confirm {{
            background: #2563EB;
        }}

        .confirm:hover {{
            background: #1D4ED8;
        }}

        .cancel {{
            background: #DC2626;
        }}

        .cancel:hover {{
            background: #B91C1C;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <h1>Payment Simulation</h1>
        <p>Enrollment ID: {enrollmentId}</p>
        <p>This page simulates an external payment provider.</p>

        <div class='buttons'>
            <a class='confirm' href='/payment-confirmed?enrollmentId={enrollmentId}'>Confirm Payment</a>
            <a class='cancel' href='/payment-cancelled?enrollmentId={enrollmentId}'>Cancel Payment</a>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateResultPage(string title, string message, string color)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{title}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background: #F3F6FA;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }}

        .card {{
            background: white;
            padding: 36px;
            border-radius: 18px;
            width: 440px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.12);
            text-align: center;
        }}

        h1 {{
            color: {color};
        }}

        p {{
            color: #4B5563;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <h1>{title}</h1>
        <p>{message}</p>
        <p>You can now return to the desktop application.</p>
    </div>
</body>
</html>";
        }

        private string CreateErrorPage(string message)
        {
            return CreateResultPage("Payment Error", message, "#DC2626");
        }
    }
}