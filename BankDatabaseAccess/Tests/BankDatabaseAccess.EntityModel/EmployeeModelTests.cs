using Xunit;
using System;
using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.EntityModel.Tests
{
    public class EmployeeModelTests
    {
        [Fact]
        public void EmployeeModel_ShouldInheritFromPersonModel()
        {
            // Arrange & Act
            var employee = new EmployeeModel();

            // Assert
            Assert.IsAssignableFrom<PersonModel>(employee);
        }

        [Fact]
        public void Salary_ShouldHaveDefaultValue()
        {
            // Arrange & Act
            var employee = new EmployeeModel();

            // Assert
            Assert.Equal(0m, employee.Salary);
        }

        [Fact]
        public void Salary_ShouldBeReadOnly()
        {
            // Arrange
            var employee = new EmployeeModel();

            // Act & Assert
            // Verify that Salary has a private setter by checking it cannot be set publicly
            var salaryProperty = typeof(EmployeeModel).GetProperty("Salary");
            Assert.NotNull(salaryProperty);
            Assert.True(salaryProperty!.CanRead);
            Assert.False(salaryProperty.SetMethod?.IsPublic ?? false);
        }

        [Fact]
        public void EmployeeModel_ShouldHaveUsernameProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedUsername = "empuser";

            // Act
            employee.Username = expectedUsername;

            // Assert
            Assert.Equal(expectedUsername, employee.Username);
        }

        [Fact]
        public void EmployeeModel_ShouldHaveFullNameProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedFullName = "Jane Smith";

            // Act
            employee.FullName = expectedFullName;

            // Assert
            Assert.Equal(expectedFullName, employee.FullName);
        }

        [Fact]
        public void EmployeeModel_ShouldHavePasswordProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedPassword = "employeePass123";

            // Act
            employee.Password = expectedPassword;

            // Assert
            Assert.Equal(expectedPassword, employee.Password);
        }

        [Fact]
        public void EmployeeModel_ShouldHaveEmailProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedEmail = "employee@example.com";

            // Act
            employee.Email = expectedEmail;

            // Assert
            Assert.Equal(expectedEmail, employee.Email);
        }

        [Fact]
        public void EmployeeModel_ShouldHavePhoneProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedPhone = "9876543210";

            // Act
            employee.Phone = expectedPhone;

            // Assert
            Assert.Equal(expectedPhone, employee.Phone);
        }

        [Fact]
        public void EmployeeModel_ShouldHaveNidProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedNid = "EMP-NID-789";

            // Act
            employee.Nid = expectedNid;

            // Assert
            Assert.Equal(expectedNid, employee.Nid);
        }

        [Fact]
        public void EmployeeModel_ShouldHaveAddressProperty()
        {
            // Arrange
            var employee = new EmployeeModel();
            var expectedAddress = "456 Corporate Blvd, Business City";

            // Act
            employee.Address = expectedAddress;

            // Assert
            Assert.Equal(expectedAddress, employee.Address);
        }

        [Fact]
        public void EmployeeModel_ShouldInitializeWithAllInheritedProperties()
        {
            // Arrange & Act
            var employee = new EmployeeModel
            {
                Username = "janesmith",
                FullName = "Jane Smith",
                Password = "securePass456",
                Email = "jane@company.com",
                Phone = "9876543210",
                Nid = "EMP789",
                Address = "456 Corporate Blvd"
            };

            // Assert
            Assert.Equal("janesmith", employee.Username);
            Assert.Equal("Jane Smith", employee.FullName);
            Assert.Equal("securePass456", employee.Password);
            Assert.Equal("jane@company.com", employee.Email);
            Assert.Equal("9876543210", employee.Phone);
            Assert.Equal("EMP789", employee.Nid);
            Assert.Equal("456 Corporate Blvd", employee.Address);
        }

        [Fact]
        public void EmployeeModel_ShouldHaveSalaryPropertyOfTypeDecimal()
        {
            // Arrange
            var employee = new EmployeeModel();

            // Act
            var salaryProperty = typeof(EmployeeModel).GetProperty("Salary");

            // Assert
            Assert.NotNull(salaryProperty);
            Assert.Equal(typeof(decimal), salaryProperty!.PropertyType);
        }

        [Fact]
        public void EmployeeModel_ShouldNotAllowPublicSalarySetting()
        {
            // Arrange
            var employee = new EmployeeModel();
            var salaryProperty = typeof(EmployeeModel).GetProperty("Salary");

            // Act
            var setMethod = salaryProperty?.GetSetMethod();

            // Assert
            Assert.Null(setMethod); // Public setter should not exist
        }

        [Fact]
        public void EmployeeModel_InheritedProperties_ShouldHaveDefaultEmptyStrings()
        {
            // Arrange & Act
            var employee = new EmployeeModel();

            // Assert
            Assert.Equal(string.Empty, employee.Username);
            Assert.Equal(string.Empty, employee.FullName);
            Assert.Equal(string.Empty, employee.Password);
            Assert.Equal(string.Empty, employee.Email);
            Assert.Equal(string.Empty, employee.Phone);
            Assert.Equal(string.Empty, employee.Nid);
            Assert.Equal(string.Empty, employee.Address);
        }

        [Fact]
        public void EmployeeModel_ShouldBeInstantiable()
        {
            // Arrange & Act
            var employee = new EmployeeModel();

            // Assert
            Assert.NotNull(employee);
            Assert.IsType<EmployeeModel>(employee);
        }
    }
}
