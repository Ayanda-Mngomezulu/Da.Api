namespace Login_InvalidPassword_ReturnsFalse
{
    [Fact]
    public void Login_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(repo => repo.ValidateUser("testuser", "wrongpassword"))
                .Returns(false);
        var userService = new UserService(mockRepo.Object);

        // Act
        var result = userService.Login("testuser", "wrongpassword");

        // Assert
        Assert.False(result);
    }
