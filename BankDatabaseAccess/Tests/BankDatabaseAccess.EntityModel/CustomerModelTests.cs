using Xunit;
using System;
using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.EntityModel.Tests
{
    public class CustomerModelTests
    {
        [Fact]
        public void CustomerModel_ShouldInheritFromPersonModel()
        {
            // Arrange & Act
            var customer = new CustomerModel();

            // Assert
            Assert.IsAssignableFrom<PersonModel>(customer);
        }

        [Fact]
        public void Balance_ShouldHaveDefaultValue()
        {
            // Arrange & Act
            var customer = new CustomerModel();

            // Assert
            Assert.Equal(0m, customer.Balance);
        }

        [Fact]
        public void Balance_ShouldBeSettable()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedBalance = 1000.50m;

            // Act
            customer.Balance = expectedBalance;

            // Assert
            Assert.Equal(expectedBalance, customer.Balance);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(1000000.99)]
        [InlineData(-500.25)]
        public void Balance_ShouldAcceptVariousDecimalValues(decimal balance)
        {
            // Arrange
            var customer = new CustomerModel();

            // Act
            customer.Balance = balance;

            // Assert
            Assert.Equal(balance, customer.Balance);
        }

        [Fact]
        public void CustomerModel_ShouldHaveUsernameProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedUsername = "testuser";

            // Act
            customer.Username = expectedUsername;

            // Assert
            Assert.Equal(expectedUsername, customer.Username);
        }

        [Fact]
        public void CustomerModel_ShouldHaveFullNameProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedFullName = "John Doe";

            // Act
            customer.FullName = expectedFullName;

            // Assert
            Assert.Equal(expectedFullName, customer.FullName);
        }

        [Fact]
        public void CustomerModel_ShouldHavePasswordProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedPassword = "securePassword123";

            // Act
            customer.Password = expectedPassword;

            // Assert
            Assert.Equal(expectedPassword, customer.Password);
        }

        [Fact]
        public void CustomerModel_ShouldHaveEmailProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedEmail = "test@example.com";

            // Act
            customer.Email = expectedEmail;

            // Assert
            Assert.Equal(expectedEmail, customer.Email);
        }

        [Fact]
        public void CustomerModel_ShouldHavePhoneProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedPhone = "1234567890";

            // Act
            customer.Phone = expectedPhone;

            // Assert
            Assert.Equal(expectedPhone, customer.Phone);
        }

        [Fact]
        public void CustomerModel_ShouldHaveNidProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedNid = "NID123456";

            // Act
            customer.Nid = expectedNid;

            // Assert
            Assert.Equal(expectedNid, customer.Nid);
        }

        [Fact]
        public void CustomerModel_ShouldHaveAddressProperty()
        {
            // Arrange
            var customer = new CustomerModel();
            var expectedAddress = "123 Main St, City, Country";

            // Act
            customer.Address = expectedAddress;

            // Assert
            Assert.Equal(expectedAddress, customer.Address);
        }

        [Fact]
        public void CustomerModel_ShouldInitializeWithAllProperties()
        {
            // Arrange & Act
            var customer = new CustomerModel
            {
                Username = "johndoe",
                FullName = "John Doe",
                Password = "password123",
                Email = "john@example.com",
                Phone = "1234567890",
                Nid = "NID123",
                Address = "123 Main St",
                Balance = 5000.00m
            };

            // Assert
            Assert.Equal("johndoe", customer.Username);
            Assert.Equal("John Doe", customer.FullName);
            Assert.Equal("password123", customer.Password);
            Assert.Equal("john@example.com", customer.Email);
            Assert.Equal("1234567890", customer.Phone);
            Assert.Equal("NID123", customer.Nid);
            Assert.Equal("123 Main St", customer.Address);
            Assert.Equal(5000.00m, customer.Balance);
        }

        [Fact]
        public void Balance_ShouldSupportLargeValues()
        {
            // Arrange
            var customer = new CustomerModel();
            var largeBalance = decimal.MaxValue;

            // Act
            customer.Balance = largeBalance;

            // Assert
            Assert.Equal(largeBalance, customer.Balance);
        }

        [Fact]
        public void Balance_ShouldSupportNegativeValues()
        {
            // Arrange
            var customer = new CustomerModel();
            var negativeBalance = -1000.50m;

            // Act
            customer.Balance = negativeBalance;

            // Assert
            Assert.Equal(negativeBalance, customer.Balance);
        }
    }
}
