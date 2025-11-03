using System.Security.Claims;

namespace CalculateGrandTotal_CorrectValues
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            [Fact]
            public void CalculateGrandTotal_CorrectValues()
            {
                // Arrange
                var claims = new List<Claim>
    {
        new Claim { Amount = 100 },
        new Claim { Amount = 200 },
        new Claim { Amount = 50 }
    };
                var service = new ClaimService();

                // Act
                var total = service.CalculateGrandTotal(claims);

                // Assert
                Assert.Equal(350, total);
            }

        }
    }
}