using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAccessTokenManagement();
// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).AddUserAccessTokenHandler()
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler()
    {
        UseProxy = false
    };
});
//adduseraccesstokenhandler makes sure that the access token is passed on each request to the API that is made by our
//API client

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Authentication/AccessDenied";
})
//This configures the cookie handler and it enables
//our application to use cookie based authentication for our default scheme
//This means that once an identity token is validated and transformed inti a claims identity,
//it will be stored in an encrypted cookie, and that cookie is then used on subsequent requests to the web app to check
//whether we are making an authenticated request
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => 
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = "https://localhost:5001/";
    options.ClientId = "imagegalleryclient";
    options.ClientSecret = "secret";
    options.ResponseType = "code"; //middleware automatically enable pkce protection when response type is code
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.ClaimActions.Remove("aud"); //removes the aud claim filter
    options.ClaimActions.DeleteClaim("sid"); //deletes the given claim
    options.ClaimActions.DeleteClaim("idp");

    options.Scope.Add("roles");
    options.ClaimActions.MapJsonKey("role", "role");
    options.TokenValidationParameters = new()
    {
        NameClaimType = "given_name",
        RoleClaimType = "role"
    };

    //options.Scope.Add("imagegalleryapi.fullaccess");
    options.Scope.Add("imagegalleryapi.read");
    options.Scope.Add("imagegalleryapi.write");

    options.Scope.Add("country");
    options.ClaimActions.MapUniqueJsonKey("country", "country");

    options.Scope.Add("offline_access");
    //options.Scope.Add("openid"); enable to openid by default
    //options.Scope.Add("profile"); enable to profile by default
    //options.CallbackPath = new PathString("signin-oidc"); we do not have to set it here as we have already set the path in
    //iDP config file. added here so that we know that we can add and update it here
    // SignedOutCallbackPath: default = host:port/signout-callback-oidc.
    // Must match with the post logout redirect URI at IDP client config if
    // you want to automatically return to the application after logging out
    // of IdentityServer.
    // To change, set SignedOutCallbackPath
    // eg: options.SignedOutCallbackPath = new PathString("pathaftersignout");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserCanAddImage", AuthorizationPolicies.CanAddImage());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
