using Umbraco.Core.Services.Implement;
using Xunit;


public class UserServiceTests
{
[Fact]
public void Login_ValidUser_ReturnsTrue()
{
var service = new UserService();
var username = "testuser";
var password = "Password123";
var result = service.Login(username, password);
Assert.True(result);
}


[Fact]
public void SubmitIncident_ValidInput_ReturnsIncidentId()
{
var service = new IncidentService();
var incident = new global::Incident { Title = "Test", Description = "Testing" };
var result = service.SubmitIncident(incident);
Assert.NotNull(result);
Assert.IsType<int>(result);
}
}
