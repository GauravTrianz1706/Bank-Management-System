using Xunit;
using System;
using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.EntityModel.Tests
{
    public class PersonModelTests
    {
        [Fact]
        public void PersonModel_ShouldBeInstantiable()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.NotNull(person);
            Assert.IsType<PersonModel>(person);
        }

        [Fact]
        public void Username_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.Username);
        }

        [Fact]
        public void Username_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedUsername = "testuser123";

            // Act
            person.Username = expectedUsername;

            // Assert
            Assert.Equal(expectedUsername, person.Username);
        }

        [Fact]
        public void FullName_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.FullName);
        }

        [Fact]
        public void FullName_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedFullName = "John Doe";

            // Act
            person.FullName = expectedFullName;

            // Assert
            Assert.Equal(expectedFullName, person.FullName);
        }

        [Fact]
        public void Password_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.Password);
        }

        [Fact]
        public void Password_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedPassword = "securePassword123";

            // Act
            person.Password = expectedPassword;

            // Assert
            Assert.Equal(expectedPassword, person.Password);
        }

        [Fact]
        public void Email_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.Email);
        }

        [Fact]
        public void Email_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedEmail = "test@example.com";

            // Act
            person.Email = expectedEmail;

            // Assert
            Assert.Equal(expectedEmail, person.Email);
        }

        [Fact]
        public void Phone_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.Phone);
        }

        [Fact]
        public void Phone_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedPhone = "1234567890";

            // Act
            person.Phone = expectedPhone;

            // Assert
            Assert.Equal(expectedPhone, person.Phone);
        }

        [Fact]
        public void Nid_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.Nid);
        }

        [Fact]
        public void Nid_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedNid = "NID123456789";

            // Act
            person.Nid = expectedNid;

            // Assert
            Assert.Equal(expectedNid, person.Nid);
        }

        [Fact]
        public void Address_ShouldHaveDefaultEmptyString()
        {
            // Arrange & Act
            var person = new PersonModel();

            // Assert
            Assert.Equal(string.Empty, person.Address);
        }

        [Fact]
        public void Address_ShouldBeSettable()
        {
            // Arrange
            var person = new PersonModel();
            var expectedAddress = "123 Main Street, City, Country";

            // Act
            person.Address = expectedAddress;

            // Assert
            Assert.Equal(expectedAddress, person.Address);
        }

        [Fact]
        public void PersonModel_ShouldInitializeWithAllProperties()
        {
            // Arrange & Act
            var person = new PersonModel
            {
                Username = "johndoe",
                FullName = "John Doe",
                Password = "password123",
                Email = "john@example.com",
                Phone = "1234567890",
                Nid = "NID123",
                Address = "123 Main St"
            };

            // Assert
            Assert.Equal("johndoe", person.Username);
            Assert.Equal("John Doe", person.FullName);
            Assert.Equal("password123", person.Password);
            Assert.Equal("john@example.com", person.Email);
            Assert.Equal("1234567890", person.Phone);
            Assert.Equal("NID123", person.Nid);
            Assert.Equal("123 Main St", person.Address);
        }

        [Theory]
        [InlineData("user1", "User One", "pass1", "user1@test.com", "1111111111", "NID1", "Address 1")]
        [InlineData("user2", "User Two", "pass2", "user2@test.com", "2222222222", "NID2", "Address 2")]
        [InlineData("user3", "User Three", "pass3", "user3@test.com", "3333333333", "NID3", "Address 3")]
        public void PersonModel_ShouldAcceptVariousPropertyValues(string username, string fullName, string password, string email, string phone, string nid, string address)
        {
            // Arrange & Act
            var person = new PersonModel
            {
                Username = username,
                FullName = fullName,
                Password = password,
                Email = email,
                Phone = phone,
                Nid = nid,
                Address = address
            };

            // Assert
            Assert.Equal(username, person.Username);
            Assert.Equal(fullName, person.FullName);
            Assert.Equal(password, person.Password);
            Assert.Equal(email, person.Email);
            Assert.Equal(phone, person.Phone);
            Assert.Equal(nid, person.Nid);
            Assert.Equal(address, person.Address);
        }

        [Fact]
        public void PersonModel_AllProperties_ShouldBeOfTypeString()
        {
            // Arrange
            var personType = typeof(PersonModel);

            // Act & Assert
            Assert.Equal(typeof(string), personType.GetProperty("Username")?.PropertyType);
            Assert.Equal(typeof(string), personType.GetProperty("FullName")?.PropertyType);
            Assert.Equal(typeof(string), personType.GetProperty("Password")?.PropertyType);
            Assert.Equal(typeof(string), personType.GetProperty("Email")?.PropertyType);
            Assert.Equal(typeof(string), personType.GetProperty("Phone")?.PropertyType);
            Assert.Equal(typeof(string), personType.GetProperty("Nid")?.PropertyType);
            Assert.Equal(typeof(string), personType.GetProperty("Address")?.PropertyType);
        }

        [Fact]
        public void PersonModel_ShouldHaveSevenProperties()
        {
            // Arrange
            var personType = typeof(PersonModel);

            // Act
            var properties = personType.GetProperties();

            // Assert
            Assert.Equal(7, properties.Length);
        }

        [Fact]
        public void PersonModel_Properties_ShouldAllowNullAssignment()
        {
            // Arrange
            var person = new PersonModel();

            // Act
            person.Username = null!;
            person.FullName = null!;
            person.Password = null!;
            person.Email = null!;
            person.Phone = null!;
            person.Nid = null!;
            person.Address = null!;

            // Assert
            Assert.Null(person.Username);
            Assert.Null(person.FullName);
            Assert.Null(person.Password);
            Assert.Null(person.Email);
            Assert.Null(person.Phone);
            Assert.Null(person.Nid);
            Assert.Null(person.Address);
        }
    }
}
