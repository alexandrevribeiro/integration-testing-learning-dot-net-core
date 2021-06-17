using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TennisBookings.Web.Core;
using TennisBookings.Web.IntegrationTests.Extensions;
using TennisBookings.Web.IntegrationTests.Fakes;
using TennisBookings.Web.IntegrationTests.Helpers;
using Xunit;

namespace TennisBookings.Web.IntegrationTests.Pages
{
    public class EnquiryPageTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public EnquiryPageTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_SendsEmail()
        {
            var emailService = new FakeEmailService();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IEmailService>(emailService);
                });
            }).CreateClient();

            // ASP.NET Core uses "Antiforgery Tokens" to prevent CSRF attacks.
            // These are included in forms and as a cookie. 
            // So, just making a POST request will fail since it doesn't have the token
            // neither the cookie... instead, it's necessary to GET the form page and 
            // use to make a POST request (the cookies will be automatically handled by the client)

            var getResponse = await client.GetAsync("enquiry");
            using var content = await HtmlHelpers.GetDocumentAsync(getResponse);
            var form = (IHtmlFormElement)content.QuerySelector("form");

            if (form["Email"] is IHtmlInputElement email)
                email.Value = "example@example.com";

            if (form["Subject"] is IHtmlInputElement subject)
                subject.Value = "Testing";

            if (form["Message"] is IHtmlTextAreaElement message)
                message.Value = "This is a test message.";

            var button = (IHtmlButtonElement)content.QuerySelector("button");
            var formSubmission = form.GetSubmission(button);
            var target = (Uri)formSubmission.Target;

            var request = new HttpRequestMessage(new HttpMethod(formSubmission.Method.ToString()), target)
            {
                Content = new StreamContent(formSubmission.Body)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);

            response.AssertOk();

            Assert.Single(emailService.SentEmails);
        }
    }
}
