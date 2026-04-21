using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;
using System;
using System.Linq;
using System.Reflection;

const bool SHOW_FULL_TOKEN = false;

string ftUser = @"DESKTOP-C2JQV6K\DevOps";   // or just "DevOps"
string ftPassword = "PUT_PASSWORD_HERE";

Console.WriteLine("=== FTSP TOKEN TROUBLESHOOT START ===");

try
{
    var serviceClient = ClientFactory.GetServiceApiClientV2("FTSP_Token_Test", 46520);

    Console.WriteLine();
    Console.WriteLine("--- CLIENT SESSION INFO ---");
    try
    {
        var session = serviceClient.GetClientSessionInfo();
        Console.WriteLine($"DescriptiveClientName: {session.DescriptiveClientName}");
        Console.WriteLine($"ClientId: {session.ClientId}");
        Console.WriteLine($"ClientUsername: {session.ClientUsername}");
        Console.WriteLine($"ClientFullName: {session.ClientFullName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not read client session info.");
        Console.WriteLine(ex.ToString());
    }

    Console.WriteLine();
    Console.WriteLine("--- FT LOGIN STATUS BEFORE ---");
    try
    {
        Console.WriteLine($"IsFactoryTalkUserLoggedIn: {serviceClient.IsFactoryTalkUserLoggedIn()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not check IsFactoryTalkUserLoggedIn().");
        Console.WriteLine(ex.ToString());
    }

    Console.WriteLine();
    Console.WriteLine("--- TRYING TO LOCATE FactoryTalkServicesPlatformLogin ---");

    TryLoadAssembly("RockwellAutomation.FactoryTalkLogixEcho.Api");
    TryLoadAssembly("RockwellAutomation.FactoryTalkLogixEcho.Api.Client");
    TryLoadAssembly("RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces");

    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
        .OrderBy(a => a.GetName().Name)
        .ToList();

    foreach (var asm in loadedAssemblies)
    {
        Console.WriteLine($"Loaded Assembly: {asm.GetName().Name}");
    }

    var loginType =
        AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("RockwellAutomation.FactoryTalkLogixEcho.Api.FactoryTalkServicesPlatformLogin", false))
            .FirstOrDefault(t => t != null);

    if (loginType == null)
    {
        Console.WriteLine();
        Console.WriteLine("FactoryTalkServicesPlatformLogin type was NOT found.");
        Console.WriteLine("This means the current runtime does not expose that class through the assemblies available to this app.");
        Console.WriteLine("Stopping here.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Found type: {loginType.FullName}");
    Console.WriteLine($"Assembly: {loginType.Assembly.FullName}");

    var loginInstance = Activator.CreateInstance(loginType);
    if (loginInstance == null)
    {
        Console.WriteLine("Could not create FactoryTalkServicesPlatformLogin instance.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("--- TRYING GetTokenForUser(user, password) ---");

    var getTokenForUser = loginType.GetMethod("GetTokenForUser", new[] { typeof(string), typeof(string) });
    if (getTokenForUser == null)
    {
        Console.WriteLine("GetTokenForUser(string, string) method not found.");
        return;
    }

    string? token = null;

    try
    {
        token = getTokenForUser.Invoke(loginInstance, new object[] { ftUser, ftPassword }) as string;
        Console.WriteLine("GetTokenForUser() call returned without throwing.");
    }
    catch (TargetInvocationException tie)
    {
        Console.WriteLine("GetTokenForUser() threw TargetInvocationException.");
        Console.WriteLine("--- INNER EXCEPTION ---");
        Console.WriteLine(tie.InnerException?.ToString() ?? "(none)");
        return;
    }
    catch (Exception ex)
    {
        Console.WriteLine("GetTokenForUser() threw.");
        Console.WriteLine(ex.ToString());
        return;
    }

    Console.WriteLine();
    Console.WriteLine("--- TOKEN RESULT ---");
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("Token was null or empty.");
        return;
    }

    Console.WriteLine($"Token length: {token.Length}");
    Console.WriteLine($"Masked token: {MaskToken(token)}");

    if (SHOW_FULL_TOKEN)
    {
        Console.WriteLine("Full token:");
        Console.WriteLine(token);
    }

    Console.WriteLine();
    Console.WriteLine("--- TRYING LoginFactoryTalkUser(token) ---");

    try
    {
        serviceClient.LoginFactoryTalkUser(token);
        Console.WriteLine("LoginFactoryTalkUser(token) returned without throwing.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("LoginFactoryTalkUser(token) threw.");
        Console.WriteLine(ex.ToString());
    }

    Console.WriteLine();
    Console.WriteLine("--- FT LOGIN STATUS AFTER ---");
    try
    {
        Console.WriteLine($"IsFactoryTalkUserLoggedIn: {serviceClient.IsFactoryTalkUserLoggedIn()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not check IsFactoryTalkUserLoggedIn() after login attempt.");
        Console.WriteLine(ex.ToString());
    }

    Console.WriteLine();
    Console.WriteLine("--- TRYING LogoutFactoryTalk() ---");
    try
    {
        serviceClient.LogoutFactoryTalk();
        Console.WriteLine("LogoutFactoryTalk() returned without throwing.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("LogoutFactoryTalk() threw.");
        Console.WriteLine(ex.ToString());
    }

    Console.WriteLine();
    Console.WriteLine("=== FTSP TOKEN TROUBLESHOOT END ===");
}
catch (Exception ex)
{
    Console.WriteLine("=== TOP-LEVEL ERROR ===");
    Console.WriteLine(ex.ToString());
}

static void TryLoadAssembly(string assemblyName)
{
    try
    {
        Assembly.Load(assemblyName);
        Console.WriteLine($"Assembly.Load succeeded: {assemblyName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Assembly.Load failed: {assemblyName}");
        Console.WriteLine($"  {ex.Message}");
    }
}

static string MaskToken(string token)
{
    if (string.IsNullOrEmpty(token))
        return "(empty)";

    if (token.Length <= 12)
        return token;

    return $"{token[..6]}...{token[^6..]}";
}