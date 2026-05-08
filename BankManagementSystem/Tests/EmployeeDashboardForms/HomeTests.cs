using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.EmployeeDashboardForms;
using BankDatabaseAccess.EntityModel;

namespace BankManagementSystem.Tests.EmployeeDashboardForms
{
    public class HomeTests
    {
        [Fact]
        public void Constructor_WithValidEmployee_InitializesForm()
        {
            // Arrange
            var employee = new EmployeeModel
            {
                Username = "testemployee",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            var form = new Home(employee);

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void Home_IsFormType()
        {
            // Arrange
            var employee = new EmployeeModel
            {
                Username = "testemployee",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            var form = new Home(employee);

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Constructor_WithNullEmployee_ThrowsException()
        {
            // Act
            Action act = () => new Home(null);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Home_CanBeInstantiated()
        {
            // Arrange
            var employee = new EmployeeModel
            {
                Username = "testemployee",
                Password = "TestPassword1",
                Email = "test@example.com"
            };

            // Act
            Action act = () => new Home(employee);

            // Assert
            act.Should().NotThrow();
        }
    }
}
