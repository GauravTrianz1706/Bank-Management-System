using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem;
using BankDatabaseAccess.EntityModel;

namespace BankManagementSystem.Tests
{
    public class EmployeeDashBoardTests
    {
        [Fact]
        public void Constructor_WithValidEmployee_InitializesForm()
        {
            // Arrange
            var employee = new EmployeeModel
            {
                Username = "testemployee",
                Password = "TestPassword1"
            };

            // Act
            var form = new EmployeeDashBoard(employee);

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void EmployeeDashBoard_IsFormType()
        {
            // Arrange
            var employee = new EmployeeModel
            {
                Username = "testemployee",
                Password = "TestPassword1"
            };

            // Act
            var form = new EmployeeDashBoard(employee);

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Constructor_WithNullEmployee_ThrowsException()
        {
            // Act
            Action act = () => new EmployeeDashBoard(null);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void EmployeeDashBoard_CanBeInstantiated()
        {
            // Arrange
            var employee = new EmployeeModel
            {
                Username = "testemployee",
                Password = "TestPassword1"
            };

            // Act
            Action act = () => new EmployeeDashBoard(employee);

            // Assert
            act.Should().NotThrow();
        }
    }
}
